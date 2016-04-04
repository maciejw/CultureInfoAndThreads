using Microsoft.Win32;
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
using static CultureInfoAndThreads.DotNetVersion;

namespace CultureInfoAndThreads
{
    public static class DotNetVersion
    {

        public static void ShowVersionHeader()
        {
            var assembly = typeof(CultureInfo).Assembly;
            var version = FileVersionInfo.GetVersionInfo(assembly.Location);
            Console.WriteLine("Version info:");
            Console.WriteLine($"{version}");
        }
        public static void Show45or451FromRegistry()
        {
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                var ndpValue = ndpKey.GetValue("Release") as int?;
                if (ndpValue.HasValue)
                {
                    Console.WriteLine("Registry version: " + CheckFor45DotVersion(ndpValue.Value));
                }
                else
                {
                    Console.WriteLine("Registry version 4.5 or later is not detected.");
                }
            }
            Console.WriteLine();
        }
        public static void ShowVersionFromEnvironment()
        {
            Console.WriteLine($"Environment version: {Environment.Version}");
            Console.WriteLine();

        }
        public static void ShowUpdates()
        {
            Console.WriteLine("Updates:");
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\Updates"))
            {
                foreach (string baseKeyName in baseKey.GetSubKeyNames())
                {
                    if (baseKeyName.Contains(".NET Framework") || baseKeyName.StartsWith("KB") || baseKeyName.Contains(".NETFramework"))
                    {
                        using (RegistryKey updateKey = baseKey.OpenSubKey(baseKeyName))
                        {
                            string name = (string)updateKey.GetValue("PackageName", "");
                            Console.WriteLine($"{baseKeyName}  {name}");
                            foreach (string kbKeyName in updateKey.GetSubKeyNames())
                            {
                                using (RegistryKey kbKey = updateKey.OpenSubKey(kbKeyName))
                                {
                                    name = (string)kbKey.GetValue("PackageName", "");
                                    Console.WriteLine($"  {kbKeyName}  {name}");

                                    if (kbKey.SubKeyCount > 0)
                                    {
                                        foreach (string sbKeyName in kbKey.GetSubKeyNames())
                                        {
                                            using (RegistryKey sbSubKey = kbKey.OpenSubKey(sbKeyName))
                                            {
                                                name = (string)sbSubKey.GetValue("PackageName", "");
                                                if (name == "")
                                                    name = (string)sbSubKey.GetValue("Description", "");
                                                Console.WriteLine($"    {sbKeyName}  {name}");

                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }
        private static string CheckFor45DotVersion(int releaseKey)
        {
            if (releaseKey >= 393295)
            {
                return "4.6 or later";
            }
            if ((releaseKey >= 379893))
            {
                return "4.5.2 or later";
            }
            if ((releaseKey >= 378675))
            {
                return "4.5.1 or later";
            }
            if ((releaseKey >= 378389))
            {
                return "4.5 or later";
            }
            // This line should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }
    }

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
        private const string TestCulture = "pl-PL";

        void IParameterInspector.AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
        }

        object IParameterInspector.BeforeCall(string operationName, object[] inputs)
        {
            switch (inputs.OfType<TestCase>().First())
            {
                case TestCase.Case1:
                    Case1();
                    break;
                case TestCase.Case2:
                    Case2();
                    break;
                case TestCase.Case3:
                    Case3();
                    break;
                case TestCase.Case4:
                    Case4();
                    break;
                default:
                    break;
            }

            return null;
        }



        private void Case1()
        {
            var cultureInfo = new CultureInfo(TestCulture);
            var thread = Thread.CurrentThread;

            thread.CurrentCulture = cultureInfo;
            thread.CurrentUICulture = cultureInfo;
        }
        private void Case2()
        {
            var cultureInfo = CultureInfo.GetCultureInfo(TestCulture);
            var thread = Thread.CurrentThread;

            thread.CurrentCulture = cultureInfo;
            thread.CurrentUICulture = cultureInfo;
        }
        private void Case3()
        {
            var cultureInfo = new CultureInfo(TestCulture);

            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }
        private void Case4()
        {
            var cultureInfo = CultureInfo.GetCultureInfo(TestCulture);

            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }
    }
    public enum TestCase
    {
        Case1,
        Case2,
        Case3,
        Case4
    }

    [ServiceContract]
    public interface IMyServiceClientAsync : IClientChannel
    {
        [OperationContract(Action = nameof(IMyService.Work), ReplyAction = nameof(IMyService.Work))]
        Task WorkAsync(TestCase @case);
    }

    [ServiceContract]
    public interface IMyServiceClient: IClientChannel
    {
        [OperationContract(Action = nameof(IMyService.Work), ReplyAction = nameof(IMyService.Work))]
        void Work(TestCase @case);
    }

    [ServiceContract]
    public interface IMyService
    {
        [OperationContract(Action = nameof(Work), ReplyAction = nameof(Work))]
        void Work(TestCase @case);
    }

    public class MyService : IMyService
    {
        public void Work(TestCase @case)
        {
            DumpThreadInfo();
        }
    }

    public static class Program
    {
        public static void DumpThreadInfo()
        {
            var thread = Thread.CurrentThread;

            Console.WriteLine($"ManagedThreadId: {thread.ManagedThreadId} CurrentUICulture: {thread.CurrentUICulture} CurrentCulture: {thread.CurrentCulture} TestKey resource: {TestResource.TestKey}");
        }

        private static NetTcpBinding binding;
        private static EndpointAddress address;
        private static Uri addressUri;
        private static Dictionary<string, Action> commands;

        static void Main(string[] args)
        {
            addressUri = new Uri($"net.tcp://localhost:8888/{nameof(MyService)}");

            address = new EndpointAddress(addressUri);

            binding = new NetTcpBinding();

            commands = new Dictionary<string, Action>
            {
                { nameof(ShowFrameworkVersionInfo), ShowFrameworkVersionInfo },
                { nameof(StartClientCase1), StartClientCase1 },
                { nameof(StartClientCase2), StartClientCase2 },
                { nameof(StartClientCase3), StartClientCase3 },
                { nameof(StartClientCase4), StartClientCase4 },
                { nameof(StartServer), StartServer },
                { nameof(Help), Help },
            };


            var commandName = args.FirstOrDefault() ?? nameof(Help);

            Action command;
            if (commands.TryGetValue(commandName, out command))
            {
                command();
            }
            else
            {
                Console.WriteLine($"Invalid command: {commandName}");
                Help();
            }
        }

        private static void Help()
        {
            var lines = new List<string> { "Available commands:" };
            lines.AddRange(commands.Keys.Select(i => $"\t{i}"));
            lines.ForEach(Console.WriteLine);
        }

        private static void ShowFrameworkVersionInfo()
        {
            ShowVersionHeader();
            Show45or451FromRegistry();
            ShowVersionFromEnvironment();
            ShowUpdates();
        }

        private static void StartServer()
        {
            const string quitText = "Press Ctrl+C to quit.";

            ManualResetEventSlim quitEvent = new ManualResetEventSlim();

            AppContext.SetSwitch("Switch.System.Globalization.NoAsyncCurrentCulture", false);

            var serviceHost = new ServiceHost(typeof(MyService));

            var serviceEndpoint = serviceHost.AddServiceEndpoint(typeof(IMyService), binding, addressUri);

            serviceEndpoint.Behaviors.Add(new TestCultureInfoEndpointBehavior());

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

        private static void StartClientCase1()
        {
            Task.WaitAll(ClientCallAsync(TestCase.Case1));
        }
        private static void StartClientCase2()
        {
            Task.WaitAll(ClientCallAsync(TestCase.Case2));
        }
        private static void StartClientCase3()
        {
            Task.WaitAll(ClientCallAsync(TestCase.Case3));
        }
        private static void StartClientCase4()
        {
            Task.WaitAll(ClientCallAsync(TestCase.Case4));
        }

        private async static Task ClientCallAsync(TestCase @case)
        {
            var client = ChannelFactory<IMyServiceClientAsync>.CreateChannel(binding, address);

            client.Open();

            await client.WorkAsync(@case);

            client.Close();
        }
    }
}
