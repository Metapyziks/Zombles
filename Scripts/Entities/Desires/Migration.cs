using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Desires
{
    class Migration : Desire
    {
        public static IEnumerable<Migration> Discover(Beliefs beliefs)
        {
            var agent = beliefs.Entity;
            var curUtil = beliefs.Blocks.First(x => x.Block == agent.Block).Utility;
            var best = beliefs.Blocks
                .Where(x => x.Utility > curUtil)
                .OrderByDescending(x => x.Utility)
                .FirstOrDefault();

            if (best != null && best.Block != agent.Block) {
                yield return new Migration(best);
            }
        }

        private BlockBeliefs _destBeliefs;

        public Block Destination { get; private set; }

        public override float Utility
        {
            get { return (float) _destBeliefs.Utility; }
        }

        public Migration(BlockBeliefs dest)
        {
            _destBeliefs = dest;
            Destination = dest.Block;
        }

        public override bool ConflictsWith(Desire other)
        {
            return other is Migration;
        }

        public override Desire ResolveConflict(Desire other)
        {
            return this;
        }

        public override Intention GetIntention(Beliefs beliefs)
        {
            return new Intentions.Migration(this, beliefs);
        }
    }
}
