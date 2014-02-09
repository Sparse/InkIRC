using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkIRC
{

    class IrcBot
    {
        public LoggingTools.LogTool log = new LoggingTools.LogTool();
        public NetTools.IrcSocket clientSocket;

        private string mHost;
        private int mPort;
        bool mRestarted = false;

        public IrcBot()
        {
            StartBot();
        }

        private void StartBot()
        {
            if (!mRestarted) log.Write("Welcome to InkIRC!!", MessageType.Welcome);
            mRestarted = false;
            log.Write("Please input a host IP", MessageType.Welcome);
            log.Write("Host: ", MessageType.Info);
            clientSocket = new NetTools.IrcSocket(log);
            clientSocket.Host = Console.ReadLine();
            clientSocket.Port = 6667;
            mHost = clientSocket.Host;
            mPort = clientSocket.Port;
            clientSocket.Connect();
        }

        private void Restart()
        {
            clientSocket = new NetTools.IrcSocket(log);
            clientSocket.Host = mHost;
            clientSocket.Port = mPort;
            clientSocket.Connect();
        }
        
        
        
        private void clientSocket_OnConnectionFailed(int pCode)
        {
            if (pCode != 0)
            {
                log.Write("Fatal Error Occured! Terminating", MessageType.Error);
                Console.Read();
                Environment.Exit(0);
            }
            else
            {
                //log.Write(e.Message, MessageType.Error);
                //log.Write("Connection to the specified host " + Host + " has failed", MessageType.Error);
                //log.Write("Should I attempt to reconnect? y|n", MessageType.Info);

                //string response = Console.ReadLine();

                //switch (response)
                //{
                //    case "y":
                //        mRestarted = true;
                //        clientSocket = null;
                //        Restart();
                //        break;
                //    case "n":
                //        Environment.Exit(0);
                //        break;
                //    case "yes":
                //        mRestarted = true;
                //        clientSocket = null;          
                //        Restart();
                //        break;
                //    case "no":
                //        Environment.Exit(0);
                //        break;
                //    default:
                //        log.Write("Please input a valid response!", MessageType.Error);
                //        clientSocket_OnConnectionFailed(0);
                //        break;
                //
                Restart();
            }


        }

        private void clientSocket_OnDataReceived(byte[] pArray)
        {
            throw new NotImplementedException();
        }
    }
}
