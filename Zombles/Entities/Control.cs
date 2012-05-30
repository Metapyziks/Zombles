using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Zombles.Entities
{
    public class Control : Component
    {
        private Movement myMovement;
        private RenderAnim myAnim;

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
        }

        public override void OnThink( double dt )
        {
            Vector2 dir = new Vector2();
            dir.X = (float) ( Math.Cos( myAnim.Rotation ) * 2.0 * dt );
            dir.Y = (float) ( Math.Sin( myAnim.Rotation ) * 2.0 * dt );
            myMovement.Move( dir );
        }
    }
}
