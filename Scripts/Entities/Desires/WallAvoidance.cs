using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zombles.Scripts.Entities.Desires
{
    public class WallAvoidance : Desire
    {
        public static IEnumerable<WallAvoidance> Discover(Beliefs beliefs)
        {
            yield return new WallAvoidance();
        }

        public WallAvoidance()
        {

        }

        public override Intention GetIntention(Beliefs beliefs)
        {
            return new Intentions.WallAvoidance(this, beliefs);
        }

        public override float Utility
        {
            get { return 1f; }
        }

        public override bool ConflictsWith(Desire other)
        {
            return other is WallAvoidance;
        }

        public override Desire ResolveConflict(Desire other)
        {
            return this;
        }
    }
}
