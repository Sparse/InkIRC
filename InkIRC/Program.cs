using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace InkIRC
{
    class Program
    {
        static void Main(string[] args)
        {
            IrcBot ircClient = new IrcBot();
            while (ircClient.Running)
            {
                //Fuck you my braces rule
                Thread.Sleep(1);
            }
        }
    }
}
