using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zombles.Scripts.Entities.Desires
{
    public class Wander : Desire
    {
        public static IEnumerable<Wander> Discover(Beliefs beliefs)
        {
            yield return new Wander();
        }

        public override Intention GetIntention(Beliefs beliefs)
        {
            return new Intentions.Wander(this, beliefs);
        }

        public override float Utility
        {
            get { return 0f; }
        }

        public Wander() { }

        public override bool ConflictsWith(Desire other)
        {
            return other is Wander;
        }

        public override Desire ResolveConflict(Desire other)
        {
            return this;
        }
    }
}
