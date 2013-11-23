using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkIRC.LoggingTools
{
    class LogTool
    {
        public void Write(string pMessage, MessageType pMessageType)
        {
            switch (pMessageType)
            {
                case MessageType.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case MessageType.Welcome:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case MessageType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case MessageType.Channel:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case MessageType.Server:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case MessageType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case MessageType.PrivateMessage:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case MessageType.ClientMessage:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;                    
                default:                
                break;
            }                  
            Console.WriteLine("[" + DateTime.Now.ToShortTimeString() + "]" + " (" + pMessageType + ") " + ">>> " + pMessage);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
