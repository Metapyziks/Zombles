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
            get { return Entity.Position.Z + Offset.Y; }
        }
        public float Bottom
        {
            get { return Entity.Position.Z + Offset.Y + Size.Y; }
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

        public bool IsIntersecting( Collision other )
        {
            return this.Left < other.Right && this.Right > other.Left
                && this.Top < other.Bottom && this.Bottom > other.Top;
        }

        public Vector2 TryMove( Vector2 move )
        {
            NearbyEntityEnumerator iter = new NearbyEntityEnumerator( Entity.City,
                new Vector2( Entity.Position.X, Entity.Position.Z ), 2.0f );

            while ( iter.MoveNext() )
                move = TryMove( iter.Current, move );

            return move;
        }

        public Vector2 TryMove( Entity obstacle, Vector2 move )
        {
            if ( obstacle == Entity || !obstacle.HasComponent<Collision>() )
                return move;

            Collision that = obstacle.GetComponent<Collision>();

            if ( this.IsIntersecting( that ) )
            {
                float il = that.Right - this.Left;
                float ir = this.Right - that.Left;
                float ix = ( il < ir ) ? il : -ir;

                float it = that.Bottom - this.Top;
                float ib = this.Bottom - that.Top;
                float iy = ( it < ib ) ? it : -ib;

                if ( Math.Abs( ix ) <= Math.Abs( iy ) )
                    return new Vector2( ix, move.Y );
                else
                    return new Vector2( move.X, iy );
            }

            if ( move.X > 0 )
            {
                if ( this.Right < that.Left && this.Right + move.X > that.Left )
                {
                    float dx = that.Left - this.Right;
                    float dy = ( dx / move.X ) * move.Y;

                    if ( this.Top + dy < that.Bottom && this.Bottom + dy > that.Top )
                        return new Vector2( dx, move.Y );
                }
            }
            else if ( move.X < 0 )
            {
                if ( this.Left > that.Right && this.Left + move.X < that.Right )
                {
                    float dx = that.Right - this.Left;
                    float dy = ( dx / move.X ) * move.Y;

                    if ( this.Top + dy < that.Bottom && this.Bottom + dy > that.Top )
                        return new Vector2( dx, move.Y );
                }
            }

            if ( move.Y > 0 )
            {
                if ( this.Bottom < that.Top && this.Bottom + move.Y > that.Top )
                {
                    float dy = that.Top - this.Bottom;
                    float dx = ( dy / move.Y ) * move.X;

                    if ( this.Left + dx < that.Right && this.Right + dx > that.Left )
                        return new Vector2( move.X, dy );
                }
            }
            else if ( move.Y < 0 )
            {
                if ( this.Top > that.Bottom && this.Top + move.Y < that.Bottom )
                {
                    float dy = that.Bottom - this.Top;
                    float dx = ( dy / move.Y ) * move.X;

                    if ( this.Left + dx < that.Right && this.Right + dx > that.Left )
                        return new Vector2( move.X, dy );
                }
            }

            return move;
        }
    }
}
