using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Desires
{
    class Mobbing : Desire
    {
        public const float MobRadius = 8f;
        public const float MaxMobRatio = 0.33f;

        private static bool ShouldMob(Entity self, EntityBeliefs ent)
        {
            var trace = new TraceLine(self.World);
            trace.Origin = ent.LastPos;
            trace.HitGeometry = true;
            trace.HitEntities = false;
            trace.HullSize = self.GetComponent<Collision>().Size;

            int survivors = 1;
            int zombies = 1;

            var it = new NearbyEntityEnumerator(self.World, ent.LastPos, MobRadius);
            while (it.MoveNext()) {
                var cur = it.Current;

                if (cur == self || cur == ent.Entity) continue;

                if (!cur.HasComponent<Human>() || !cur.HasComponent<Health>()) continue;
                if (!cur.GetComponent<Health>().IsAlive) continue;

                trace.Target = cur.Position2D;

                if (trace.GetResult().Hit) continue;

                if (cur.HasComponent<Survivor>()) survivors += cur.GetComponent<Health>().Value;
                else zombies += cur.GetComponent<Health>().Value;
            }

            return zombies <= MaxMobRatio * survivors;
        }

        public static IEnumerable<Mobbing> Discover(Beliefs beliefs)
        {
            foreach (var zom in beliefs.Entities.Where(x => x.Type == EntityType.Zombie)) {
                if (beliefs.Entity.World.Difference(beliefs.Entity.Position2D, zom.LastPos).LengthSquared > MobRadius * MobRadius) continue;
                if (ShouldMob(beliefs.Entity, zom)) {
                    if (beliefs.Human.IsSelected) {
                        System.Diagnostics.Debugger.Break();
                    }
                    yield return new Mobbing(zom.Entity);
                }
            }
        }

        public Entity Target { get; private set; }

        public override float Utility
        {
            get { return 10000f; }
        }

        public Mobbing(Entity target)
        {
            Target = target;
        }

        public override bool ConflictsWith(Desire other)
        {
            return other is Mobbing || other is ThreatAvoidance;
        }

        public override Desire ResolveConflict(Desire other)
        {
            return this;
        }

        public override Intention GetIntention(Beliefs beliefs)
        {
            return new Intentions.Mobbing(this, beliefs);
        }
    }
}
