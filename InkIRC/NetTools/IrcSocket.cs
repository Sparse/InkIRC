using System;
using System.Collections;
using System.Net.Sockets;

namespace InkIRC.NetTools
{
    class IrcSocket
    {
        private delegate void SocketExceptionOccured(string pExceptionString);
        private event SocketExceptionOccured OnSocketException;

        public string Host { get; set; }
        public string SocketMessage { get; private set; }
        public ushort Port { get; set; }

        private LoggingTools.LogTool mLog;
        private Socket mSocket;
       

        public IrcSocket(LoggingTools.LogTool pLog)
        {
            mLog = pLog;
            OnSocketException += IrcSocket_OnSocketException;
        }

        public void Connect()
        {
            try
            {
                if (mSocket != null) throw new Exception("Socket is already connected! Aborting");

                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSocket.BeginConnect(Host, (int)Port, EndConnect, null);
            }
            catch (Exception e)
            {
                mSocket = null;
                OnSocketException(SocketMessage = e.Message.ToString());            
                return;
            }
        }

        private void EndConnect(IAsyncResult pIAsyncResult)
        {
            try
            {
                 mSocket.EndConnect(pIAsyncResult);
            }
            catch (Exception e)
            {
                mSocket = null;
                OnSocketException(SocketMessage = e.ToString()); 
                throw;
            }
        }

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

    }
}
