using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static System.TimeSpan;
using static CultureInfoAndThreads.Helpers;

namespace CultureInfoAndThreads
{
    public class TcpTest
    {
        private readonly IPAddress ip;
        private readonly int port;
        private readonly TcpListener listener;
        private List<Task> connectedClients;

        public TcpTest()
        {
            port = 8888;
            ip = IPAddress.Loopback;
            listener = new TcpListener(ip, port);
        }

        public void StartTcpServer()
        {
            using (new SimpleConsoleService(
                startAction: listener.Start,
                workAction: ct =>
                {
                    var server = AcceptClientAsync(ct);
                    while (!ct.IsCancellationRequested)
                    {
                        server.Wait(FromSeconds(1));
                    };
                },
                stopAction: listener.Stop
            )) { }
        }

        private async Task AcceptClientAsync(CancellationToken ct)
        {
            connectedClients = new List<Task>();

            while (!ct.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync();
                #region log
                var clientEndpoint = client.Client.RemoteEndPoint;
                DumpThreadInfo($"AcceptClientAsync client {clientEndpoint} connected");
                #endregion
                var clientTask = ProcessClientAsync(client, ct);

                connectedClients.Add(clientTask);
                #region log
                DumpThreadInfo($"AcceptClientAsync client {clientEndpoint} accepted, clients connected {connectedClients.Count}");
                #endregion
                var _ = clientTask.ContinueWith(t =>
                {

                    if (t.Exception != null)
                    {
                        #region log
                        DumpThreadInfo($"AcceptClientAsync client {clientEndpoint} crashed {t.Exception}");
                        #endregion
                    }
                    connectedClients.Remove(t);
                    #region log
                    DumpThreadInfo($"AcceptClientAsync client {clientEndpoint} disconnected, clients connected {connectedClients.Count}");
                    #endregion
                });
                #region log
                DumpThreadInfo($"AcceptClientAsync client {clientEndpoint} processed");
                #endregion
            }
        }

        private async Task ProcessClientAsync(TcpClient client, CancellationToken ct)
        {
            var clientEndpoint = client.Client.RemoteEndPoint;

            var clientDisconnected = false;

            using (var networkStream = client.GetStream())
            using (var reader = new StreamReader(networkStream))
            using (var writer = new StreamWriter(networkStream))
            {
                writer.AutoFlush = true;
                while (!clientDisconnected && !ct.IsCancellationRequested)
                {
                    #region log
                    DumpThreadInfo($"ProcessClientAsync processing client {clientEndpoint}");
                    #endregion
                    var request = await reader.ReadLineAsync();

                    if (request == null || request == "disconnect")
                    {
                        clientDisconnected = true;
                        #region log
                        DumpThreadInfo($"ProcessClientAsync client {clientEndpoint} disconnected");
                        #endregion
                        continue;
                    }

                    SetCultureCase2();
                    #region log
                    DumpThreadInfo($"ProcessClientAsync client {clientEndpoint} request [{request}] received");
                    #endregion
                    await writer.WriteLineAsync($"Received {request}");
                    #region log
                    DumpThreadInfo($"ProcessClientAsync client {clientEndpoint} response sent");
                    #endregion
                }
            }
            client.Close();
        }

        public void StartTcpClient()
        {
            Task.WaitAll(StartTcpClientAsync());
        }

        private async Task StartTcpClientAsync()
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(ip, port);

                using (var networkStream = client.GetStream())
                using (var writer = new StreamWriter(networkStream))
                using (var reader = new StreamReader(networkStream))
                {
                    writer.AutoFlush = true;
                    await SendAndReceive(writer, reader, "test1");
                    await SendAndReceive(writer, reader, "test2");
                    await SendAndReceive(writer, reader, "test3");
                }
            }
        }

        private static async Task SendAndReceive(StreamWriter writer, StreamReader reader, string request)
        {
            #region log
            Console.WriteLine($"Sending request [{request}]");
            #endregion
            await writer.WriteLineAsync(request);

            string response = await reader.ReadLineAsync();

            if (response == null)
            {
                #region log
                Console.WriteLine("Server disconnected");
                #endregion
            }
            #region log
            Console.WriteLine($"Response received [{response}]");
            #endregion
        }
    }
}
