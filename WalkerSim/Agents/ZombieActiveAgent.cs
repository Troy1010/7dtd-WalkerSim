using UnityEngine;

namespace WalkerSim
{
    class ZombieActiveAgent
    {
        public ZombieAgent Parent { get; }
        public int entityId = -1;
        public ulong lifeTime = 0;
        public IZone currentZone = null;
        public Vector3 spawnPos = new Vector3();

        public ZombieActiveAgent(ZombieAgent parent)
        {
            Parent = parent;
        }
    }
}