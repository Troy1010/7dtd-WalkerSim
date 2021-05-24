using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace WalkerSim
{
    public struct DateAndAction
    {
        public DateTimeOffset? date;
        public Func<IScheduler, IDisposable> action;

        public DateAndAction(DateTimeOffset? date, Func<IScheduler, IDisposable> action)
        {
            this.date = date;
            this.action = action;
        }
    }

    class MyScheduler: IScheduler
    {
        public static readonly MyScheduler Instance = new MyScheduler();

        public void Execute()
        {
            lock(_lock)
            {
                var temp = currentList;
                currentList = otherList;
                otherList = temp;
                foreach (var x in otherList)
                {
                    if (x.date == null || x.date.Value < Now)
                    {
                        TM.Logz($"executing action:{x}");
                        x.action(this);
                    }
                    else
                    {
                        TM.Logz($"adding action to otherList:{x}");
                        currentList.Add(x);
                    }
                }
                otherList.Clear();
            }
        }

        object _lock = new Object();

        // 2 lists to switch between, for performance.
        List<DateAndAction> currentList = new List<DateAndAction>();
        List<DateAndAction> otherList = new List<DateAndAction>();

        public DateTimeOffset Now => DateTimeOffset.Now;

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            lock (_lock)
            {
                Func<IScheduler, IDisposable> myWrapper = (s) =>
                {
                    return action(s, state);
                };
                DateAndAction x = new DateAndAction(null, myWrapper);
                TM.Logz($"adding new DateAndAction:{x.date}");
                currentList.Add(x);
                return Disposable.Create(() =>
                {
                    lock(_lock)
                    {
                        currentList.Remove(x);
                    }
                });
        }
    }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            lock (_lock)
            {
                Func<IScheduler, IDisposable> myWrapper = (s) =>
                {
                    return action(s, state);
                };
                DateAndAction x = new DateAndAction(Now + dueTime, myWrapper);
                TM.Logz($"adding new DateAndAction:{x.date}");
                currentList.Add(x);
                return Disposable.Create(() =>
                {
                    lock(_lock)
                    {
                        currentList.Remove(x);
                    }
                });
            }
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            lock (_lock)
            {
                Func<IScheduler, IDisposable> myWrapper = (s) =>
                {
                    return action(s, state);
                };
                DateAndAction x = new DateAndAction(dueTime, myWrapper);
                TM.Logz($"adding new DateAndAction:{x.date}");
                currentList.Add(x);
                return Disposable.Create(() =>
                {
                    lock (_lock)
                    {
                        currentList.Remove(x);
                    }
                });
            }
        }
    }
}
