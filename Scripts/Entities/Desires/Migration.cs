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
            var pos = agent.Position2D;
            
            var curUtil = beliefs.Blocks.First(x => x.Block == agent.Block).Utility;
            var best = beliefs.Blocks
                .Where(x => x.Utility > curUtil)
                .OrderByDescending(x => x.Utility / Math.Max(1f, agent.World.Difference(pos, x.Block.GetNearestPosition(pos)).Length))
                .FirstOrDefault();
            
            if (best != null && best.Block != agent.Block) {
                yield return new Migration(best);
            }
        }

        private double _creationTime;

        private BlockBeliefs _destBeliefs;

        public Block Destination { get; private set; }

        public override float Utility
        {
            get { return (float) _destBeliefs.Utility * 8f; }
        }

        public Migration(BlockBeliefs dest)
        {
            _creationTime = MainWindow.Time;
            _destBeliefs = dest;

            Destination = dest.Block;
        }

        public override bool ConflictsWith(Desire other)
        {
            return other is Migration;
        }

        public override Desire ResolveConflict(Desire other)
        {
            if (other is Migration) {
                return ResolveConflict((Migration) other);
            } else {
                return this;
            }
        }

        public Migration ResolveConflict(Migration other)
        {
            if (other.Destination == Destination) {
                return _creationTime < other._creationTime ? this : other;
            } else {
                return this;
            }
        }

        public override Intention GetIntention(Beliefs beliefs)
        {
            return new Intentions.Migration(this, beliefs);
        }
    }
}
