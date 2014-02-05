using System;
using System.Text;
using System.Collections;
using System.Net.Sockets;

namespace InkIRC.NetTools
{
    class IrcSocket
    {
        private delegate void SocketExceptionOccured(string pExceptionString);
        private event SocketExceptionOccured OnSocketException;

        public delegate void ServerDataReceived(byte[] pArray);
        public event ServerDataReceived OnDataReceived; 

        public string Host { get; set; }
        public int Port { get; set; }

        private LoggingTools.LogTool mLog;
        private byte[] mSocketBuffer = new byte[64];
        private int mReceivedDataLength = 0;
        private Socket mSocket;
       

        public IrcSocket(LoggingTools.LogTool pLog)
        {
            mLog = pLog;
            OnSocketException += IrcSocket_OnSocketException;
            //OnDataReceived += IrcSocket_OnDataReceived;
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
                OnSocketException(e.Message.ToString());            
                return;
            }
        }

        private void EndConnect(IAsyncResult pIAsyncResult)
        {
            try
            {
                 mSocket.EndConnect(pIAsyncResult);
                 BeginReceive();
            }
            catch (Exception e)
            {
                mSocket = null;
                OnSocketException(e.ToString());
                return;
            }
        }

        private void BeginReceive()
        {
            mSocket.BeginReceive(mSocketBuffer, mReceivedDataLength, mSocketBuffer.Length - mReceivedDataLength, SocketFlags.None, EndReceive, null);
        }

        private void EndReceive(IAsyncResult pIAsyncResult)
        {
            SocketError socketMessage;
            int receivedData;
            
            try
            {
                receivedData = mSocket.EndReceive(pIAsyncResult, out socketMessage);
            }
            catch (Exception e)
            {
                mSocket.Close();
                mSocket = null;
                OnSocketException(e.ToString());
                return;
            }

            if (receivedData < 0)
            {                 
                mSocket.Close();
                mSocket = null;
                OnSocketException(socketMessage.ToString());
                return;
            }

            mReceivedDataLength += receivedData;
            OnDataReceived(this.mSocketBuffer);
            ConstructReceivedData();
            BeginReceive();
        }

        private void ConstructReceivedData() //just a test method, later abstracted parsing will be implemented
        {

        }

        #region Events

        void IrcSocket_OnSocketException(string pExceptionString)
        { 
            if (!string.IsNullOrEmpty(pExceptionString)) mLog.Write(pExceptionString, MessageType.Error);
            
            mLog.Write("Attempt to reconnect? y/n", MessageType.Info);
            string response = Console.ReadLine();
            switch (response.ToLower())
            {
                case "y":
                    Connect();
                    break;
                case "n":
                    mLog.Write("Exiting", MessageType.Info);
                    Console.Read();
                    break;
                case "yes":
                    Connect();
                    break;
                case "no":
                    mLog.Write("Exiting", MessageType.Info);
                    Console.Read();
                    break;
                case "":
                    Connect();
                    break;
                default:
                    mLog.Write("Please input a proper response", MessageType.Warning);
                    IrcSocket_OnSocketException(null);
                    break;
            }
        }

        //private void IrcSocket_OnDataReceived(byte[] pArray)
        //{
        //    string data = "";
        //    //just a test, to see if I'm getting data
        //    for (int i = 0; i < mReceivedDataLength; i++)
        //    {
        //        data += Encoding.ASCII.GetString(mSocketBuffer, 0, 64);
        //    }
        //    mLog.Write(data, MessageType.Server);
        //}

        #endregion

    }
}
