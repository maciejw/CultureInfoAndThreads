using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static System.TimeSpan;

namespace CultureInfoAndThreads
{
    public class SimpleConsoleService : IDisposable
    {
        private const string quitText = "Press Ctrl+C to quit.";
        private readonly ManualResetEventSlim quitEvent;
        private readonly Thread quitThread;
        private readonly CancellationTokenSource cancellationTokenSource;

        public SimpleConsoleService(Action startAction = null, Action<CancellationToken> workAction = null, Action stopAction = null)
        {
            quitEvent = new ManualResetEventSlim();
            cancellationTokenSource = new CancellationTokenSource();

            var cancellationToken = cancellationTokenSource.Token;

            quitThread = new Thread(() =>
            {

                while (!quitEvent.Wait(FromSeconds(10)))
                {
                    Console.WriteLine($"{quitText}");
                };

            });

            ConsoleCancelEventHandler cancelKeyPressHandler = (o, a) =>
            {
                a.Cancel = true;
                cancellationTokenSource.Cancel();
                quitEvent.Set();
            };

            startAction?.Invoke();
            Console.WriteLine($"Server started. {quitText}");

            Console.CancelKeyPress += cancelKeyPressHandler;
            quitThread.Start();

            workAction?.Invoke(cancellationToken);

            quitThread.Join();
            Console.CancelKeyPress -= cancelKeyPressHandler;

            stopAction?.Invoke();
            Console.WriteLine("Server stopped.");
        }

        public void Dispose()
        {
            this.quitEvent.Dispose();
            this.cancellationTokenSource.Dispose();
        }
    }






}
