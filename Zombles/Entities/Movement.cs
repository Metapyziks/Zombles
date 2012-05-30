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

        public Movement( Entity ent )
            : base( ent )
        {

        }

        public override void OnSpawn()
        {
            myCollision = null;

            if ( Entity.HasComponent<Collision>() )
                myCollision = Entity.GetComponent<Collision>();
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
