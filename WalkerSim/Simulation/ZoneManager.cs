using System.Collections.Generic;
using UnityEngine;

namespace WalkerSim
{
    public class ZoneManager<T>
        where T : IZone
    {
        protected List<T> _zones = new();
        protected object _lock = new();
        protected int _pickCount = 0;

        public T? GetNext()
        {
            lock (_lock)
            {
                if (_zones.Count == 0)
                    return default;

                int pick = _pickCount % _zones.Count;
                _pickCount++;

                return _zones[pick];
            }
        }

        public T? GetRandom(PRNG prng, int lastIndex = -1)
        {
            lock (_lock)
            {
                if (_zones.Count == 0)
                    return default;

                for (int i = 0; i < 4; i++)
                {
                    var idx = prng.Get(0, _zones.Count);
                    var zone = _zones[idx];
                    if (zone.GetIndex() != lastIndex)
                    {
                        return zone;
                    }
                }

                return default;
            }
        }

        public T? FindByPos2D(Vector3 pos)
        {
            lock (_lock)
            {
                foreach (var zone in _zones)
                {
                    var z = zone;
                    if (z.IsInside2D(pos))
                        return zone;
                }

                return default;
            }
        }

        public List<T> FindAllByPos2D(Vector3 pos)
        {
            List<T> res = new();
            lock (_lock)
            {
                foreach (var zone in _zones)
                {
                    if (zone.IsInside2D(pos))
                    {
                        res.Add(zone);
                    }
                }
            }
            return res;
        }

        public T? GetRandomClosest(Vector3 pos, PRNG prng, float maxDistance, List<IZone>? excluded, int numAttempts = 5)
        {
            lock (_lock)
            {
                if (_zones.Count == 0)
                    return default;

                float bestDistance = maxDistance;

                T? res = default;
                for (int i = 0; i < numAttempts; i++)
                {
                    var zone = GetRandom(prng);
                    if (zone == null || (excluded?.Contains(zone) ?? false))
                    {
                        continue;
                    }

                    float dist = Vector3.Distance(zone.GetCenter(), pos);
                    if (dist < bestDistance)
                    {
                        res = zone;
                        bestDistance = dist;
                    }
                }

                return res;
            }
        }

        public T? GetClosest(Vector3 pos, float maxDistance)
        {
            lock (_lock)
            {
                if (_zones.Count == 0)
                    return default;

                float bestDistance = float.MaxValue;

                T? res = default;
                for (int i = 0; i < _zones.Count; i++)
                {
                    var zone = _zones[i];
                    float dist = Vector3.Distance(zone.GetCenter(), pos);
                    if (dist < bestDistance)
                    {
                        res = zone;
                        bestDistance = dist;
                    }
                }

                return res;
            }
        }
    }
}
