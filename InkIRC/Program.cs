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
            NetTools.IrcSocket client = new NetTools.IrcSocket();

            log.Write("Welcome to InkIRC!!", MessageType.Welcome);
            client.Connect();
            Console.Read();         
        }
    }
}
