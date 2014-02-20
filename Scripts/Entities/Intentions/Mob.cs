using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Entities;
using Zombles.Scripts.Entities.Desires;

namespace Zombles.Scripts.Entities.Intentions
{
    class Mob : Intention
    {
        public Entity Target { get; private set; }

        public Mob(Mobbing desire, Beliefs beliefs)
            : base(desire, beliefs)
        {
            Target = desire.Target;
        }

        public override bool ShouldAbandon()
        {
            return !Target.GetComponent<Health>().IsAlive;
        }

        public override bool ShouldKeep()
        {
            return false;
        }

        public override void Act()
        {
            Human.StartMoving(Entity.World.Difference(Target.Position2D, Entity.Position2D));
        }
    }
}
