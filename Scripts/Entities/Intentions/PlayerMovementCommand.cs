using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Entities;

namespace Zombles.Scripts.Entities.Intentions
{
    public class PlayerMovementCommand : Intention
    {
        private RouteNavigator _nav;

        public PlayerMovementCommand(Desires.PlayerMovementCommand desire, Beliefs beliefs)
            : base(desire, beliefs)
        {
            _nav = new RouteNavigator(Entity, desire.Destination);
        }

        public override bool ShouldAbandon()
        {
            return !ShouldKeep();
        }

        public override bool ShouldKeep()
        {
            return !_nav.HasEnded;
        }

        public override IEnumerable<Action> GetActions()
        {
            if (_nav.HasDirection) {
                yield return new DropItemAction(8f);
                yield return new MovementAction(_nav.GetDirection().Normalized() * 4f);
            }
        }
    }
}
