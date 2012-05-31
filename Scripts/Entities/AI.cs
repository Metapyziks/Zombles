using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public class AI : Component
    {
        protected Human Human { get; private set; }

        public AI( Entity ent )
            : base( ent )
        {

        }

        public override void OnSpawn()
        {
            if ( Entity.HasComponent<Human>() )
                Human = Entity.GetComponent<Human>();
        }

        protected NearbyEntityEnumerator SearchNearbyEnts( float radius )
        {
            return new NearbyEntityEnumerator( City, Position2D, radius );
        }
    }
}
