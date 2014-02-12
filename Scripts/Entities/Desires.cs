using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zombles.Scripts.Entities
{
    abstract class Desire : IComparable<Desire>
    {
        private static List<MethodInfo> _discoveryMethods;

        private static void FindDiscoveryMethods()
        {
            _discoveryMethods = new List<MethodInfo>();

            foreach (var type in ScriptManager.GetTypes(typeof(Desire))) {
                var discover = type.GetMethod("Discover", BindingFlags.Public | BindingFlags.Static);

                if (!discover.ReturnType.DoesExtend(typeof(IEnumerable<Desire>))) continue;
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

            desires.Sort();

            var filtred = new List<Desire>();

            for (int i = 0; i < desires.Count; ++i) {
                var a = desires[i];
                for (int j = desires.Count - 1; j >= i; --j) {
                    var b = desires[j];

                    if (a.ConflictsWith(b) || b.ConflictsWith(a)) {
                        desires.RemoveAt(j);
                        desires[i] = a.ResolveConflict(b);

                        desires.Sort(); --i; break;
                    }
                }
            }

            return desires;
        }

        public abstract float Utility { get; }

        public abstract bool ConflictsWith(Desire other);

        public abstract Desire ResolveConflict(Desire other);

        public int CompareTo(Desire other)
        {
            float thisU = Utility;
            float thatU = other.Utility;

            return thisU > thatU ? 1 : thisU == thatU ? 0 : -1;
        }
    }
}
