using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class Mob : SubsumptionStack.Layer
    {
        public double MinMobCheckPeriod { get; set; }
        public double MaxMobCheckPeriod { get; set; }

        public float MobRadius { get; set; }
        public float MaxMobRatio { get; set; }

        protected double _nextMobCheck;
        protected Entity _mobTarget;

        protected override void OnSpawn()
        {
            _nextMobCheck = MainWindow.Time;
            _mobTarget = null;

            MinMobCheckPeriod = 0.25;
            MaxMobCheckPeriod = 0.5;

            MaxMobRatio = 0.33f;

            MobRadius = 8f;
        }

        private bool ShouldMob(Entity ent)
        {
            var trace = new TraceLine(World);
            trace.Origin = ent.Position2D;
            trace.HitGeometry = true;
            trace.HitEntities = false;
            trace.HullSize = Entity.GetComponent<Collision>().Size;

            int survivors = 1;
            int zombies = 1;

            var it = new NearbyEntityEnumerator(World, ent.Position2D, MobRadius);
            while (it.MoveNext()) {
                var cur = it.Current;

                if (cur == Entity || cur == ent) continue;

                if (!cur.HasComponent<Human>() || !cur.HasComponent<Health>()) continue;
                if (!cur.GetComponent<Health>().IsAlive) continue;
                
                trace.Target = cur.Position2D;

                if (trace.GetResult().Hit) continue;

                if (cur.HasComponent<Survivor>()) ++survivors;
                else ++zombies;
            }

            return zombies <= MaxMobRatio * survivors;
        }

        protected override bool OnThink(double dt)
        {
            System.Diagnostics.Debugger.Break();

            if (MainWindow.Time >= _nextMobCheck) {
                _nextMobCheck = MainWindow.Time + Tools.Random.NextDouble(
                    MinMobCheckPeriod, MaxMobCheckPeriod);

                var trace = new TraceLine(World);
                trace.Origin = Position2D;
                trace.HitGeometry = true;
                trace.HitEntities = false;

                float closestDist2 = float.MaxValue;

                _mobTarget = null;

                var it = new NearbyEntityEnumerator(World, Entity.Position2D, MobRadius);
                while (it.MoveNext()) {
                    var cur = it.Current;
                    if (!cur.HasComponent<Zombie>() || !cur.HasComponent<Health>()) continue;
                    if (!cur.GetComponent<Health>().IsAlive) continue;

                    Vector2 diff = World.Difference(Position2D, cur.Position2D);
                    var dist2 = diff.LengthSquared;

                    if (dist2 == 0 || dist2 >= closestDist2) continue;

                    trace.Target = cur.Position2D;

                    if (trace.GetResult().Hit) continue;

                    if (ShouldMob(cur)) {
                        _mobTarget = cur;
                        closestDist2 = dist2;
                    }
                }
            }

            if (_mobTarget != null && _mobTarget.GetComponent<Health>().IsAlive) {
                Human.StartMoving(World.Difference(Position2D, _mobTarget.Position2D));
                return true;
            }

            return false;
        }
    }
}
