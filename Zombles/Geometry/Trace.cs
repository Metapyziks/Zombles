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
            bool hitEnts = false, bool hitGeom = true, Vector2 hullSize = default( Vector2 ) )
        {
            return new Trace( city )
            {
                Origin = start,
                Target = end,
                HitEntities = hitEnts,
                HitGeometry = hitGeom,
                HullSize = hullSize
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

            HitGeometry = true;
            HitEntities = false;


            HullSize = new Vector2( 0.0f, 0.0f );
        }

        private Face GeometryTrace( ref Vector2 vec )
        {
            Face xf = ( vec.X > 0.0f ? Face.East : Face.West );
            Face yf = ( vec.Y > 0.0f ? Face.South : Face.North );

            float xm = 1.0f, ym = 1.0f;

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

            if ( xm < 1.0f || ym < 1.0f )
            {
                if ( xm <= ym )
                {
                    vec *= xm;
                    return xf;
                }
                else
                {
                    vec *= ym;
                    return yf;
                }
            }

            return Face.None;
        }

        private Face GeometryTraceHull( ref Vector2 vec )
        {
            Face xf = ( vec.X > 0.0f ? Face.East : Face.West );
            Face yf = ( vec.Y > 0.0f ? Face.South : Face.North );

            float xm = 1.0f, ym = 1.0f;

            Vector2 offset = -HullSize / 2.0f;

            float left = Origin.X + offset.X;
            float right = left + HullSize.X;
            float top = Origin.Y + offset.Y;
            float bottom = top + HullSize.Y;

            if ( vec.X != 0.0f )
            {
                float dydx = vec.Y / vec.X;

                float startX = vec.X > 0.0f ? right : left;
                int startIX = (int) ( vec.X > 0.0f ? Math.Ceiling( startX ) : Math.Floor( startX ) );
                float y = Origin.Y + ( startIX - startX ) * dydx;

                int xa = Math.Sign( vec.X );
                int wxa = ( vec.X > 0.0f ? -1 : 0 );
                int sxa = ( vec.X > 0.0f ? 0 : -1 );
                int xe = (int) ( vec.X > 0.0f ? Math.Ceiling( startX + vec.X ) : Math.Floor( startX + vec.X ) );

                Block blk = null;

                for ( int ix = startIX; ix != xe; ix += xa, y += xa * dydx )
                {
                    int wx = ( ix + wxa ) - (int) Math.Floor( (double) ( ix + wxa ) / City.Width ) * City.Width;
                    int sx = ( ix + sxa ) - (int) Math.Floor( (double) ( ix + sxa ) / City.Width ) * City.Width;

                    int minY = (int) Math.Floor( y + offset.Y );
                    int maxY = (int) Math.Floor( y + offset.Y + HullSize.Y );

                    for ( int iy = minY; iy <= maxY; ++iy )
                    {
                        int wy = iy - (int) Math.Floor( (double) iy / City.Height ) * City.Height;

                        if ( blk == null || wx < blk.X || wy < blk.Y ||
                                wx >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                            blk = City.GetBlock( wx, wy );

                        bool hit = false;

                        Tile tw = blk[ wx, wy ];

                        hit = tw.IsWallSolid( xf );

                        if ( !hit )
                        {
                            if ( sx < blk.X || wy < blk.Y ||
                                    sx >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                                blk = City.GetBlock( sx, wy );

                            Tile ts = blk[ sx, wy ];

                            hit = ( iy >= minY && ts.IsWallSolid( Face.North ) && !tw.IsWallSolid( Face.North ) ) ||
                                ( iy <= maxY && ts.IsWallSolid( Face.South ) && !tw.IsWallSolid( Face.South ) );
                        }

                        if ( hit )
                        {
                            xm = ( ix - startX ) / vec.X;
                            ix = xe - xa;
                            break;
                        }
                    }
                }
            }

            if ( vec.Y != 0.0f )
            {
                float dxdy = vec.X / vec.Y;

                float startY = vec.Y > 0.0f ? bottom : top;
                int startIY = (int) ( vec.Y > 0.0f ? Math.Ceiling( startY ) : Math.Floor( startY ) );
                float x = Origin.X + ( startIY - startY ) * dxdy;

                int ya = Math.Sign( vec.Y );
                int wya = ( vec.Y > 0.0f ? -1 : 0 );
                int sya = ( vec.Y > 0.0f ? 0 : -1 );
                int ye = (int) ( vec.Y > 0.0f ? Math.Ceiling( startY + vec.Y ) : Math.Floor( startY + vec.Y ) );

                Block blk = null;

                for ( int iy = startIY; iy != ye; iy += ya, x += ya * dxdy )
                {
                    int wy = ( iy + wya ) - (int) Math.Floor( (double) ( iy + wya ) / City.Height ) * City.Height;
                    int sy = ( iy + sya ) - (int) Math.Floor( (double) ( iy + sya ) / City.Height ) * City.Height;

                    int minX = (int) Math.Floor( x + offset.X );
                    int maxX = (int) Math.Floor( x + offset.X + HullSize.X );

                    for ( int ix = minX; ix <= maxX; ++ix )
                    {
                        int wx = ix - (int) Math.Floor( (double) ix / City.Width ) * City.Width;

                        if ( blk == null || wx < blk.X || wy < blk.Y ||
                                wx >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                            blk = City.GetBlock( wx, wy );

                        bool hit = false;

                        Tile tw = blk[ wx, wy ];

                        hit = tw.IsWallSolid( yf );

                        if ( !hit )
                        {
                            if ( wx < blk.X || sy < blk.Y ||
                                    wx >= blk.X + blk.Width || sy >= blk.Y + blk.Height )
                                blk = City.GetBlock( wx, sy );

                            Tile ts = blk[ wx, sy ];

                            hit = ( ix >= minX && ts.IsWallSolid( Face.West ) && !tw.IsWallSolid( Face.West ) ) ||
                                ( ix <= maxX && ts.IsWallSolid( Face.East ) && !tw.IsWallSolid( Face.East ) );
                        }

                        if ( hit )
                        {
                            ym = ( iy - startY ) / vec.Y;
                            iy = ye - ya;
                            break;
                        }
                    }
                }
            }

            if ( xm < 1.0f || ym < 1.0f )
            {
                if ( xm <= ym )
                {
                    vec *= xm;
                    return xf;
                }
                else
                {
                    vec *= ym;
                    return yf;
                }
            }

            return Face.None;
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
                hitFace = HullSize.LengthSquared == 0 ? GeometryTrace( ref vec ) : GeometryTraceHull( ref vec );

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
