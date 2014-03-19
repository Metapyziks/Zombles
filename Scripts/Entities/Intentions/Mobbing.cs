using System.Collections.Generic;
using Zombles.Entities;

namespace Zombles.Scripts.Entities.Intentions
{
    class Mobbing : Intention
    {
        public Entity Target { get; private set; }

        public Mobbing(Desires.Mobbing desire, Beliefs beliefs)
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

        public override IEnumerable<Action> GetActions()
        {
            yield return new DropItemAction(3f);

            var diff = Entity.World.Difference(Entity.Position2D, Target.Position2D);
            yield return new MovementAction(diff.Normalized() * 64f);

            if (diff.LengthSquared < 1.5f) {
                yield return new AttackAction(Target);
            }
        }
    }
}
