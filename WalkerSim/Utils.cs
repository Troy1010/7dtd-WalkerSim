using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WalkerSim
{
    public static class Utils
    {
        public static float Remap(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        public static bool IsPlayerAdmin(ClientInfo cl)
        {
            return GameManager.Instance.adminTools.IsAdmin(cl);
        }

        // Returns the distance between a and b
        public static int Distance(int a, int b)
        {
            return Math.Max(a, b) - Math.Min(a, b);
        }

        public static float Distance(float a, float b)
        {
            return Math.Max(a, b) - Math.Min(a, b);
        }

        public static void DisposeWith(this IDisposable disposable, CompositeDisposable composite)
        {
            composite.Add(disposable);
        }

        public static async void FireAndForget(this Task task, Action<Exception> onException)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                onException(e);
            }
        }

        public static async void FireAndForget(this Task task)
        {
            task.FireAndForget(e => Log.Error("Error in task {0}", e));
        }
    }
}
