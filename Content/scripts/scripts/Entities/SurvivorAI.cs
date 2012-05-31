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
        private double myNextTurnTime;

        public SurvivorAI( Entity ent )
            : base( ent )
        {

        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            ChangeDirection();
        }

        public override void OnThink( double dt )
        {
            if ( ZomblesGame.Time >= myNextTurnTime )
                ChangeDirection();
        }

        private void ChangeDirection()
        {
            Human.StartMoving( new Vector2( Tools.Random.NextSingle() - 0.5f, Tools.Random.NextSingle() - 0.5f ) );
            myNextTurnTime = ZomblesGame.Time + Tools.Random.NextDouble() * 4.0 + 1.0;
        }
    }
}
