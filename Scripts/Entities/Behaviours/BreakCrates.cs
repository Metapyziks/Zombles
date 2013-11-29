using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class BreakCrates : SubsumptionStack.Layer
    {
        private double _nextSearch;
        private Entity _currTarget;

        protected override bool OnThink(double dt)
        {
            if (Human.IsHoldingItem && Human.HeldItem.HasComponent<Plank>()) return false;

            if (_currTarget != null) {
                if (!_currTarget.IsValid || !_currTarget.HasComponent<WoodenBreakable>()) {
                    _currTarget = null;
                    _nextSearch = MainWindow.Time;
                } else {
                    var diff = World.Difference(Entity.Position2D, _currTarget.Position2D);

                    if (diff.LengthSquared < 0.75) {
                        Human.StopMoving();
                        if (Human.CanAttack) {
                            Human.Attack(diff);
                        }
                    } else {
                        Human.StartMoving(diff);
                    }

                    return true;
                }
            }

            if (MainWindow.Time < _nextSearch) return false;
            _nextSearch = MainWindow.Time + Tools.Random.NextDouble(0.4, 0.6);

            var closest = float.MaxValue;

            var trace = new TraceLine(World);
            trace.Origin = Entity.Position2D;
            trace.HitGeometry = true;
            trace.HitEntities = true;

            var iter = new NearbyEntityEnumerator(World, Entity.Position2D, 8f);
            while (iter.MoveNext()) {
                var ent = iter.Current;

                if (!ent.HasComponent<WoodenBreakable>()) continue;
                
                var diff = World.Difference(Entity.Position2D, ent.Position2D);
                if (diff.LengthSquared >= closest) continue;

                trace.Target = ent.Position2D;

                var result = trace.GetResult();
                if (result.Hit && !(result.HitEntity && result.Entity == ent)) continue;

                closest = diff.LengthSquared;
                _currTarget = ent;
            }

            return false;
        }
    }
}
