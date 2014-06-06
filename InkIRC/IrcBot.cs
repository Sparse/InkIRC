using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace InkIRC
{

    class IrcBot
    {
        private delegate void PingReceived(string pPingToken);
        private delegate void CommandReceived(string pCommand);

        private event PingReceived OnPingRecieved;
        private event CommandReceived OnCommandReceived;

        public LoggingTools.LogTool Log = new LoggingTools.LogTool();
        public NetTools.IrcSocket ClientSocket;

        public bool Running { get; private set; } // remember to set for Exiting the bot

        private string mHost;
        private int mPort;
        private bool mRestarted = false;
      

        public IrcBot()
        {
            StartBot();
            Running = true;
        }

        private void StartBot()
        {
            if (!mRestarted) Log.Write("Welcome to InkIRC!!", MessageType.Welcome);
            mRestarted = false;
            Log.Write("Please input a host IP", MessageType.Welcome);
            Log.Write("Host: ", MessageType.Info);
            ClientSocket = new NetTools.IrcSocket(Log);

            ClientSocket.OnConnectionFailed += clientSocket_OnConnectionFailed;
            ClientSocket.OnDisconnect += ClientSocket_OnDisconnect;
            ClientSocket.OnConnected += ClientSocket_OnConnected;
            ClientSocket.ValidMessageFound += ClientSocket_ValidMessageFound;
            this.OnPingRecieved += IrcBot_OnPingRecieved;
            this.OnCommandReceived += IrcBot_OnCommandReceived;
            ClientSocket.Host = Console.ReadLine();
            ClientSocket.Port = 6667;
            mHost = ClientSocket.Host;
            mPort = ClientSocket.Port;
            ClientSocket.Connect();
            //while loop for getting data from the console, eventually
        }

        private void Restart()
        {
            ClientSocket = new NetTools.IrcSocket(Log);
            ClientSocket.Host = mHost;
            ClientSocket.Port = mPort;
            ClientSocket.Connect();
        }
      
        #region EventHandlersSocketBased

        void ClientSocket_OnConnected()
        {
            ClientSocket.Send(Encoding.ASCII.GetBytes("USER InkIRC 8 * :InkIRC\r\n"));
            ClientSocket.Send(Encoding.ASCII.GetBytes("NICK InkBot\r\n"));
        } 

        void ClientSocket_OnDisconnect(string pSocketError)
        {
            Log.Write(pSocketError, MessageType.Error);
            ClientSocket.IrcConnectionSocket.Close();
            ClientSocket = null;
            Restart();
        }        

        private void clientSocket_OnConnectionFailed(int pCode)
        {
            if (pCode != 0)
            {
                Log.Write("Fatal Error Occured! Terminating", MessageType.Error);
                Running = false;
            }
            else
            {
                Log.Write("I failed to connect to " + mHost, MessageType.Error);
                Log.Write("Should I retry? y|n", MessageType.Info);
                string response = Console.ReadLine();

                switch (response.ToLower())
                {
                    case "yes":
                    case "y":
                        mRestarted = true;
                        ClientSocket = null;
                        Restart();
                        break;
                    case "n":
                    case "no":
                        mRestarted = true;
                        StartBot();
                        break;
                    default:
                        Log.Write("Please input a valid response!", MessageType.Error);
                        clientSocket_OnConnectionFailed(0);
                        break;
                }

            }
        }

        void ClientSocket_ValidMessageFound()
        {
            StringBuilder decodedMessage = new StringBuilder();
            string message = ClientSocket.MessageQueue.Dequeue();
            string prefix;
            string command;
            string param;
            string trailer;
            int commandEnd;
            int prefixEnd;
            //parse prefix, if exists
            if (message.IndexOf(":") == 0)
            {
                prefixEnd = message.IndexOf(" ");
                prefix = message.Substring(0, prefixEnd);
                message = message.Substring(prefixEnd, message.Length - prefixEnd);
            }

            commandEnd = message.IndexOf(" ");
            command = message.Substring(0, commandEnd);
            if (command == "PING")
            {
                int pingTokenLocation = message.IndexOf(" :");
                OnPingRecieved(message.Substring(pingTokenLocation, message.Length - pingTokenLocation));
            }
            else
            {
                OnCommandReceived(command);
            }
            decodedMessage.Append(message);
            Console.WriteLine(decodedMessage.ToString());
          
        }
        #endregion

        #region EventHandlersClientBased

        void IrcBot_OnPingRecieved(string pPingToken)
        {
            ClientSocket.Send(Encoding.ASCII.GetBytes("PONG" + pPingToken + "\r\n"));
        }

        void IrcBot_OnCommandReceived(string pCommand)
        {
            switch (pCommand)
            {
                case "451":
                    Log.Write("Error 451 encountered", MessageType.Info);
                    break;
                default:
                    break;
            }
        }
        #endregion



    }
}
