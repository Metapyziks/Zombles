using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class SelfDefence : SubsumptionStack.Layer
    {
        public double MinAttackCheckPeriod { get; set; }
        public double MaxAttackCheckPeriod { get; set; }

        public float AttackRadius { get; set; }

        protected double _nextDefendCheck;
        protected Entity _attackTarget;

        protected override void OnSpawn()
        {
            _nextDefendCheck = MainWindow.Time;
            _attackTarget = null;

            MinAttackCheckPeriod = 0.25;
            MaxAttackCheckPeriod = 0.5;

            AttackRadius = 0.75f + Tools.Random.NextSingle() * 1.25f;
        }

        protected override bool OnThink(double dt)
        {
            if (MainWindow.Time >= _nextDefendCheck) {
                _nextDefendCheck = MainWindow.Time + Tools.Random.NextDouble(
                    MinAttackCheckPeriod, MaxAttackCheckPeriod);

                _attackTarget = null;

                var trace = new TraceLine(World);
                trace.Origin = Position2D;
                trace.HitGeometry = true;
                trace.HitEntities = false;
                trace.HullSize = Entity.GetComponent<Collision>().Size;

                float closestDist2 = float.MaxValue;

                var it = SearchNearbyEnts(AttackRadius);
                while (it.MoveNext()) {
                    var cur = it.Current;
                    if (!cur.HasComponent<Zombie>() || !cur.HasComponent<Health>()) continue;

                    if (!cur.GetComponent<Health>().IsAlive) continue;

                    Vector2 diff = World.Difference(Position2D, cur.Position2D);
                    var dist2 = diff.LengthSquared;

                    if (dist2 == 0 || dist2 >= closestDist2) continue;

                    trace.Target = cur.Position2D;

                    if (trace.GetResult().Hit) continue;

                    _attackTarget = cur;
                    closestDist2 = dist2;
                }
            }

            if (_attackTarget != null && _attackTarget.GetComponent<Health>().IsAlive) {
                var diff = World.Difference(Position2D, _attackTarget.Position2D);

                if (Human.CanAttack) {
                    if (diff.LengthSquared < 0.75f) {
                        Human.Attack(diff);
                        Human.StopMoving();
                    } else {
                        Human.StartMoving(diff);
                    }
                }

                return true;
            }

            return false;
        }
    }
}
