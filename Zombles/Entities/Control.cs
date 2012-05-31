using System;

using OpenTK;

namespace Zombles.Entities
{
    public class Control : Component
    {
        private static Random stRand = new Random();

        private Movement myMovement;
        private RenderAnim myAnim;
        private float myDirection;

        private float myMoveSpeed;

        public Control( Entity ent )
            : base( ent )
        {
            myMoveSpeed = stRand.NextSingle() + 1.0f;
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
            myAnim.Speed = myMoveSpeed / 2.0;
        }

        public override void OnThink( double dt )
        {
            Vector2 dir = new Vector2();
            dir.X = (float) ( Math.Cos( myDirection ) * dt ) * myMoveSpeed;
            dir.Y = (float) ( Math.Sin( myDirection ) * dt ) * myMoveSpeed;
            myMovement.Move( dir );
        }
    }
}
