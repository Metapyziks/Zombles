using System.Collections.Generic;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Intentions
{
    class Mobbing : Intention
    {
        public EntityBeliefs Target { get; private set; }

        public Mobbing(Desires.Mobbing desire, Beliefs beliefs)
            : base(desire, beliefs)
        {
            Target = desire.Target;
        }

        public override bool ShouldAbandon()
        {
            return !Target.Entity.GetComponent<Health>().IsAlive;
        }

        public override bool ShouldKeep()
        {
            return false;
        }

        public override IEnumerable<Action> GetActions()
        {
            yield return new DropItemAction(3f);

            var diff = Entity.World.Difference(Entity.Position2D, Target.LastPos);

            var trace = new TraceLine(World);
            trace.Origin = Target.LastPos;
            trace.HitGeometry = true;
            trace.HitEntities = false;
            trace.HullSize = Entity.GetComponent<Collision>().Size;

            bool closest = true;

            var it = new NearbyEntityEnumerator(World, Target.LastPos, diff.Length);
            while (it.MoveNext()) {
                var cur = it.Current;

                if (cur == Entity || !cur.HasComponent<Human>() || !cur.GetComponent<Health>().IsAlive) continue;

                trace.Target = cur.Position2D;

                if (trace.GetResult().Hit) continue;

                closest = false;
                break;
            }

            if (!closest) {
                yield return new MovementAction(diff.Normalized() * 128f);
            }

            if (diff.LengthSquared < 1.5f) {
                yield return new AttackAction(Target.Entity);
            }
        }
    }
}
