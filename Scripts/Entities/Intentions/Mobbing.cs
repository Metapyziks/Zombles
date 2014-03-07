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
            var diff = Entity.World.Difference(Target.Position2D, Entity.Position2D);
            yield return new MovementAction(diff);

            if (diff.LengthSquared < 0.75f) {
                yield return new AttackAction(Target);
            }
        }
    }
}
