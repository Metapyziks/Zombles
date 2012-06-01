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
        private const double AttackInterval = 1.0;

        private float myViewRadius;

        private double myLastSearch;
        private double myLastAttack;
        private Entity myCurTarget;

        public ZombieAI( Entity ent )
            : base( ent )
        {
            myViewRadius = 12.0f;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            myLastSearch = ZomblesGame.Time - Tools.Random.NextDouble() * TargetSearchInterval;
            myCurTarget = null;
        }

        public override void OnThink( double dt )
        {
            if ( !Human.Health.IsAlive )
                return;

            if ( ZomblesGame.Time - myLastSearch > TargetSearchInterval )
                FindTarget();
            
            if( myCurTarget != null )
            {
                Vector2 diff = myCurTarget.Position2D - Position2D;

                Health targHealth = myCurTarget.GetComponent<Health>();

                if ( !myCurTarget.HasComponent<Survivor>() || !targHealth.IsAlive || diff.LengthSquared > myViewRadius * myViewRadius )
                    myCurTarget = null;
                else if ( ZomblesGame.Time - myLastAttack > AttackInterval )
                {
                    if ( diff.LengthSquared < 1.0f )
                    {
                        targHealth.Damage( Tools.Random.Next( 10, 25 ), Entity );
                        myLastAttack = ZomblesGame.Time;
                        Human.StopMoving();
                    }
                    else
                        Human.StartMoving( myCurTarget.Position2D - Position2D );
                }
            }
        }

        private void FindTarget()
        {
            Entity closest = null;
            float bestDist = myViewRadius * myViewRadius;

            NearbyEntityEnumerator it = SearchNearbyEnts( myViewRadius );
            while ( it.MoveNext() )
            {
                if ( !it.Current.HasComponent<Survivor>() )
                    continue;

                if ( !it.Current.GetComponent<Health>().IsAlive )
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
