using Zombles.Entities;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class FollowRoute : SubsumptionStack.Layer
    {
        protected RouteNavigator RouteNavigation { get; private set; }

        protected override void OnSpawn()
        {
            RouteNavigation = Entity.GetComponentOrNull<RouteNavigator>();
        }

        protected override bool OnThink(double dt)
        {
            if (RouteNavigation != null && RouteNavigation.HasPath) {
                Human.StartMoving(World.Difference(Position2D, RouteNavigation.NextWaypoint));
                return true;
            }

            return false;
        }
    }
}
