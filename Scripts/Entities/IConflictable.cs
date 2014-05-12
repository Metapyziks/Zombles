using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zombles.Scripts.Entities
{
    public abstract class Conflictable<T> : IComparable<T>
        where T : Conflictable<T>
    {
        public static List<T> ResolveConflicts(IEnumerable<T> elements)
        {
            var list = elements.ToList();

            list.Sort();

            var filtred = new List<T>();

            try {
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
            } catch { list = new List<T>(); }

            return list;
        }

        public abstract float Utility { get; }

        public abstract bool ConflictsWith(T other);

        public abstract T ResolveConflict(T other);

        public int CompareTo(T other)
        {
            float thisU = Utility;
            float thatU = other.Utility;

            return thisU > thatU ? -1 : thisU == thatU ? 0 : 1;
        }
    }
}
