using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Entities;

namespace Zombles.Geometry
{
    public class Trace
    {
        private Vector2 myNormal; 

        public readonly City City;

        public Vector2 Origin { get; set; }
        public Vector2 Normal
        {
            get { return myNormal; }
            set
            {
                myNormal = value;
                myNormal.Normalize();
            }
        }

        public float Length { get; set; }

        public bool HitEntities { get; set; }
        public bool HitGeometry { get; set; }

        public Predicate<Entity> HitEntityPredicate { get; set; }

        public Vector2 Target
        {
            get { return City.Wrap( Origin + Vector ); }
            set
            {
                Vector = City.Difference( Origin, value );
            }
        }

        public Vector2 Vector
        {
            get { return Normal * Length; }
            set
            {
                Normal = value;
                Length = value.Length;
            }
        }

        public Trace( City city )
        {
            City = city;

            HitGeometry = true;
            HitEntities = false;
        }

        public TraceResult GetResult()
        {
            Vector2 vec = ( Length == 0.0f ? Math.Max( City.Width, City.Height ) : Length ) * Normal;

            float xm = 1.0f, ym = 1.0f;

            Face xf = ( vec.X > 0.0f ? Face.East : Face.West );
            Face yf = ( vec.Y > 0.0f ? Face.South : Face.North );

            if ( HitGeometry )
            {
                if ( vec.X != 0.0f )
                {
                    float dydx = vec.Y / vec.X;

                    int startX = (int) ( vec.X > 0.0f ? Math.Ceiling( Origin.X ) : Math.Floor( Origin.X ) );
                    float y = Origin.Y + ( startX - Origin.X ) * dydx;

                    int xa = ( vec.X > 0.0f ? 1 : -1 );
                    int wxa = ( vec.X > 0.0f ? -1 : 0 );
                    int xe = (int) ( vec.X > 0.0f ? Math.Ceiling( Origin.X + vec.X ) : Math.Floor( Origin.X + vec.X ) );

                    Block blk = null;

                    for ( int ix = startX; ix != xe; ix += xa, y += xa * dydx )
                    {
                        int wx = ( ix + wxa ) - (int) Math.Floor( (double) ( ix + wxa ) / City.Width ) * City.Width;
                        int wy = (int) ( y - (int) Math.Floor( y / City.Height ) * City.Height );

                        if ( blk == null || wx < blk.X || wy < blk.Y ||
                                wx >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                            blk = City.GetBlock( wx, wy );

                        if ( blk[ wx, wy ].IsWallSolid( xf ) )
                        {
                            xm = ( ix - Origin.X ) / vec.X;
                            break;
                        }
                    }
                }

                if ( vec.Y != 0.0f )
                {
                    float dxdy = vec.X / vec.Y;

                    int startY = (int) ( vec.Y > 0.0f ? Math.Ceiling( Origin.Y ) : Math.Floor( Origin.Y ) );
                    float x = Origin.X + ( startY - Origin.Y ) * dxdy;

                    int ya = ( vec.Y > 0.0f ? 1 : -1 );
                    int wya = ( vec.Y > 0.0f ? -1 : 0 );
                    int ye = (int) ( vec.Y > 0.0f ? Math.Ceiling( Origin.Y + vec.Y ) : Math.Floor( Origin.Y + vec.Y ) );

                    Block blk = null;

                    for ( int iy = startY; iy != ye; iy += ya, x += ya * dxdy )
                    {
                        int wy = ( iy + wya ) - (int) Math.Floor( (double) ( iy + wya ) / City.Height ) * City.Height;
                        int wx = (int) ( x - (int) Math.Floor( x / City.Width ) * City.Width );

                        if ( blk == null || wx < blk.X || wy < blk.Y ||
                                wx >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                            blk = City.GetBlock( wx, wy );

                        if ( blk[ wx, wy ].IsWallSolid( yf ) )
                        {
                            ym = ( iy - Origin.Y ) / vec.Y;
                            break;
                        }
                    }
                }
            }

            if ( xm == 1.0f && ym == 1.0f )
                return new TraceResult( this, vec );

            if ( xm <= ym )
                return new TraceResult( this, xm * vec, xf );
            else
                return new TraceResult( this, ym * vec, yf );
        }
    }

    public class TraceResult
    {
        public readonly City City;

        public Vector2 Origin { get; private set; }
        public Vector2 Normal { get; private set; }
        public Vector2 Target { get; private set; }
        public Vector2 Vector { get; private set; }

        public float Length
        {
            get { return Vector.Length; }
        }

        public Vector2 End
        {
            get { return City.Wrap( Origin + Vector ); }
        }

        public float Ratio
        {
            get { return Length / ( Target - Origin ).Length; }
        }

        public bool Hit
        {
            get { return HitEntity || HitWorld; }
        }

        public bool HitEntity { get; private set; }
        public bool HitWorld { get; private set; }

        public Face Face { get; private set; }
        public Entity Entity { get; private set; }

        internal TraceResult( Trace trace, Vector2 vec )
        {
            City = trace.City;

            Origin = trace.Origin;
            Target = trace.Target;
            Normal = trace.Normal;

            Vector = vec;
        }

        internal TraceResult( Trace trace, Vector2 vec, Entity ent )
            : this( trace, vec )
        {
            HitEntity = true;
            Entity = ent;
        }

        internal TraceResult( Trace trace, Vector2 vec, Face face )
            : this( trace, vec )
        {
            HitWorld = true;
            Face = face;
        }
    }
}
