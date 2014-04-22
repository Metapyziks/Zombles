using System.Linq;

using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class BreakCrates : SubsumptionStack.Layer
    {
        private double _nextSearch;
        private Entity _currTarget;
        private RouteNavigator _nav;

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
                        //Human.StopMoving();
                        if (Human.CanAttack) {
                            Human.Attack(diff);
                        }
                        return true;
                    } else if (_nav != null && _nav.HasDirection) {
                        Human.StartMoving(_nav.GetDirection());
                        return true;
                    }

                    return false;
                }
            }

            if (MainWindow.Time < _nextSearch) return false;
            _nextSearch = MainWindow.Time + Tools.Random.NextDouble(0.4, 0.6);

            _currTarget = Entity.Block
                .Where(x => x.HasComponent<WoodenBreakable>())
                .OrderBy(x => World.Difference(Position2D, x.Position2D).LengthSquared)
                .FirstOrDefault();

            if (_currTarget == null) {
                if (_nav != null) {
                    _nav.Dispose();
                    _nav = null;
                }
            } else if (_nav == null || _nav.HasEnded || _nav.CurrentTarget != _currTarget.Position2D) {
                if (_nav != null) _nav.Dispose();

                _nav = new RouteNavigator(Entity, _currTarget.Position2D);
            }

            return false;
        }
    }
}
