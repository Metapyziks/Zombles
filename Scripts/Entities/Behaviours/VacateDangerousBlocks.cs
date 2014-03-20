using System.Linq;

using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class VacateDangerousBlocks : SubsumptionStack.Layer
    {
        private RouteNavigator _nav;
        
        protected override bool OnThink(double dt)
        {
            var block = World.GetBlock(Position2D);

            if (!World.GetTile(Position2D).IsInterior || block.Enclosed || ((Survivor) Human).Exposure <= 0f) {
                if (_nav != null) {
                    _nav.Dispose();
                    _nav = null;
                }
                return false;
            }

            if (_nav != null) {
                if (_nav.HasDirection) {
                    Human.StartMoving(_nav.GetDirection());
                }
                return true;
            } else {
                var dest = World.GetIntersections(block)
                    .Where(x => x.Position.LengthSquared > 0f)
                    .OrderBy(x => World.Difference(Entity.Position2D, x.Position).LengthSquared)
                    .First();

                _nav = new RouteNavigator(Entity, dest.Position);
                return false;
            }
        }

        protected override void OnRemove()
        {
            _nav.Dispose();
        }
    }
}
