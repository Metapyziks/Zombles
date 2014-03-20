using System.Linq;

using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class VacateDangerousBlocks : SubsumptionStack.Layer
    {
        protected RouteNavigator RouteNavigation { get; private set; }

        private Intersection _curDest;

        protected override void OnSpawn()
        {
            RouteNavigation = Entity.GetComponentOrNull<RouteNavigator>();
            _curDest = null;
        }

        protected override bool OnThink(double dt)
        {
            if (RouteNavigation == null) return false;
            if (!(Human is Survivor)) return false;

            if (_curDest != null && RouteNavigation.HasRoute && RouteNavigation.CurrentTarget != _curDest.Position) return false;

            var vacating = _curDest != null && RouteNavigation.HasRoute;

            var block = World.GetBlock(Position2D);

            if (!World.GetTile(Position2D).IsInterior || block.Enclosed) {
                if (vacating) RouteNavigation.Route = null;
                return false;
            }

            if (((Survivor) Human).Exposure <= 0f) {
                if (vacating) RouteNavigation.Route = null;
                return false;
            }

            if (vacating) {
                Human.StartMoving(World.Difference(Position2D, RouteNavigation.NextWaypoint));
                return true;
            }

            _curDest = World.GetIntersections(block)
                .Where(x => x.Position.LengthSquared > 0f)
                .OrderBy(x => World.Difference(Entity.Position2D, x.Position).LengthSquared)
                .First();

            RouteNavigation.NavigateTo(_curDest.Position);
            return false;
        }

        protected override void OnRemove()
        {
            RouteNavigation = null;
        }
    }
}
