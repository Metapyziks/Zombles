using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zombles.Scripts.Entities
{
    public abstract class Desire : IComparable<Desire>
    {
        private static List<MethodInfo> _discoveryMethods;

        private static void FindDiscoveryMethods()
        {
            _discoveryMethods = new List<MethodInfo>();

            foreach (var type in ScriptManager.GetTypes(typeof(Desire))) {
                var discover = type.GetMethod("Discover", BindingFlags.Public | BindingFlags.Static);

                if (!typeof(IEnumerable<Desire>).IsAssignableFrom(discover.ReturnType)) continue;
                if (discover.GetParameters().Length != 1) continue;
                if (discover.GetParameters().First().ParameterType != typeof(Beliefs)) continue;

                _discoveryMethods.Add(discover);
            }
        }

        public static IEnumerable<Desire> DiscoverAll(Beliefs beliefs)
        {
            if (_discoveryMethods == null) {
                FindDiscoveryMethods();
            }

            var desires = _discoveryMethods
                .SelectMany(x => (IEnumerable<Desire>) x.Invoke(null, new[] { beliefs }))
                .ToList();

            return desires;
        }

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
