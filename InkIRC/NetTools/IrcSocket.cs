using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

namespace InkIRC.NetTools
{
    class IrcSocket
    {
        public delegate void ServerDataReceived(byte[] pArray);
        public delegate void ConnectFailed(int pCode);
        
        public event ServerDataReceived OnDataReceived;
        public event ConnectFailed OnConnectionFailed;

        public string Host { get; set; }
        public int Port { get; set; }

        private LoggingTools.LogTool mLog;
        private byte[] mSocketBuffer = new byte[64];
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
                Connect();
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
                OnConnectionFailed(0);
                return;
                //mLog.Write(e.Message, MessageType.Error);
                //mLog.Write("Connection to the specified host " + Host + " has failed", MessageType.Error);
                //mLog.Write("Should I attempt to reconnect? y|n", MessageType.Info);

                //string response = Console.ReadLine();

                //switch (response)
                //{
                //    case "y":
                //        mSocket = null;
                //        OnConnectionFailed(1);
                //        break;
                //    case "n":
                //        Environment.Exit(0);
                //        break;
                //    case "yes":
                //        mSocket = null;
                //        OnConnectionFailed(1);
                //        break;
                //    case "no":
                //        Environment.Exit(0);
                //        break;
                //    default:
                //        OnConnectionFailed(0);
                //        break;
                //}
            }
        }

        private void BeginReceive()
        {
            mSocket.BeginReceive(mSocketBuffer, mReceivedDataLength, mSocketBuffer.Length - mReceivedDataLength, SocketFlags.None, EndReceive, null);
        }

        private void EndReceive(IAsyncResult pIAsyncResult)
        {
            int receivedData;
            
            try
            {
                receivedData = mSocket.EndReceive(pIAsyncResult);
            }
            catch (Exception e)
            {
                mSocket.Close();
                mLog.Write(e.Message, MessageType.Error);
                mSocket = null;                
                return;
            }

            if (receivedData < 0)
            {                 
                mSocket.Close();
                mSocket = null;               
                return;
            }
            mReceivedDataLength += receivedData;
            OnDataReceived(this.mSocketBuffer);
            BeginReceive();
        }



        

        #region Events

      

        #endregion

    }
}
