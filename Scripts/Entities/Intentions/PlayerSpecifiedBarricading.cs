using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zombles.Scripts.Entities.Intentions
{
    public class PlayerSpecifiedBarricading : Intention
    {
        public PlayerSpecifiedBarricading(Desires.PlayerSpecifiedBarricading desire, Beliefs beliefs)
            : base(desire, beliefs)
        {

        }

        public override bool ShouldAbandon()
        {
            return false;
        }

        public override bool ShouldKeep()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Action> GetActions()
        {
            throw new NotImplementedException();
        }
    }
}
