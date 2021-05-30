using System;

namespace WalkerSim
{
    [Serializable]
    struct ZombieData
    {
        public int health;
        public float x;
        public float y;
        public float z;
        public float targetX;
        public float targetY;
        public float targetZ;
        public float dirX;
        public float dirY;
        public bool target;
    }
}