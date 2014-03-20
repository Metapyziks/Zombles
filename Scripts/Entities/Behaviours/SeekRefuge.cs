using System.Diagnostics;
using System.Linq;
using Zombles.Entities;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class SeekRefuge : SubsumptionStack.Layer
    {
        private RouteNavigator _nav;

        protected override bool OnThink(double dt)
        {
            if (_nav != null && _nav.HasEnded) {
                _nav.Dispose();
                _nav = null;
            }

            if (_nav != null) {
                if (_nav.HasDirection) {
                    Human.StartMoving(_nav.GetDirection());
                    return true;
                }
                return false;
            }

            var block = World.GetBlock(Position2D);

            if (block.Enclosed) return false;
            if (World.GetTile(Position2D).IsInterior) return false;

            var dest = block.FirstOrDefault(x => x.HasComponent<Survivor>()
                && World.GetTile(x.Position2D).IsInterior);

            if (dest == null) return false;

            _nav = new RouteNavigator(Entity, dest.Position2D);
            return true;
        }
    }
}
