using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    public class ZombieAI : HumanControl
    {
        private const double TargetSearchInterval = 1.0;
        private const double AttackInterval = 1.0;

        private float myViewRadius;

        private double myLastSearch;
        private double myLastAttack;
        private double myLastSeen;
        private Entity myCurTarget;
        private Vector2 myLastSeenPos;

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

            if ( myCurTarget != null )
            {
                Vector2 diff = City.Difference( Position2D, myCurTarget.Position2D );

                Health targHealth = myCurTarget.GetComponent<Health>();

                if ( !myCurTarget.HasComponent<Survivor>() || !targHealth.IsAlive || diff.LengthSquared > myViewRadius * myViewRadius )
                    myCurTarget = null;
                else
                {
                    myLastSeenPos = myCurTarget.Position2D;
                    myLastSeen = ZomblesGame.Time;

                    if ( ZomblesGame.Time - myLastAttack > AttackInterval )
                    {
                        if ( diff.LengthSquared < 1.0f )
                        {
                            targHealth.Damage( Tools.Random.Next( 10, 25 ), Entity );
                            myLastAttack = ZomblesGame.Time;
                            Human.StopMoving();
                        }
                        else
                        {
                            Human.StartMoving( diff );
                        }
                    }
                }
            }
            else
            {
                if ( ( ZomblesGame.Time - myLastSeen ) > 10.0 ||
                    City.Difference( Position2D, myLastSeenPos ).LengthSquared <= 1.0f )
                {
                    int attempts = 0;
                    while ( attempts++ < 16 )
                    {
                        float rad = 2.0f + Tools.Random.NextSingle() * 6.0f;
                        double ang = Tools.Random.NextDouble() * Math.PI * 2.0;

                        myLastSeenPos = new Vector2(
                            Position2D.X + (float) Math.Cos( ang ) * rad,
                            Position2D.Y + (float) Math.Sin( ang ) * rad
                        );

                        Trace trace = new Trace( City );
                        trace.Origin = Position2D;
                        trace.Target = myLastSeenPos;
                        trace.HitGeometry = true;
                        trace.HitEntities = false;

                        if ( !trace.GetResult().Hit )
                            break;
                    }

                    if ( attempts == 16 )
                        myLastSeen = ZomblesGame.Time + Tools.Random.NextDouble() * 1.0 + 9.0;
                    else
                        myLastSeen = ZomblesGame.Time;
                }

                Human.StartMoving( City.Difference( Position2D, myLastSeenPos ) );
            }
        }

        private void FindTarget()
        {
            Trace trace = new Trace( City );
            trace.Origin = Position2D;
            trace.HitGeometry = true;
            trace.HitEntities = false;

            Entity closest = null;
            float bestDist2 = myViewRadius * myViewRadius;

            NearbyEntityEnumerator it = SearchNearbyEnts( myViewRadius );
            while ( it.MoveNext() )
            {
                if ( !it.Current.HasComponent<Survivor>() )
                    continue;

                if ( !it.Current.GetComponent<Health>().IsAlive )
                    continue;

                Vector2 diff = City.Difference( Position2D, it.Current.Position2D );

                float dist2 = diff.LengthSquared;
                if ( dist2 < bestDist2 )
                {
                    trace.Target = it.Current.Position2D;

                    if ( !trace.GetResult().Hit )
                    {
                        closest = it.Current;
                        bestDist2 = dist2;
                    }
                }
            }

            myCurTarget = closest;
            myLastSearch = ZomblesGame.Time;
        }
    }
}
