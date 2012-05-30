using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Zombles.Entities
{
    public class Control : Component
    {
        private static Random stRand = new Random();

        private Movement myMovement;
        private RenderAnim myAnim;
        private float myDirection;

        public Control( Entity ent )
            : base( ent )
        {

        }

        public override void OnSpawn()
        {
            myMovement = null;
            myAnim = null;

            if( Entity.HasComponent<Movement>() )
                myMovement = Entity.GetComponent<Movement>();

            if ( Entity.HasComponent<RenderAnim>() )
                myAnim = Entity.GetComponent<RenderAnim>();

            myDirection = myAnim.Rotation = stRand.NextSingle() * MathHelper.TwoPi - MathHelper.Pi;
        }

        public override void OnThink( double dt )
        {
            Vector2 dir = new Vector2();
            dir.X = (float) ( Math.Cos( myDirection ) * 2.0 * dt );
            dir.Y = (float) ( Math.Sin( myDirection ) * 2.0 * dt );
            myMovement.Move( dir );
        }
    }
}
