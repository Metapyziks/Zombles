using System.Linq;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class DropWood : SubsumptionStack.Layer
    {
        protected override bool OnThink(double dt)
        {
            if (!Human.IsHoldingItem) return false;
            if (Human is Survivor && ((Survivor) Human).Exposure <= .5f) return false;
            if (!Human.HeldItem.HasComponent<Plank>()) return false;
            
            Human.DropItem();
            return true;
        }
    }
}
