using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

namespace InkIRC.NetTools
{
    class IrcSocket
    {
        public delegate void ServerDataReceived(byte[] pArray, int pRecievedDataLength);
        public delegate void ConnectFailed(int pCode); //pcode is just a place holder...later on, this will need to be expanded for reason codes.
        public delegate void SocketDisconnect(); //need to pass something...
        public delegate void ConnectionSucceded();
        
        public event ServerDataReceived OnDataReceived;
        public event ConnectFailed OnConnectionFailed;
        public event SocketDisconnect OnDisconnect;
        public event ConnectionSucceded OnConnected;

        public string Host { get; set; }
        public int Port { get; set; }

        private LoggingTools.LogTool mLog;
        private byte[] mSocketRecieveBuffer = new byte[512];
        private byte[] mSocketSendBuffer = new byte[256];
        private int mReceivedDataLength = 0;
        private Socket mSocket;

        public IrcSocket(LoggingTools.LogTool pLog)
        {
            mLog = pLog;
        }

        public void Connect()
        {
            try
            {
                if (mSocket != null) throw new Exception("Socket is already connected! Aborting");

                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSocket.BeginConnect(Host, Port, EndConnect, null);
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
                OnConnected();
                BeginReceive();
            }
            catch (Exception e)
            {
                mSocket = null;
                OnConnectionFailed(0);
            }
        }

        private void BeginReceive()
        {
            mSocket.BeginReceive(mSocketRecieveBuffer, mReceivedDataLength, mSocketRecieveBuffer.Length - mReceivedDataLength, SocketFlags.None, EndReceive, null);
        }

        private void EndReceive(IAsyncResult pIAsyncResult)
        {
            int receivedData;
            SocketError dataError;
           
            receivedData = mSocket.EndReceive(pIAsyncResult, out dataError);
            if (receivedData <= 0)
            {                 
                mSocket.Close();
                mSocket = null;
                OnConnectionFailed(0);//the numbers are going to be reason codes...eventually
                mLog.Write(dataError.ToString(), MessageType.Error);
                return;
            }

            mReceivedDataLength += receivedData;
            OnDataReceived(this.mSocketRecieveBuffer, mReceivedDataLength);
            BeginReceive();
        }

        public void Send(byte[] pData)
        {
            mSocketSendBuffer = pData;
            Buffer.BlockCopy(pData, 0, mSocketSendBuffer, 0, pData.Length);
            mSocket.BeginSend(mSocketSendBuffer, 0, mSocketSendBuffer.Length, SocketFlags.None, EndSend, null);
        }

        private void EndSend(IAsyncResult pIAsyncResult)
        {
            SocketError sendError;
            try
            {
                mSocket.EndSend(pIAsyncResult, out sendError);
            }
            catch (Exception)
            {
                OnDisconnect();                
            }
        }


        

        #region Events

      

        #endregion

    }
}
