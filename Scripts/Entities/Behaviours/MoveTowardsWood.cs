using System.Linq;
using Zombles.Entities;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class MoveTowardsWood : SubsumptionStack.Layer
    {
        private Entity _currTarget;
        private double _nextTargetTime;

        protected override bool OnThink(double dt)
        {
            if (Human.IsHoldingItem && Human.HeldItem.HasComponent<Plank>()) return false;

            if (_nextTargetTime >= MainWindow.Time) {
                _nextTargetTime = MainWindow.Time + 0.5;

                _currTarget = SearchNearbyVisibleEnts(16f, (ent, diff) => ent.HasComponent<WoodPile>()
                        && World.GetTile(ent.Position2D).IsInterior)
                    .OrderBy(ent => World.Difference(Position2D, ent.Position2D).LengthSquared)
                    .FirstOrDefault();
            }

            if (_currTarget == null) return false;

            Human.StartMoving(World.Difference(Position2D, _currTarget.Position2D));
            return true;
        }
    }
}
