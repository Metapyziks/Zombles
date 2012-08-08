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
        public static TraceResult Quick( City city, Vector2 start, Vector2 end,
            bool hitEnts = false, bool hitGeom = true )
        {
            return new Trace( city )
            {
                Origin = start,
                Target = end,
                HitEntities = hitEnts,
                HitGeometry = hitGeom
            }.GetResult();
        }

        private Vector2 myNormal; 

        public readonly City City;

        public Vector2 Origin { get; set; }
        public Vector2 Normal
        {
            get { return myNormal; }
            set
            {
                myNormal = value;
                if ( myNormal.LengthSquared == 0.0f )
                    myNormal = new Vector2( 1.0f, 0.0f );
                else
                    myNormal.Normalize();
            }
        }

        public float Length { get; set; }

        public Vector2 HullSize { get; set; }

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

            HullSize = new Vector2( 0.0f, 0.0f );

            HitGeometry = true;
            HitEntities = false;
        }

        private Face GeometryTrace( ref Vector2 vec )
        {
            Face xf = ( vec.X > 0.0f ? Face.East : Face.West );
            Face yf = ( vec.Y > 0.0f ? Face.South : Face.North );

            float xm = 1.0f, ym = 1.0f;
            Face hitFace = Face.None;

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
                        hitFace = xf;
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
                        hitFace = yf;
                        break;
                    }
                }
            }

            if ( hitFace == xf )
                vec *= xm;
            else if( hitFace == yf )
                vec *= ym;

            return hitFace;
        }

        private Entity EntityTrace( ref Vector2 vec )
        {
            NearbyEntityEnumerator it = new NearbyEntityEnumerator(
                    City, Origin + vec / 2.0f, vec.Length / 2.0f + 2.0f );

            float ratio = 1.0f;
            float dydx = vec.Y / vec.X;
            float dxdy = vec.X / vec.Y;

            Entity hitEnt = null;

            while ( it.MoveNext() )
            {
                Entity ent = it.Current;
                if ( ent.HasComponent<Collision>() )
                {
                    Collision col = ent.GetComponent<Collision>();
                    if ( col.Model != CollisionModel.None &&
                        ( HitEntityPredicate == null || HitEntityPredicate( ent ) ) )
                    {
                        Vector2 diff = City.Difference( Origin, ent.Position2D );

                        float lt = diff.X + col.Offset.X;
                        float rt = lt + col.Size.X;
                        float tp = diff.Y + col.Offset.Y;
                        float bt = tp + col.Size.Y;

                        if ( vec.X != 0.0f )
                        {
                            float dx = 0.0f;
                            bool hit = false;
                            if ( vec.X > 0.0f && lt >= 0.0f && vec.X * ratio > lt )
                            {
                                dx = lt;
                                hit = true;
                            }
                            else if ( vec.X < 0.0f && rt <= 0.0f && vec.X * ratio < rt )
                            {
                                dx = rt;
                                hit = true;
                            }

                            if ( hit )
                            {
                                float y = dydx * dx;

                                if ( y > tp && y < bt )
                                {
                                    ratio = dx / vec.X;
                                    hitEnt = ent;
                                }
                            }
                        }

                        if ( vec.Y != 0.0f )
                        {
                            float dy = 0.0f;
                            bool hit = false;
                            if ( vec.Y > 0.0f && tp >= 0.0f && vec.Y * ratio > tp )
                            {
                                dy = tp;
                                hit = true;
                            }
                            else if ( vec.Y < 0.0f && bt <= 0.0f && vec.Y * ratio < bt )
                            {
                                dy = bt;
                                hit = true;
                            }

                            if ( hit )
                            {
                                float x = dxdy * dy;

                                if ( x > lt && x < rt )
                                {
                                    ratio = dy / vec.Y;
                                    hitEnt = ent;
                                }
                            }
                        }
                    }
                }
            }

            if ( hitEnt != null )
                vec *= ratio;

            return hitEnt;
        }

        public TraceResult GetResult()
        {
            Vector2 vec = ( Length == 0.0f ? Math.Max( City.Width, City.Height ) : Length ) * Normal;
            
            Face hitFace = Face.None;
            if ( HitGeometry )
                hitFace = GeometryTrace( ref vec );

            Entity hitEnt = null;
            if ( HitEntities )
                hitEnt = EntityTrace( ref vec );

            if ( hitEnt != null )
                return new TraceResult( this, vec, hitEnt );
            else if ( hitFace != Face.None )
                return new TraceResult( this, vec, hitFace );
            else
                return new TraceResult( this, vec );
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
