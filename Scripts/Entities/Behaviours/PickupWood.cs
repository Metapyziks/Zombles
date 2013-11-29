using System.Linq;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class PickupWood : SubsumptionStack.Layer
    {
        protected override bool OnThink(double dt)
        {
            if (Human.IsHoldingItem) return false;
            if (Human is Survivor && ((Survivor) Human).Exposure > 0f) return false;

            var pile = World.GetBlock(Entity.Position2D)
                .Where(x => x.HasComponent<WoodPile>())
                .OrderBy(x => World.Difference(Entity.Position2D, x.Position2D).LengthSquared)
                .FirstOrDefault(x => World.Difference(Entity.Position2D, x.Position2D)
                    .LengthSquared < 2f && x.GetComponent<WoodPile>().CanPickup(Entity));

            if (pile == null) return false;
            if (!World.GetTile(pile.Position2D).IsInterior) return false;

            Human.PickupItem(pile);

            return true;
        }
    }
}
