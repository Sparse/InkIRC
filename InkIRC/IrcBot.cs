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
        public delegate void PingRecieved(string pPing);
        public event PingRecieved OnPingRecieved;

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
            ClientSocket.OnDataReceived += clientSocket_OnDataReceived;
            ClientSocket.OnConnectionFailed += clientSocket_OnConnectionFailed;
            ClientSocket.OnDisconnect += ClientSocket_OnDisconnect;
            ClientSocket.OnConnected += ClientSocket_OnConnected;
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
            ClientSocket.OnDataReceived += clientSocket_OnDataReceived;
            ClientSocket.OnConnectionFailed += clientSocket_OnConnectionFailed;
            ClientSocket.Connect();
        }

        private static int FindLineTerminator(byte[] pArray, int pStart, int pDataLength, ref int pEOLLength)
        {
            if (pDataLength <= 1) return -1;
            for (int index = 0; index < (pDataLength - 1); ++index)
            {
                if (pArray[pStart + index] == '\n' || (pArray[pStart + index] == '\r' && pArray[pStart + index + 1] == '\n'))
                {
                    pEOLLength = 1;
                    if (pArray[pStart + index] == '\r') ++pEOLLength;
                    return pStart + index;
                }
            }
            return -1;
        }

      
        #region EventHandlers

        void ClientSocket_OnConnected()
        {
            Thread.Sleep(1000);
            ClientSocket.Send(Encoding.ASCII.GetBytes("USER InkIRC 8 * :InkIRC"));
            ClientSocket.Send(Encoding.ASCII.GetBytes("NICK InkBot"));
        } 

        void ClientSocket_OnDisconnect()
        {
            throw new NotImplementedException(); //fuck it, worry about it later...
        }

        private void clientSocket_OnDataReceived(byte[] pArray, int pRecievedDataLength)
        {
            do // This kind of loop, means it will ALWAYS run once, before testing the while condition at the end
            {
                int eolLength = 0; // eol terminator length
                int eol = FindLineTerminator(pArray, 0, pRecievedDataLength, ref eolLength); // Find the end of line, and obtain the length of the eol terminator
                if (eol < 0) break; // If there is no end of line, bail out, and wait for more data
                string line = Encoding.ASCII.GetString(pArray, 0, eol); // Convert the bytes to ASCII string, upto eol (end of line, before terminators, can be 0 if it's an empty line)
                Log.Write(line, MessageType.Server); // Pass off the message to the event, don't care what happens now, just cleanup buffer for next line after this
                pRecievedDataLength -= (eol + eolLength); // Remove length of line, plus length of line terminator, so we start at the next line
                if (pRecievedDataLength > 0) Buffer.BlockCopy(pArray, (eol + eolLength), pArray, 0, pRecievedDataLength); // If we didn't use all the data, copy what's left to the start
            } while (pRecievedDataLength > 0); // If there is still data, after parsing a line, try to parse another
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


        #endregion


    }
}
