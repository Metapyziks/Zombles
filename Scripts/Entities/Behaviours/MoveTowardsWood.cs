using System.Linq;
using Zombles.Entities;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class MoveTowardsWood : SubsumptionStack.Layer
    {
        private Entity _currTarget;
        private double _nextTargetTime;
        private RouteNavigator _nav;

        protected override bool OnThink(double dt)
        {
            if (Human.IsHoldingItem && Human.HeldItem.HasComponent<Plank>()) return false;

            if (_nextTargetTime <= MainWindow.Time) {
                _nextTargetTime = MainWindow.Time + 0.5;

                _currTarget = Entity.Block
                    .Where(x => x.HasComponent<WoodPile>()
                        && World.GetTile(x.Position2D).IsInterior)
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
            }

            if (_currTarget == null || _nav == null || !_nav.HasDirection) return false;

            Human.StartMoving(_nav.GetDirection());
            return true;
        }
    }
}
