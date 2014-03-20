using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zombles.Scripts.Entities.Desires
{
    public class PlanBarricade : Desire
    {
        public static IEnumerable<Barricading> Discover(Beliefs beliefs)
        {
            var blockBeliefs = beliefs.GetBlock(beliefs.Entity.Block);

            var resources = beliefs.Entities
                .Where(x => x.Type == EntityType.PlankPile || x.Type == EntityType.PlankSource)
                .Where(x => x.LastBlock == blockBeliefs.Block)
                .Sum(x => x.Type == EntityType.PlankPile
                    ? x.Entity.GetComponent<WoodPile>().Count
                    : x.Entity.GetComponent<WoodenBreakable>().MinPlanks);

            if (!blockBeliefs.Block.Enclosed && blockBeliefs.Resources + resources > 10) {
                yield return new Barricading(blockBeliefs);
            }
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
