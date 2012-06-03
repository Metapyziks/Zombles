﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Geometry;

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
                new Vector2( Entity.Position.X, Entity.Position.Z ), 2.0f + move.Length );

            while ( iter.MoveNext() )
                move = TryMove( iter.Current, move );

            float xm = 1.0f, ym = 1.0f;

            if ( move.X > 0 )
            {
                float dydx = move.Y / move.X;

                int startX = (int) Math.Ceiling( Right );
                float y = Position.Z + ( startX - Right ) * dydx;

                Block blk = null;

                for ( int ix = startX; ix < Right + move.X; ++ix, y += dydx )
                {
                    int wx = ( ix - 1 ) - (int) Math.Floor( (double) ( ix - 1 ) / City.Width ) * City.Width;
                    int sx = ix - (int) Math.Floor( (double) ix / City.Width ) * City.Width;

                    int minY = (int) Math.Floor( y + Offset.Y );
                    int maxY = (int) Math.Floor( y + Offset.Y + Size.Y );

                    for ( int iy = minY; iy <= maxY; ++iy )
                    {
                        int wy = iy - (int) Math.Floor( (double) iy / City.Height ) * City.Height;

                        if ( blk == null || wx < blk.X || wy < blk.Y ||
                                wx >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                            blk = City.GetBlock( wx, wy );

                        bool hit = false;

                        Tile tw = blk[ wx, wy ];

                        hit = tw.IsWallSolid( Face.East );

                        if( !hit )
                        {
                            if ( sx < blk.X || wy < blk.Y ||
                                    sx >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                                blk = City.GetBlock( sx, wy );

                            Tile ts = blk[ sx, wy ];

                            hit = ( iy > minY && ts.IsWallSolid( Face.North ) && !tw.IsWallSolid( Face.North ) ) ||
                                ( iy < maxY && ts.IsWallSolid( Face.South ) && !tw.IsWallSolid( Face.South ) );
                        }

                        if ( hit )
                        {
                            xm = ( ix - Right ) / move.X;
                            ix = (int) Math.Ceiling( Right + move.X );
                            break;
                        }
                    }
                }
            }
            else if ( move.X < 0 )
            {
                float dydx = move.Y / -move.X;

                int startX = (int) Math.Floor( Left );
                float y = Position.Z + ( Left - startX ) * dydx;

                Block blk = null;

                for ( int ix = startX; ix > Left + move.X; --ix, y += dydx )
                {
                    int wx = ix - (int) Math.Floor( (double) ix / City.Width ) * City.Width;
                    int sx = ( ix - 1 ) - (int) Math.Floor( (double) ( ix - 1 ) / City.Width ) * City.Width;

                    int minY = (int) Math.Floor( y + Offset.Y );
                    int maxY = (int) Math.Floor( y + Offset.Y + Size.Y );

                    for ( int iy = minY; iy <= maxY; ++iy )
                    {
                        int wy = iy - (int) Math.Floor( (double) iy / City.Height ) * City.Height;

                        if ( blk == null || wx < blk.X || wy < blk.Y ||
                                wx >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                            blk = City.GetBlock( wx, wy );

                        bool hit = false;

                        Tile tw = blk[ wx, wy ];

                        hit = tw.IsWallSolid( Face.West );

                        if ( !hit )
                        {
                            if ( sx < blk.X || wy < blk.Y ||
                                    sx >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                                blk = City.GetBlock( sx, wy );

                            Tile ts = blk[ sx, wy ];

                            hit = ( iy > minY && ts.IsWallSolid( Face.North ) && !tw.IsWallSolid( Face.North ) ) ||
                                ( iy < maxY && ts.IsWallSolid( Face.South ) && !tw.IsWallSolid( Face.South ) );
                        }

                        if ( hit )
                        {
                            xm = ( ix - Left ) / move.X;
                            ix = (int) Math.Floor( Left + move.X );
                            break;
                        }
                    }
                }
            }

            if ( move.Y > 0 )
            {
                float dxdy = move.X / move.Y;

                int startY = (int) Math.Ceiling( Bottom );
                float x = Position.X + ( startY - Bottom ) * dxdy;

                Block blk = null;

                for ( int iy = startY; iy < Bottom + move.Y; ++iy, x += dxdy )
                {
                    int wy = ( iy - 1 ) - (int) Math.Floor( (double) ( iy - 1 ) / City.Height ) * City.Height;
                    int sy = iy - (int) Math.Floor( (double) iy / City.Height ) * City.Height;

                    int minX = (int) Math.Floor( x + Offset.X );
                    int maxX = (int) Math.Floor( x + Offset.X + Size.X );

                    for ( int ix = minX; ix <= maxX; ++ix )
                    {
                        int wx = ix - (int) Math.Floor( (double) ix / City.Width ) * City.Width;

                        if ( blk == null || wx < blk.X || wy < blk.Y ||
                                wx >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                            blk = City.GetBlock( wx, wy );

                        bool hit = false;

                        Tile tw = blk[ wx, wy ];

                        hit = tw.IsWallSolid( Face.South );

                        if ( !hit )
                        {
                            if ( sy < blk.X || wy < blk.Y ||
                                    sy >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                                blk = City.GetBlock( wx, sy );

                            Tile ts = blk[ wx, sy ];

                            hit = ( ix > minX && ts.IsWallSolid( Face.West ) && !tw.IsWallSolid( Face.West ) ) ||
                                ( ix < maxX && ts.IsWallSolid( Face.East ) && !tw.IsWallSolid( Face.East ) );
                        }

                        if ( hit )
                        {
                            ym = ( iy - Bottom ) / move.Y;
                            iy = (int) Math.Ceiling( Bottom + move.Y );
                            break;
                        }
                    }
                }
            }
            else if ( move.Y < 0 )
            {
                float dxdy = move.X / -move.Y;

                int startY = (int) Math.Floor( Top );
                float x = Position.X + ( Top - startY ) * dxdy;

                Block blk = null;

                for ( int iy = startY; iy > Top + move.Y; --iy, x += dxdy )
                {
                    int wy = iy - (int) Math.Floor( (double) iy / City.Height ) * City.Height;
                    int sy = ( iy - 1 ) - (int) Math.Floor( (double) ( iy - 1 ) / City.Height ) * City.Height;

                    int minX = (int) Math.Floor( x + Offset.X );
                    int maxX = (int) Math.Floor( x + Offset.X + Size.X );

                    for ( int ix = minX; ix <= maxX; ++ix )
                    {
                        int wx = ix - (int) Math.Floor( (double) ix / City.Width ) * City.Width;

                        if ( blk == null || wx < blk.X || wy < blk.Y ||
                                wx >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                            blk = City.GetBlock( wx, wy );

                        bool hit = false;

                        Tile tw = blk[ wx, wy ];

                        hit = tw.IsWallSolid( Face.North );

                        if ( !hit )
                        {
                            if ( sy < blk.X || wy < blk.Y ||
                                    sy >= blk.X + blk.Width || wy >= blk.Y + blk.Height )
                                blk = City.GetBlock( wx, sy );

                            Tile ts = blk[ wx, sy ];

                            hit = ( ix > minX && ts.IsWallSolid( Face.West ) && !tw.IsWallSolid( Face.West ) ) ||
                                ( ix < maxX && ts.IsWallSolid( Face.East ) && !tw.IsWallSolid( Face.East ) );
                        }

                        if ( hit )
                        {
                            ym = ( iy - Top ) / move.Y;
                            iy = (int) Math.Ceiling( Top + move.Y );
                            break;
                        }
                    }
                }
            }

            return new Vector2( xm * move.X, ym * move.Y );
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
