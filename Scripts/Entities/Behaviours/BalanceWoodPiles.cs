using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class BalanceWoodPiles : SubsumptionStack.Layer
    {
        protected override bool OnThink(double dt)
        {
            if (Human.IsHoldingItem) {
                if (!Human.HeldItem.IsOfClass("plank")) return false;

                // TODO
            } else {

            }

            return false;
        }
    }
}
