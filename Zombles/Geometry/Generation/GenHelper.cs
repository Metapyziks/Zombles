using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public static class GenHelper
    {
        /// <summary>
        /// Sets the floor index of a rectangle of tiles to build a floor
        /// </summary>
        /// <param name="x">Horizontal start position</param>
        /// <param name="y">Vertical start position</param>
        /// <param name="width">Width of the floor</param>
        /// <param name="depth">Depth of the floor</param>
        /// <param name="height">Height of the floor from the ground</param>
        /// <param name="texture">Tile texture name</param>
        /// <param name="tiles">Tile array to build the floor in</param>
        public static void BuildFloor( TileBuilder[ , ] tiles, int x, int y,
            int width, int depth, int height, String texture )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int i = 0; i < width; ++i )
            {
                int tx = x + i;
                if ( tx >= 0 && tx < tw )
                {
                    for ( int j = 0; j < depth; ++j )
                    {
                        int ty = y + j;

                        if ( ty >= 0 && ty < th )
                            tiles[ tx, ty ].SetFloor( height, texture );
                    }
                }
            }
        }

        /// <summary>
        /// Sets the floor index of a rectangle of tiles to build a floor
        /// </summary>
        /// <param name="x">Horizontal start position</param>
        /// <param name="y">Vertical start position</param>
        /// <param name="width">Width of the floor</param>
        /// <param name="depth">Depth of the floor</param>
        /// <param name="height">Height of the floor from the ground</param>
        /// <param name="textureFunc">Function deciding which texture to apply to a floor tile.
        /// Params are: int horzpos, int vertpos</param>
        /// <param name="tiles">Tile array to build the floor in</param>
        public static void BuildFloor( TileBuilder[ , ] tiles, int x, int y,
            int width, int depth, int height, Func<int, int, String> textureFunc )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int i = 0; i < width; ++i )
            {
                int tx = x + i;
                if ( tx >= 0 && tx < tw )
                {
                    for ( int j = 0; j < depth; ++j )
                    {
                        int ty = y + j;

                        if ( ty >= 0 && ty < th )
                            tiles[ tx, ty ].SetFloor( height, textureFunc( i, j ) );
                    }
                }
            }
        }

        /// <summary>
        /// Sets the roof index of a rectangle of tiles to build a roof
        /// </summary>
        /// <param name="x">Horizontal start position</param>
        /// <param name="y">Vertical start position</param>
        /// <param name="width">Width of the roof</param>
        /// <param name="depth">Depth of the roof</param>
        /// <param name="height">Height of the roof from the ground</param>
        /// <param name="texture">Tile texture name</param>
        /// <param name="slant">Direction of slant</param>
        /// <param name="tiles">Tile array to build the roof in</param>
        public static void BuildRoof( TileBuilder[ , ] tiles, int x, int y,
            int width, int depth, int height, String texture, Face slant = Face.None )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int i = 0; i < width; ++i )
            {
                int tx = x + i;
                if ( tx >= 0 && tx < tw )
                {
                    for ( int j = 0; j < depth; ++j )
                    {
                        int ty = y + j;

                        if ( ty >= 0 && ty < th )
                            tiles[ tx, ty ].SetRoof( height, slant, texture );
                    }
                }
            }
        }

        /// <summary>
        /// Sets the roof index of a rectangle of tiles to build a roof
        /// </summary>
        /// <param name="x">Horizontal start position</param>
        /// <param name="y">Vertical start position</param>
        /// <param name="width">Width of the roof</param>
        /// <param name="depth">Depth of the roof</param>
        /// <param name="height">Height of the roof from the ground</param>
        /// <param name="textureFunc">Function deciding which texture to apply to a roof tile.
        /// Params are: int horzpos, int vertpos</param>
        /// <param name="slant">Direction of slant</param>
        /// <param name="tiles">Tile array to build the roof in</param>
        public static void BuildRoof( TileBuilder[ , ] tiles, int x, int y,
            int width, int depth, int height, Func<int, int, String> textureFunc, Face slant = Face.None )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int i = 0; i < width; ++i )
            {
                int tx = x + i;
                if ( tx >= 0 && tx < tw )
                {
                    for ( int j = 0; j < depth; ++j )
                    {
                        int ty = y + j;

                        if ( ty >= 0 && ty < th )
                            tiles[ tx, ty ].SetRoof( height, slant, textureFunc( i, j ) );
                    }
                }
            }
        }

        /// <summary>
        /// Sets the wall indices of a row of tiles to build a wall
        /// </summary>
        /// <param name="x">Horizontal position of the wall</param>
        /// <param name="y">Vertical position of the wall</param>
        /// <param name="face">Direction the exterior of the wall is facing</param>
        /// <param name="width">Width of the wall</param>
        /// <param name="height">Height of the wall</param>
        /// <param name="texture">Tile texture name</param>
        /// <param name="tiles">Tile array to build the wall in</param>
        public static void BuildWall( TileBuilder[ , ] tiles, int x, int y, Face face,
            int width, int height, String texture )
        {
            BuildWall( tiles, x, y, face, width, 0, height, texture );
        }

        /// <summary>
        /// Sets the wall indices of a row of tiles to build a wall
        /// </summary>
        /// <param name="x">Horizontal position of the wall</param>
        /// <param name="y">Vertical position of the wall</param>
        /// <param name="face">Direction the exterior of the wall is facing</param>
        /// <param name="width">Width of the wall</param>
        /// <param name="offset">Distance from the ground</param>
        /// <param name="height">Height of the wall</param>
        /// <param name="texture">Tile texture name</param>
        /// <param name="tiles">Tile array to build the wall in</param>
        public static void BuildWall( TileBuilder[ , ] tiles, int x, int y, Face face,
            int width, int offset, int height, String texture )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int j = 0; j < width; ++j )
            {
                int tx = x + ( face == Face.North || face == Face.South ? j : 0 );
                int ty = y + ( face == Face.East || face == Face.West ? j : 0 );

                if ( tx >= 0 && tx < tw && ty >= 0 && ty < th )
                    for ( int i = 0; i < height; ++i )
                        tiles[ tx, ty ].SetWall( face, i + offset, texture );
            }
        }

        /// <summary>
        /// Sets the wall indices of a row of adjacent tiles to build a wall
        /// </summary>
        /// <param name="x">Horizontal position of the wall</param>
        /// <param name="y">Vertical position of the wall</param>
        /// <param name="face">Direction the exterior of the wall is facing</param>
        /// <param name="width">Width of the wall</param>
        /// <param name="height">Height of the wall</param>
        /// <param name="inTexture">Interior tile texture name</param>
        /// <param name="exTexture">Exterior tile texture name</param>
        /// <param name="tiles">Tile array to build the wall in</param>
        public static void BuildWall( TileBuilder[ , ] tiles, int x, int y, Face face,
            int width, int height, String inTexture, String exTexture )
        {
            BuildWall( tiles, x, y, face, width, 0, height, inTexture, exTexture );
        }

        /// <summary>
        /// Sets the wall indices of a row of adjacent tiles to build a wall
        /// </summary>
        /// <param name="x">Horizontal position of the wall</param>
        /// <param name="y">Vertical position of the wall</param>
        /// <param name="face">Direction the exterior of the wall is facing</param>
        /// <param name="width">Width of the wall</param>
        /// <param name="offset">Distance from the ground</param>
        /// <param name="height">Height of the wall</param>
        /// <param name="inTexture">Interior tile texture name</param>
        /// <param name="exTexture">Exterior tile texture name</param>
        /// <param name="tiles">Tile array to build the wall in</param>
        public static void BuildWall( TileBuilder[ , ] tiles, int x, int y, Face face,
            int width, int offset, int height, String inTexture, String exTexture )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int j = 0; j < width; ++j )
            {
                int tx = x + ( face == Face.North || face == Face.South ? j : 0 );
                int ty = y + ( face == Face.East || face == Face.West ? j : 0 );

                if ( tx >= 0 && tx < tw && ty >= 0 && ty < th )
                    for ( int i = 0; i < height; ++i )
                        tiles[ tx, ty ].SetWall( face, i + offset, inTexture );

                tx += face.GetNormalX();
                ty += face.GetNormalY();

                if ( tx >= 0 && tx < tw && ty >= 0 && ty < th )
                    for ( int i = 0; i < height; ++i )
                        tiles[ tx, ty ].SetWall( face.GetOpposite(), i + offset, exTexture );
            }
        }

        /// <summary>
        /// Sets the wall indices of a row of adjacent tiles to build a wall
        /// </summary>
        /// <param name="x">Horizontal position of the wall</param>
        /// <param name="y">Vertical position of the wall</param>
        /// <param name="face">Direction the exterior of the wall is facing</param>
        /// <param name="width">Width of the wall</param>
        /// <param name="height">Height of the wall</param>
        /// <param name="textureFunc">Function deciding which texture to apply to a wall tile.
        /// Params are: int horzpos, int level, Face face, bool isInterior</param>
        /// <param name="tiles">Tile array to build the wall in</param>
        public static void BuildWall( TileBuilder[ , ] tiles, int x, int y, Face face,
            int width, int height, Func<int, int, Face, bool, String> textureFunc )
        {
            BuildWall( tiles, x, y, face, width, 0, height, textureFunc );
        }

        /// <summary>
        /// Sets the wall indices of a row of adjacent tiles to build a wall
        /// </summary>
        /// <param name="x">Horizontal position of the wall</param>
        /// <param name="y">Vertical position of the wall</param>
        /// <param name="face">Direction the exterior of the wall is facing</param>
        /// <param name="width">Width of the wall</param>
        /// <param name="offset">Distance from the ground</param>
        /// <param name="height">Height of the wall</param>
        /// <param name="textureFunc">Function deciding which texture to apply to a wall tile.
        /// Params are: int horzpos, int level, Face face, bool isInterior</param>
        /// <param name="tiles">Tile array to build the wall in</param>
        public static void BuildWall( TileBuilder[ , ] tiles, int x, int y, Face face,
            int width, int offset, int height, Func<int, int, Face, bool, String> textureFunc )
        {
            Face opp = face.GetOpposite();

            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int j = 0; j < width; ++j )
            {
                int tx = x + ( face == Face.North || face == Face.South ? j : 0 );
                int ty = y + ( face == Face.East || face == Face.West ? j : 0 );

                if ( tx >= 0 && tx < tw && ty >= 0 && ty < th )
                {
                    for ( int i = 0; i < height; ++i )
                    {
                        String tex = textureFunc( j, i + offset, face, true );
                        if( tex != null )
                            tiles[ tx, ty ].SetWall( face, i + offset, tex );
                    }
                }

                tx += face.GetNormalX();
                ty += face.GetNormalY();

                if ( tx >= 0 && tx < tw && ty >= 0 && ty < th )
                {
                    for ( int i = 0; i < height; ++i )
                    {
                        String tex = textureFunc( j, i + offset, face, false );
                        if ( tex != null )
                            tiles[ tx, ty ].SetWall( opp, i + offset, tex );
                    }
                }
            }
        }

        /// <summary>
        /// Builds a solid (non hollow) box
        /// </summary>
        /// <param name="x">Horizontal position of the box</param>
        /// <param name="y">Vertical position of the box</param>
        /// <param name="width">Width of the box</param>
        /// <param name="depth">Depth of the box</param>
        /// <param name="height">Height of the box</param>
        /// <param name="wallTexture">Top tile texture for the box</param>
        /// <param name="topTexture">Wall tile texture for the box</param>
        /// <param name="tiles">Tile array to build the box in</param>
        public static void BuildSolid( TileBuilder[ , ] tiles, int x, int y, int width, int depth,
            int height, String wallTexture, String topTexture )
        {
            BuildFloor( tiles, x, y, width, depth, height, topTexture );
            BuildWall( tiles, x - 1, y, Face.East, depth, height, wallTexture );
            BuildWall( tiles, x, y - 1, Face.South, width, height, wallTexture );
            BuildWall( tiles, x + width, y, Face.West, depth, height, wallTexture );
            BuildWall( tiles, x, y + depth, Face.North, width, height, wallTexture );
        }
    }
}
