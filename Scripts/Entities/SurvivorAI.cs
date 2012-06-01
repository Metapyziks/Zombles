using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public class SurvivorAI : AI
    {
        private const double TargetSearchInterval = 0.25;

        private float myFleeRadius;
        private float myRunRadius;

        private Vector2 myFleePos;
        private double myLastSearch;

        public SurvivorAI( Entity ent )
            : base( ent )
        {
            myFleeRadius = 4.0f + Tools.Random.NextSingle() * 6.0f;
            myRunRadius = 2.0f  + Tools.Random.NextSingle() * 4.0f;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            myFleePos = new Vector2();
            myLastSearch = ZomblesGame.Time - Tools.Random.NextDouble() * TargetSearchInterval;
        }

        public override void OnThink( double dt )
        {
            if ( !Human.Health.IsAlive )
                return;

            if ( ZomblesGame.Time - myLastSearch > TargetSearchInterval )
            {
                myLastSearch = ZomblesGame.Time + Tools.Random.NextSingle() * TargetSearchInterval;

                myFleePos = new Vector2();

                NearbyEntityEnumerator it = SearchNearbyEnts( myFleeRadius );
                while ( it.MoveNext() )
                {
                    Entity cur = it.Current;
                    if ( cur.HasComponent<Zombie>() )
                    {
                        Vector2 diff = cur.Position2D - Position2D;
                        float dist2 = diff.LengthSquared;

                        if ( dist2 > 0 )
                        {
                            myFleePos -= diff / dist2;

                            if ( dist2 < myRunRadius * myRunRadius )
                                ( Human as Survivor ).StartRunning();
                        }
                    }
                }

                if ( myFleePos.LengthSquared == 0.0f )
                {
                    myFleePos.X = Tools.Random.NextSingle() * 2.0f - 1.0f;
                    myFleePos.Y = Tools.Random.NextSingle() * 2.0f - 1.0f;
                }

                Human.StartMoving( myFleePos );
            }
        }
    }
}
