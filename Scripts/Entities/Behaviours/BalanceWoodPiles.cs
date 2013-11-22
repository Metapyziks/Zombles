using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Entities;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class BalanceWoodPiles : SubsumptionStack.Layer
    {
        private double _nextSearch;
        private Entity _currTarget;

        protected override bool OnThink(double dt)
        {
            if (MainWindow.Time >= _nextSearch) {
                _nextSearch = MainWindow.Time + Tools.Random.NextDouble(0.25, 0.5);
                
                var dest = SearchNearbyVisibleEnts(8f, (ent, diff) =>
                    ent.HasComponent<WoodPile>() &&
                    ent.GetComponent<WoodPile>().Count < 8)
                    .OrderByDescending(x => x.GetComponent<WoodPile>().Count)
                    .FirstOrDefault();

                if (Human.IsHoldingItem) {
                    if (!Human.HeldItem.IsOfClass("plank")) {
                        _currTarget = null;
                        return false;
                    }

                    _currTarget = dest;
                } else {
                    var src = SearchNearbyVisibleEnts(8f, (ent, diff) =>
                        ent.HasComponent<WoodPile>() &&
                        ent.GetComponent<WoodPile>().Count > 0)
                        .OrderBy(x => x.GetComponent<WoodPile>().Count)
                        .FirstOrDefault();

                    if (src != null && dest != null && src != dest) {
                        _currTarget = src;
                    }
                }
            }

            if (_currTarget != null && _currTarget.IsValid) {
                if (Human.IsHoldingItem && _currTarget.GetComponent<Item>().CanPickup(Entity)) {
                    _currTarget.GetComponent<WoodPile>().AddPlank(Human.DropItem());
                    _currTarget = null;
                    return true;
                } else if (!Human.IsHoldingItem && _currTarget.GetComponent<Item>().CanPickup(Entity)) {
                    Human.PickupItem(_currTarget);
                    _currTarget = null;
                    return true;
                }

                Human.StartMoving(World.Difference(Position2D, _currTarget.Position2D));
                return true;
            }

            return false;
        }
    }
}
