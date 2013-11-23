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
            log.Write("Welcome to InkIRC!!", MessageType.Welcome);
            Console.Read();         
        }
    }
}
