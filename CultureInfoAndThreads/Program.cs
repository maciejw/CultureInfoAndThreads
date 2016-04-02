using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CultureInfoAndThreads.Program;

namespace CultureInfoAndThreads
{
    public class EndpointBehavior : IEndpointBehavior
    {
        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher
                .DispatchRuntime.Operations
                .Select(i => i.ParameterInspectors).ToList()
                .ForEach(pi => pi.Add(new ParameterInspector()));
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }
    }

    public class ParameterInspector : IParameterInspector
    {
        void IParameterInspector.AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
        }

        object IParameterInspector.BeforeCall(string operationName, object[] inputs)
        {
            var cultureInfo = new CultureInfo("pl-PL");
            var thread = Thread.CurrentThread;

            thread.CurrentCulture = cultureInfo;
            thread.CurrentUICulture = cultureInfo;

            return null;
        }
    }

    [ServiceContract]
    public interface IMyServiceClient : IClientChannel
    {
        [OperationContract(Action = ServiceAction, ReplyAction = ServiceReplyAction)]
        Task WorkAsync();
    }

    [ServiceContract]
    public interface IMyService
    {
        [OperationContract(Action = ServiceAction, ReplyAction = ServiceReplyAction)]
        void Work();
    }

    public class MyService : IMyService
    {
        public void Work()
        {

            DumpThreadInfo();
        }
    }

    class Program
    {
        public const string ServiceAction = nameof(IMyService.Work);
        public const string ServiceReplyAction = nameof(IMyService.Work) + "Response";

        public static void DumpThreadInfo()
        {
            var thread = Thread.CurrentThread;

            Console.WriteLine($"{thread.ManagedThreadId} {thread.CurrentUICulture} {thread.CurrentCulture}");
        }

        private static NetTcpBinding binding;
        private static EndpointAddress address;
        private static Uri addressUri;

        static void Main(string[] args)
        {
            addressUri = new Uri($"net.tcp://localhost:8888/{nameof(MyService)}");

            address = new EndpointAddress(addressUri);

            binding = new NetTcpBinding();

            switch (args.FirstOrDefault())
            {
                case nameof(StartClient):
                    StartClient();
                    break;
                case nameof(StartServer):
                    StartServer();
                    break;
                default:
                    Console.WriteLine($"Available commands: {nameof(StartServer)}, {nameof(StartClient)}");
                    break;
            }


        }

        private static void StartServer()
        {
            const string quitText = "Press Ctrl+C to quit.";

            ManualResetEventSlim quitEvent = new ManualResetEventSlim();

            AppContext.SetSwitch("Switch.System.Globalization.NoAsyncCurrentCulture", false);

            var serviceHost = new ServiceHost(typeof(MyService));

            var serviceEndpoint = serviceHost.AddServiceEndpoint(typeof(IMyService), binding, addressUri);

            serviceEndpoint.Behaviors.Add(new EndpointBehavior());

            serviceHost.Open();

            DumpThreadInfo();

            Console.WriteLine($"Server started. {quitText}");
            Console.CancelKeyPress += (o, a) =>
            {
                a.Cancel = true;
                quitEvent.Set();
            };

            while (!quitEvent.Wait(TimeSpan.FromSeconds(5)))
            {
                Console.WriteLine($"Waiting for requests... {quitText}");
            };

            serviceHost.Close();

            Console.WriteLine("Server stopped.");
        }

        private static void StartClient()
        {
            Task.WaitAll(ClientCall());
        }

        private async static Task ClientCall()
        {
            IMyServiceClient client = ChannelFactory<IMyServiceClient>.CreateChannel(binding, address);

            client.Open();

            await client.WorkAsync();

            client.Close();
        }
    }
}
