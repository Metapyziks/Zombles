using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Entities;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class FollowRoute : SubsumptionStack.Layer
    {
        protected RouteNavigation RouteNavigation { get; private set; }

        protected override void OnSpawn()
        {
            if (Entity.HasComponent<RouteNavigation>()) {
                RouteNavigation = Entity.GetComponent<RouteNavigation>();
            }
        }

        protected override bool OnThink(double dt)
        {
            if (RouteNavigation != null && RouteNavigation.HasPath) {
                Human.StartMoving(City.Difference(Position2D, RouteNavigation.NextWaypoint));
                return true;
            }

            return false;
        }
    }
}
