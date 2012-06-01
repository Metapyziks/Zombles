using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Zombles.Entities
{
    public enum CollisionModel : byte
    {
        None = 0,
        Repel = 1,
        Box = 2
    }

    public class Collision : Component
    {
        public CollisionModel Model { get; set; }

        public Vector2 Size { get; private set; }
        public Vector2 Offset { get; private set; }

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
            Model = CollisionModel.None;
        }

        public Collision SetDimentions( float width, float height )
        {
            Size = new Vector2( width, height );
            Offset = new Vector2( -width / 2.0f, -height / 2.0f );
            return this;
        }

        public Collision SetDimentions( float width, float height, float offsx, float offsy )
        {
            Size = new Vector2( width, height );
            Offset = new Vector2( offsx, offsy );
            return this;
        }

        public Vector2 TryMove( Vector2 move )
        {
            if ( Model == CollisionModel.None )
                return move;

            NearbyEntityEnumerator iter = new NearbyEntityEnumerator( Entity.City,
                new Vector2( Entity.Position.X, Entity.Position.Z ), 2.0f );

            while ( iter.MoveNext() )
                move = TryMove( iter.Current, move );

            return move;
        }

        private Vector2 TryMove( Entity obstacle, Vector2 move )
        {
            if ( obstacle == Entity || !obstacle.HasComponent<Collision>() )
                return move;

            Collision that = obstacle.GetComponent<Collision>();

            if ( that.Model == CollisionModel.None )
                return move;

            Vector2 diff = City.Difference( Position2D, that.Position2D );

            float al = Offset.X;
            float ar = al + Size.X;
            float at = Offset.Y;
            float ab = at + Size.Y;

            float bl = diff.X + that.Offset.X;
            float br = bl + that.Size.X;
            float bt = diff.Y + that.Offset.Y;
            float bb = bt + that.Size.Y;

            bool intersecting = al < br && ar > bl && at < bb && ab > bt;

            float ix = 0.0f, iy = 0.0f;

            if ( intersecting )
            {
                float il = br - al;
                float ir = ar - bl;
                ix = ( il < ir ) ? il : -ir;

                float it = bb - at;
                float ib = ab - bt;
                iy = ( it < ib ) ? it : -ib;
            }

            if ( this.Model == CollisionModel.Box || that.Model == CollisionModel.Box )
            {
                if ( intersecting )
                {
                    if ( Math.Abs( ix ) <= Math.Abs( iy ) )
                        return new Vector2( ix, move.Y );
                    else
                        return new Vector2( move.X, iy );
                }

                if ( move.X > 0 )
                {
                    if ( ar < bl && ar + move.X > bl )
                    {
                        float dx = bl - ar;
                        float dy = ( dx / move.X ) * move.Y;

                        if ( at + dy < bb && ab + dy > bt )
                            return new Vector2( dx, move.Y );
                    }
                }
                else if ( move.X < 0 )
                {
                    if ( al > br && al + move.X < br )
                    {
                        float dx = br - al;
                        float dy = ( dx / move.X ) * move.Y;

                        if ( at + dy < bb && ab + dy > bt )
                            return new Vector2( dx, move.Y );
                    }
                }

                if ( move.Y > 0 )
                {
                    if ( at < bt && at + move.Y > bt )
                    {
                        float dy = bt - ab;
                        float dx = ( dy / move.Y ) * move.X;

                        if ( al + dx < br && ar + dx > bl )
                            return new Vector2( move.X, dy );
                    }
                }
                else if ( move.Y < 0 )
                {
                    if ( at > bb && at + move.Y < bb )
                    {
                        float dy = bb - at;
                        float dx = ( dy / move.Y ) * move.X;

                        if ( al + dx < br && ar + dx > bl )
                            return new Vector2( move.X, dy );
                    }
                }

                return move;
            }
            else
            {
                if ( intersecting )
                {
                    if ( Math.Abs( ix ) <= Math.Abs( iy ) )
                        return new Vector2( ix / 2.0f + move.X, move.Y );
                    else
                        return new Vector2( move.X, iy / 2.0f + move.Y );
                }

                return move;
            }
        }
    }
}
