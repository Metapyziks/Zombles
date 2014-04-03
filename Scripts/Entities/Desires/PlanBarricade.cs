using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zombles.Scripts.Entities.Desires
{
    public class PlanBarricade : Desire
    {
        public static IEnumerable<PlanBarricade> Discover(Beliefs beliefs)
        {
            yield break;
        }

        public override Intention GetIntention(Beliefs beliefs)
        {
            throw new NotImplementedException();
        }

        public override float Utility
        {
            get { throw new NotImplementedException(); }
        }

        public override bool ConflictsWith(Desire other)
        {
            throw new NotImplementedException();
        }

        public override Desire ResolveConflict(Desire other)
        {
            throw new NotImplementedException();
        }
    }
}
