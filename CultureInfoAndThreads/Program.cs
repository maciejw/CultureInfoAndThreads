using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CultureInfoAndThreads
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Helpers.ForceNoAsyncCurrentCultureFlag();

            var commands = CreateCommands();

            commands.RunCommand(args.FirstOrDefault());
        }

        private static Commands CreateCommands()
        {
            var wcfTest = new WcfTest();
            var tcpTest = new TcpTest();
            var ThreadPoolTest = new ThreadPoolTest();

            var commands = new Commands
            {
                { nameof(ShowFrameworkVersionInfo), ShowFrameworkVersionInfo },
                { nameof(wcfTest.StartWcfClientCase1), wcfTest.StartWcfClientCase1 },
                { nameof(wcfTest.StartWcfClientCase2), wcfTest.StartWcfClientCase2 },
                { nameof(wcfTest.StartWcfClientCase3), wcfTest.StartWcfClientCase3 },
                { nameof(wcfTest.StartWcfClientCase4), wcfTest.StartWcfClientCase4 },
                { nameof(wcfTest.StartWcfServer), wcfTest.StartWcfServer },
                { nameof(ThreadPoolTest.OnlyThreadPool), ThreadPoolTest.OnlyThreadPool },
                { nameof(tcpTest.StartTcpServer), tcpTest.StartTcpServer },
                { nameof(tcpTest.StartTcpClient), tcpTest.StartTcpClient },
            };

            return commands;
        }

        private static void ShowFrameworkVersionInfo()
        {
            DotNetVersion.ShowVersionHeader();
            DotNetVersion.Show45or451FromRegistry();
            DotNetVersion.ShowVersionFromEnvironment();
            DotNetVersion.ShowUpdates();
        }
    }
}
