using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public abstract class Intention
    {
        public Beliefs Beliefs { get; private set; }

        protected Entity Agent { get { return Beliefs.Agent; } }

        public Intention(Beliefs beliefs)
        {
            Beliefs = beliefs;
        }

        public abstract bool Act();
    }
}
