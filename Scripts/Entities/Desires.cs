using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zombles.Scripts.Entities
{
    public abstract class Desire : IComparable<Desire>
    {
        public static IEnumerable<Desire> ResolveConflicts(IEnumerable<Desire> desires)
        {
            var list = desires.ToList();

            list.Sort();

            var filtred = new List<Desire>();

            for (int i = 0; i < list.Count; ++i) {
                var a = list[i];
                for (int j = list.Count - 1; j > i; --j) {
                    var b = list[j];

                    if (a.ConflictsWith(b) || b.ConflictsWith(a)) {
                        list.RemoveAt(j);
                        list[i] = a.ResolveConflict(b);

                        list.Sort(); i = -1; break;
                    }
                }
            }

            return list;
        }

        public abstract float Utility { get; }

        public abstract bool ConflictsWith(Desire other);

        public abstract Desire ResolveConflict(Desire other);

        public abstract Intention GetIntention(Beliefs beliefs);

        public int CompareTo(Desire other)
        {
            float thisU = Utility;
            float thatU = other.Utility;

            return thisU > thatU ? -1 : thisU == thatU ? 0 : 1;
        }
    }
}
