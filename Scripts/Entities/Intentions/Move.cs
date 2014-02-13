using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Zombles.Scripts.Entities.Intentions
{
    class Move : Intention
    {
        private Vector2 _direction;

        public Move(Beliefs beliefs, Vector2 direction) : base(beliefs)
        {
            _direction = direction;
        }

        public override bool Act()
        {

        }
    }
}
