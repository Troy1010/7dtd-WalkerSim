using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UnityEngine;

namespace WalkerSim
{
    class ZombieActiveAgent : IDisposable
    {
        private CompositeDisposable _disposable = new();
        public ZombieAgent Parent { get; }
        public int entityId = -1;
        public ulong lifeTime = 0;
        public IZone? currentZone = null;
        public Vector3 spawnPos = new();

        public ZombieActiveAgent(ZombieAgent parent)
        {
            Parent = parent;
            Observable.Interval(TimeSpan.FromSeconds(5))
                .ObserveOn(MyScheduler.Instance)
                .Subscribe(_ =>
                {
                    var world = GameManager.Instance.World;
                    var entityZombie = (EntityZombie) world.GetEntity(entityId);

                    // If zombie reached its target, send it somewhere
                    var distanceToTarget = Vector3.Distance(entityZombie.position, entityZombie.InvestigatePosition);
                    if (distanceToTarget <= 20.0f)
                    {
                        var newTarget = ((Parent.pos - spawnPos).normalized * 2000) + spawnPos;
#if DEBUG
                        Log.Out(
                            $"[{Parent.id}] Reached its target at {entityZombie.InvestigatePosition}.  Sending to {newTarget}");
#endif
                        entityZombie.ClearInvestigatePosition();
                        entityZombie.SetInvestigatePosition(
                            newTarget,
                            6000,
                            false);
                    }
                    else
                    {
#if DEBUG
                        Log.Out($"[{Parent.id}] was {distanceToTarget} away from its target");
#endif
                    }
                })
                .DisposeWith(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}