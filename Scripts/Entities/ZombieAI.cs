using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public class ZombieAI : AI
    {
        private const double TargetSearchInterval = 1.0;

        private double myLastSearch;
        private Entity myCurTarget;

        public ZombieAI( Entity ent )
            : base( ent )
        {

        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            myLastSearch = ZomblesGame.Time;
            myCurTarget = null;
        }

        public override void OnThink( double dt )
        {
            if ( myCurTarget == null )
            {
                if ( ZomblesGame.Time - myLastSearch > TargetSearchInterval )
                    FindTarget();
            }
            else
            {
                Vector2 diff = myCurTarget.Position2D - Position2D;

                if ( diff.LengthSquared > Human.ViewRadius * Human.ViewRadius )
                    myCurTarget = null;
                else
                    Human.StartMoving( myCurTarget.Position2D - Position2D );
            }
        }

        private void FindTarget()
        {
            Entity closest = null;
            float bestDist = Human.ViewRadius * Human.ViewRadius;

            NearbyEntityEnumerator it = SearchNearbyEnts();
            while ( it.MoveNext() )
            {
                if ( !it.Current.HasComponent<Survivor>() )
                    continue;

                float dist = ( it.Current.Position2D - Position2D ).LengthSquared;
                if ( dist < bestDist )
                {
                    closest = it.Current;
                    bestDist = dist;
                }
            }

            myCurTarget = closest;
            myLastSearch = ZomblesGame.Time;
        }
    }
}
