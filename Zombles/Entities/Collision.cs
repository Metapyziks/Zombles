using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Zombles.Entities
{
    public class Collision : Component
    {
        public Vector2 Size { get; set; }
        public Vector2 Offset { get; set; }

        public float Left
        {
            get { return Entity.Position.X + Offset.X; }
        }
        public float Right
        {
            get { return Entity.Position.X + Offset.X + Size.X; }
        }
        public float Top
        {
            get { return Entity.Position.Y + Offset.Y; }
        }
        public float Bottom
        {
            get { return Entity.Position.Y + Offset.Y + Size.Y; }
        }

        public Collision( Entity ent )
            : base( ent )
        {

        }

        public void SetDimentions( float width, float height )
        {
            Size = new Vector2( width, height );
            Offset = new Vector2( -width / 2.0f, -height / 2.0f );
        }

        public void SetDimentions( float width, float height, float offsx, float offsy )
        {
            Size = new Vector2( width, height );
            Offset = new Vector2( offsx, offsy );
        }

        public Vector2 TryMove( Entity ent, Vector2 move )
        {
            if ( ent == Entity || !ent.HasComponent<Collision>() )
                return move;

            Collision that = ent.GetComponent<Collision>();
            float delta = 1.0f;

            if ( move.X > 0 )
            {
                if ( this.Right > that.Left )
                    delta = Math.Min( delta, ( this.Left - that.Right ) / move.X );
            }
            else if ( move.X < 0 )
            {
                if ( this.Left < that.Right )
                    delta = Math.Min( delta, ( this.Right - that.Left ) / move.X );
            }

            if ( move.Y > 0 )
            {
                if ( this.Bottom > that.Top )
                    delta = Math.Min( delta, ( this.Top - that.Bottom ) / move.Y );
            }
            else if ( move.Y < 0 )
            {
                if ( this.Top < that.Bottom )
                    delta = Math.Min( delta, ( this.Bottom - that.Top ) / move.Y );
            }

            return move * delta;
        }
    }
}
