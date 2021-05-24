using System;
using System.Reactive.Concurrency;
using System.Threading;

namespace WalkerSim
{
    class TM
    {
        public static void Logz(String msg)
        {
#if DEBUG
            Log.Out($"[TM] {msg}");
#endif
        }
        public static void LogThread(String prefix = null)
        {
            Logz($"{prefix}`ThreadID:{Thread.CurrentThread.ManagedThreadId} ThreadName:{Thread.CurrentThread.Name}");
        }

        internal static void LogE(string msg)
        {
#if DEBUG
            Log.Error($"[TM] {msg}");
#endif
        }
    }
}
