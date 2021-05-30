using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WalkerSim
{
    class ZombieAgent : IComparer, IEquatable<ZombieAgent>
    {
        public enum State
        {
            Idle,
            Wandering,
            Investigating,
            Active,
        }
        
        public int id = -1;
        public int classId = -1;
        public State state = State.Idle;
        public Vector3 pos = new Vector3();
        public int health = -1;
        
        public ZombieInactiveAgent Inactive { get; }
        public ZombieActiveAgent Active { get; }

        public ZombieAgent()
        {
            Inactive = new ZombieInactiveAgent(this);
            Active = new ZombieActiveAgent(this);
        }

        int IComparer.Compare(object a, object b)
        {
            return ((ZombieAgent)a).id - ((ZombieAgent)b).id;
        }

        public bool Equals(ZombieAgent other)
        {
            return id == other.id;
        }
    }
}
