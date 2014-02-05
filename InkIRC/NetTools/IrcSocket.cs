using System;
using System.Collections;
using System.Net.Sockets;

namespace InkIRC.NetTools
{
    class IrcSocket
    {
        public delegate void SocketExceptionOccured(IrcSocket pIrcSocket);
        public event SocketExceptionOccured OnSocketException;

        public string Host { get; set; }
        public string SocketMessage { get; private set; }
        public ushort Port { get; set; }


        private Socket mSocket;

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
                OnSocketException(this);
                SocketMessage = e.ToString();
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
                Connect();
                SocketMessage = e.ToString(); 
                throw;
            }
        }

    }
}
