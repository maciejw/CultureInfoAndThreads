using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;
using static CultureInfoAndThreads.Helpers;

namespace CultureInfoAndThreads
{
    public class WcfTest
    {
        public class TestCultureInfoEndpointBehavior : IEndpointBehavior
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
                    .ForEach(pi => pi.Add(new TestCultureInfoParameterInspector()));
            }

            void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
            {
            }
        }

        public class TestCultureInfoParameterInspector : IParameterInspector
        {
            void IParameterInspector.AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
            {
            }

            object IParameterInspector.BeforeCall(string operationName, object[] inputs)
            {
                SetTestCase(inputs.OfType<TestCase>().First());
                return null;
            }
        }

        [ServiceContract]
        public interface IMyServiceClientAsync : IClientChannel
        {
            [OperationContract(Action = nameof(IMyService.Work), ReplyAction = nameof(IMyService.Work))]
            Task WorkAsync(TestCase testCase);
        }

        [ServiceContract]
        public interface IMyServiceClient : IClientChannel
        {
            [OperationContract(Action = nameof(IMyService.Work), ReplyAction = nameof(IMyService.Work))]
            void Work(TestCase testCase);
        }

        [ServiceContract]
        public interface IMyService
        {
            [OperationContract(Action = nameof(Work), ReplyAction = nameof(Work))]
            void Work(TestCase testCase);
        }

        public class MyService : IMyService
        {
            public void Work(TestCase testCase)
            {
                DumpThreadInfo();
            }
        }

        private NetTcpBinding binding;
        private EndpointAddress address;
        private Uri addressUri;

        public WcfTest()
        {
            addressUri = new Uri($"net.tcp://localhost:8888/{nameof(MyService)}");

            address = new EndpointAddress(addressUri);

            binding = new NetTcpBinding();

        }
        public void StartWcfServer()
        {
            DumpThreadInfo();

            using (ServiceHost serviceHost = new ServiceHost(typeof(MyService)))
            {
                serviceHost
                    .AddServiceEndpoint(typeof(IMyService), binding, addressUri)
                    .Behaviors.Add(new TestCultureInfoEndpointBehavior());

                using (new SimpleConsoleService(startAction: () =>
                {
                    serviceHost.Open();
                })) { }

            }

        }
        public void StartWcfClientCase1()
        {
            Task.WaitAll(ClientCallAsync(TestCase.Case1));
        }
        public void StartWcfClientCase2()
        {
            Task.WaitAll(ClientCallAsync(TestCase.Case2));
        }
        public void StartWcfClientCase3()
        {
            Task.WaitAll(ClientCallAsync(TestCase.Case3));
        }
        public void StartWcfClientCase4()
        {
            Task.WaitAll(ClientCallAsync(TestCase.Case4));
        }

        private async Task ClientCallAsync(TestCase testCase)
        {
            var client = ChannelFactory<IMyServiceClientAsync>.CreateChannel(binding, address);

            client.Open();

            try
            {
                await client.WorkAsync(testCase);

            }
            finally
            {
                client.Close();
            }
        }

    }
}
