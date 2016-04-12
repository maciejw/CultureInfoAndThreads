using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static CultureInfoAndThreads.Helpers;

namespace CultureInfoAndThreads
{
    public class ThreadPoolTest
    {
        public void OnlyThreadPool()
        {
            var waitEvents = new List<WaitHandle>();

            ForceNoAsyncCurrentCultureFlag();

            Func<WaitCallback> prepareWork = () =>
            {
                var @event = new ManualResetEventSlim();
                waitEvents.Add(@event.WaitHandle);

                WaitCallback work = state =>
                {
                    SetCultureCase2();

                    DumpThreadInfo();

                    @event.Set();
                };

                return work;
            };

            Action queueWork = () =>
            {
                ThreadPool.QueueUserWorkItem(prepareWork());
            };


            DumpThreadInfo();


            for (int i = 0; i < 10; i++)
            {
                queueWork();
            }

            WaitHandle.WaitAll(waitEvents.ToArray());
            waitEvents.ForEach(w => w.Dispose());
            DumpThreadInfo();

        }
    }
}
