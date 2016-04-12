using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CultureInfoAndThreads
{
    public enum TestCase
    {
        Case1,
        Case2,
        Case3,
        Case4
    }

    public class Helpers {

        private const string TestCulture = "pl-PL";

        public static void DumpThreadInfo(string context = "[no context]")
        {
            var thread = Thread.CurrentThread;

            int completionPortThreads;
            int workerThreads;
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);

            Console.WriteLine($"Current TID: {thread.ManagedThreadId} Worker Threads: {workerThreads} IO Threads: {completionPortThreads} {context} IsThreadPoolThread: {thread.IsThreadPoolThread} CurrentUICulture: {thread.CurrentUICulture} CurrentCulture: {thread.CurrentCulture} TestKey resource: {TestResource.TestKey}");
        }

        public static void SetCultureCase1(string culture = TestCulture)
        {
            var cultureInfo = new CultureInfo(culture);
            var thread = Thread.CurrentThread;

            thread.CurrentCulture = cultureInfo;
            thread.CurrentUICulture = cultureInfo;
        }
        public static void SetCultureCase2(string culture = TestCulture)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            var thread = Thread.CurrentThread;

            thread.CurrentCulture = cultureInfo;
            thread.CurrentUICulture = cultureInfo;
        }
        public static void SetCultureCase3(string culture = TestCulture)
        {
            var cultureInfo = new CultureInfo(culture);

            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }
        public static void SetCultureCase4(string culture = TestCulture)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(culture);

            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }
        public static void ForceNoAsyncCurrentCultureFlag(bool flag = false)
        {
            AppContext.SetSwitch("Switch.System.Globalization.NoAsyncCurrentCulture", flag);
        }

        public static void SetTestCase(TestCase testCase)
        {
            switch (testCase)
            {
                case TestCase.Case1:
                    SetCultureCase1();
                    break;
                case TestCase.Case2:
                    SetCultureCase2();
                    break;
                case TestCase.Case3:
                    SetCultureCase3();
                    break;
                case TestCase.Case4:
                    SetCultureCase4();
                    break;
                default:
                    break;
            }
        }

    }
}
