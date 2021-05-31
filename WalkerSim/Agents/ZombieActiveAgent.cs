using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using UnityEngine;

namespace WalkerSim
{
    class ZombieActiveAgent : IDisposable
    {
        private static readonly PRNG _rng = new();
        
        private readonly IDisposable _disposable;
        public ZombieAgent Parent { get; }
        public int entityId = -1;
        public ulong lifeTime = 0;
        public IZone? currentZone = null;
        public Vector3 intendedGoal;
        public bool reachedInitialTarget;

        private BehaviorSubject<State> _state = new(State.InitialWalkIn);

        enum State
        {
            InitialWalkIn,
            Distracted,
            Idle,
            WalkOut,
        }

        public ZombieActiveAgent(ZombieAgent parent)
        {
            Parent = parent;
            _disposable = _state
                .ObserveOn(Scheduler.Default)
                .Select(state =>
                {
                    return state switch
                    {
                        State.InitialWalkIn => InitialInvestigationCheck(),
                        State.Idle => Idle(),
                        State.WalkOut => WalkOut(),
                        State.Distracted => Distracted(),
                        _ => throw new NotImplementedException(),
                    };
                })
                .Switch()
                .Subscribe();
        }

        private IObservable<Unit> InitialInvestigationCheck()
        {
            return Observable.Interval(TimeSpan.FromSeconds(5))
                .ObserveOn(MyScheduler.Instance)
                .Select(_ =>
                {
                    var world = GameManager.Instance.World;
                    if (world.GetEntity(entityId) is not EntityZombie entityZombie) return Unit.Default;

                    if (CheckIfDistracted(entityZombie)) return Unit.Default;

                    // If zombie reached its target, send it somewhere
                    var distanceToTarget = Vector3.Distance(entityZombie.position, entityZombie.InvestigatePosition);
                    if (distanceToTarget <= 20.0f)
                    {
#if DEBUG
                        Log.Out(
                            $"[{Parent.id}] Reached its target at {entityZombie.InvestigatePosition}.");
#endif
                        entityZombie.ClearInvestigatePosition();
                        reachedInitialTarget = true;
                        _state.OnNext(State.Idle);
                    }
                    else
                    {
#if DEBUG
                        Log.Out($"[{Parent.id}] [{_state.Value}] was {distanceToTarget} away from its target");
#endif
                    }

                    return Unit.Default;
                });
        }

        private IObservable<Unit> Idle()
        {
            TimeSpan toWait;
            lock (_rng)
            {
                toWait = TimeSpan.FromSeconds(_rng.Get(Config.Instance.MinIdleSeconds, Config.Instance.MaxIdleSeconds));
            }
#if DEBUG
            Log.Out($"[{Parent.id}] waiting {toWait.TotalSeconds} seconds.");
#endif

            return Observable.Interval(toWait)
                .Take(1)
                .Select(_ =>
                {
                    _state.OnNext(State.WalkOut);
                    return Unit.Default;
                });
        }

        private IObservable<Unit> WalkOut()
        {
            return Observable.Return(Unit.Default) 
                .ObserveOn(MyScheduler.Instance)
                .Select(_ =>
                {
                    var world = GameManager.Instance.World;
                    if (world.GetEntity(entityId) is EntityZombie entityZombie)
                    {
#if DEBUG
                        Log.Out($"[{Parent.id}] walking away.");
#endif
                        intendedGoal = Parent.Inactive.targetPos;
                        entityZombie.SetInvestigatePosition(
                            intendedGoal,
                            6000,
                            false);
                    }
                    
                    return Unit.Default;
                })
                .Concat(Observable.Interval(TimeSpan.FromSeconds(5))
                    .ObserveOn(MyScheduler.Instance)
                    .Select(_ =>
                    {
                        var world = GameManager.Instance.World;
                        if (world.GetEntity(entityId) is EntityZombie entityZombie)
                        {
#if DEBUG
                            var distanceToTarget = Vector3.Distance(entityZombie.position, entityZombie.InvestigatePosition);
                            Log.Out($"[{Parent.id}] [{_state.Value}] was {distanceToTarget} away from its target");
#endif
                            CheckIfDistracted(entityZombie);
                        }

                        return Unit.Default;
                    }));
        }

        private IObservable<Unit> Distracted()
        {
            return Observable.Interval(TimeSpan.FromSeconds(5))
                .Select(_ =>
                {
                    var world = GameManager.Instance.World;
                    if (world.GetEntity(entityId) is not EntityZombie entityZombie) return Unit.Default;

                    // If no longer investigating something else
                    if (!entityZombie.HasInvestigatePosition)
                    {
#if DEBUG
                        Log.Out($"[{Parent.id}] no longer distracted.");
#endif
                        // Try to walk to where we had wanted to go
                        entityZombie.SetInvestigatePosition(
                            intendedGoal,
                            6000,
                            false);
                        _state.OnNext(reachedInitialTarget ? State.WalkOut : State.InitialWalkIn);
                    }
                    
                    return Unit.Default;
                });
        }

        private bool CheckIfDistracted(EntityZombie entityZombie)
        {
            if (intendedGoal != entityZombie.InvestigatePosition)
            {
#if DEBUG
                Log.Out($"[{Parent.id}] was distracted.");
#endif
                _state.OnNext(State.Distracted);
                return true;
            }

            return false;
        }
        
        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}