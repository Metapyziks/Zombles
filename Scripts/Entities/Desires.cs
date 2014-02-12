using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zombles.Scripts.Entities
{
    abstract class Desire
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

            return _discoveryMethods.SelectMany(x => (IEnumerable<Desire>) x.Invoke(null, new[] { beliefs }));
        }
    }
}
