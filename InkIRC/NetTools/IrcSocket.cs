using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;

namespace InkIRC.NetTools
{
    class IrcSocket
    {
        public delegate int ServerDataReceived(byte[] pReceivedData, int pReceivedDataLength, int pStartingIndex);
        public delegate void ConnectFailed(int pCode); 
        public delegate void SocketDisconnect(string pSocketError); //need to pass something...
        public delegate void ConnectionSucceded();
        public delegate void IrcPacketFound();
        
        public event ServerDataReceived OnDataReceived;
        public event ConnectFailed OnConnectionFailed;
        public event SocketDisconnect OnDisconnect;
        public event ConnectionSucceded OnConnected;
        public event IrcPacketFound ValidMessageFound;

        public string Host { get; set; }
        public int Port { get; set; }
        public Socket IrcConnectionSocket { get { return mSocket; } }
        public Queue<string> MessageQueue = new Queue<string>();

        private LoggingTools.LogTool mLog;
        private byte[] mSocketRecieveBuffer = new byte[1024];
        private byte[] mSocketSendBuffer = new byte[256];
        private int mReceivedDataLength = 0;
        private int mStartingIndex = 0;
        private Socket mSocket;

        public IrcSocket(LoggingTools.LogTool pLog)
        {
            mLog = pLog;
            this.OnDataReceived += IrcSocket_OnDataReceived;
        }

        public void Connect()
        {
            try
            {
                if (mSocket != null) throw new Exception("Socket is already connected! Aborting");

                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSocket.BeginConnect(Host, Port, new AsyncCallback(EndConnect), null);
            }
            catch (Exception e)
            {
                mSocket = null;
                OnConnectionFailed(0);
                Connect();
            }
        }

        private void EndConnect(IAsyncResult pIAsyncResult)
        {
            try
            {
                mSocket.EndConnect(pIAsyncResult);
                if (mSocket.Connected)
                {
                    BeginReceive();
                    OnConnected();
                }                
            }
            catch (Exception e)
            {
                mSocket = null;
                OnConnectionFailed(0);
            }
        }

        private void BeginReceive()
        {
            mSocket.BeginReceive(mSocketRecieveBuffer, mReceivedDataLength, mSocketRecieveBuffer.Length - mReceivedDataLength, SocketFlags.None, new AsyncCallback(EndReceive), null);
        }

        private void EndReceive(IAsyncResult pIAsyncResult)
        {
            int receivedData = mSocket.EndReceive(pIAsyncResult);

            if (receivedData <= 0)
            {
                OnDisconnect("Datastream Interrupted");
                return;
            }
            else { mReceivedDataLength += receivedData; }

            while (mReceivedDataLength > 0)
            {
                int consumedBytes = OnDataReceived(mSocketRecieveBuffer, mReceivedDataLength, mStartingIndex);
                if (consumedBytes == 0) break;
                mStartingIndex += consumedBytes;
                mReceivedDataLength -= consumedBytes;
            }

            if (mReceivedDataLength == 0)
            {
                mStartingIndex = 0;
            }
            else if (mStartingIndex > 0)
            {
                Buffer.BlockCopy(mSocketRecieveBuffer, mStartingIndex, mSocketRecieveBuffer, 0, mReceivedDataLength);
                mStartingIndex = 0;
            }

            if (mReceivedDataLength == mSocketRecieveBuffer.Length)
            {
                OnDisconnect(SocketError.NoBufferSpaceAvailable.ToString());
            }
            else { BeginReceive(); }
            OnDataReceived(this.mSocketRecieveBuffer, mReceivedDataLength, 0);
            BeginReceive();
        }

        public void Send(byte[] pData)
        {
            mSocketSendBuffer = pData;
            Buffer.BlockCopy(pData, 0, mSocketSendBuffer, 0, pData.Length);
            mSocket.BeginSend(mSocketSendBuffer, 0, mSocketSendBuffer.Length, SocketFlags.None, new AsyncCallback(EndSend), null);
        }

        private void EndSend(IAsyncResult pIAsyncResult)
        {
            SocketError sendError;
            try
            {
                mSocket.EndSend(pIAsyncResult, out sendError);
            }
            catch (Exception e)
            {
                OnDisconnect(e.Message);                
            }
        }        

        #region Events
        
        private int IrcSocket_OnDataReceived(byte[] pReceivedData, int pReceivedDataLength, int pStartingIndex)
        {
            if (pReceivedDataLength < 2) { return 0; } //Invalid IRC Packet

            int length = 0;
            while (length < pReceivedDataLength && pReceivedData[pStartingIndex + length] != '\n') ++length; //Seek valid IRC Packet boundary

            if (length >= pReceivedDataLength) { return 0; } //No valid packet found return 0 consumed bytes

            int offset = 1;            
            if (pReceivedData[pStartingIndex + (length - 1)] == '\r')
            {
                offset++;
                length--;
                MessageQueue.Enqueue(Encoding.ASCII.GetString(pReceivedData, pStartingIndex, length));
                ValidMessageFound();
            }
            
            //Console.WriteLine(MessageQueue.Dequeue()); //FOR DEBUG PURPOSES ONLY
            return length + offset;
        }      

        #endregion

    }
}
