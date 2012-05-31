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

        public Vector2 Velocity { get; set; }

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
