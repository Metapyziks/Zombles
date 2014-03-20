using System.Diagnostics;
using System.Linq;
using Zombles.Entities;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class SeekRefuge : SubsumptionStack.Layer
    {
        protected override bool OnThink(double dt)
        {
            var routeNav = Entity.GetComponentOrNull<RouteNavigator>();

            if (routeNav == null || routeNav.HasRoute) return false;

            var block = World.GetBlock(Position2D);

            if (block.Enclosed) return false;
            if (World.GetTile(Position2D).IsInterior) return false;

            var dest = block.FirstOrDefault(x => x.HasComponent<Survivor>()
                && World.GetTile(x.Position2D).IsInterior);

            if (dest == null) return false;

            routeNav.NavigateTo(dest.Position2D);
            return true;
        }
    }
}
