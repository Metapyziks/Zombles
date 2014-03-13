using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Desires
{
    public class Barricading : Desire
    {
        public static IEnumerable<Barricading> Discover(Beliefs beliefs)
        {
            var block = beliefs.GetBlock(beliefs.Entity.Block);

            if (block.Utility > 0 && block.Resources > 10) {
                yield return new Barricading(block);
            }
        }

        private BlockBeliefs _blockBeliefs;
        private double _createdTime;

        public Barricading(BlockBeliefs block)
        {
            _blockBeliefs = block;
            _createdTime = MainWindow.Time;
        }

        public override Intention GetIntention(Beliefs beliefs)
        {
            return new Intentions.Barricading(this, beliefs, _blockBeliefs);
        }

        public override float Utility
        {
            get { return (float) (_blockBeliefs.Utility + (MainWindow.Time - _createdTime) * 8.0 / MainWindow.Time); }
        }

        public override bool ConflictsWith(Desire other)
        {
            return other is Barricading || other is Migration;
        }

        public override Desire ResolveConflict(Desire other)
        {
            if (other is Migration) return other;

            return this;
        }
    }
}
