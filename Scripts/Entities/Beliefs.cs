using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    internal sealed class Beliefs
    {
        private class BlockBeliefs
        {
            public double LastSeen { get; private set; }

            public Block Block { get; private set; }

            public int Survivors { get; private set; }

            public int Zombies { get; private set; }

            public int Resources { get; private set; }

            public bool HasBuilding { get; private set; }

            public double Utility { get; private set; }
        }
    }
}
