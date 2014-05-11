using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Zombles.Entities;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class PlayerMovementCommand : SubsumptionStack.Layer
    {
        private RouteNavigator _nav;

        public Vector2 Destination
        {
            get { return _nav == null ? Vector2.Zero : _nav.CurrentTarget; }
            set
            {
                _nav = new RouteNavigator(Entity, value);
            }
        }

        protected override bool OnThink(double dt)
        {
            if (_nav != null) {
                if (_nav.HasEnded) {
                    _nav.Dispose();
                    _nav = null;
                } else if (_nav.HasDirection) {
                    Human.StartMoving(_nav.GetDirection());
                    return true;
                }
            }
            
            return false;
        }
    }
}
