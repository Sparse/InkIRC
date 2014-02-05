using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace InkIRC
{
    class Program
    {
        static void Main(string[] args)
        {
            LoggingTools.LogTool log = new LoggingTools.LogTool();
            NetTools.IrcSocket client = new NetTools.IrcSocket(log);

            log.Write("Welcome to InkIRC!!", MessageType.Welcome);
            log.Write("Please input a host IP", MessageType.Welcome);
            log.Write("Host: ", MessageType.Info);
            client.Host = Console.ReadLine();
            client.Port = 6667;
            client.Connect();
            Console.Read();         
        }
    }
}
