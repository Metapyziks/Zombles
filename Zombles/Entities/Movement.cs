using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Zombles.Entities
{
    public class Movement : Component
    {
        private Collision myCollision;
        private Vector2 myVelocity;

        public Vector2 Velocity
        {
            get { return myVelocity; }
            set
            {
                myVelocity = value;
                IsMoving = myVelocity.X != 0.0f || myVelocity.Y != 0.0f;
            }
        }
        public bool IsMoving { get; private set; }

        public Movement( Entity ent )
            : base( ent )
        {
            Velocity = new Vector2();
        }

        public override void OnSpawn()
        {
            myCollision = null;

            if ( Entity.HasComponent<Collision>() )
                myCollision = Entity.GetComponent<Collision>();
        }

        public void Stop()
        {
            Velocity = new Vector2();
        }

        public override void OnThink( double dt )
        {
            if ( Velocity.LengthSquared > 0.0f )
                Move( Velocity * (float) dt );
        }

        public void Move( Vector2 move )
        {
            if ( myCollision != null )
                move = myCollision.TryMove( move );

            Vector3 pos = Entity.Position;
            pos.X += move.X;
            pos.Z += move.Y;
            Entity.Position = pos;
        }
    }
}
