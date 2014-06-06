using System;
using System.Threading;

namespace InkIRC
{
    class Program
    {
        static void Main(string[] args)
        {

            IrcBot ircClient = new IrcBot();
            while (ircClient.Running)
            {

            }
        }
    }
}
