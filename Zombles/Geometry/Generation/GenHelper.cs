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
        /// <param name="height">Depth of the floor</param>
        /// <param name="level">Height of the floor from the ground</param>
        /// <param name="texture">Tile texture name</param>
        /// <param name="tiles">Tile array to build the floor in</param>
        public static void BuildFloor( TileBuilder[ , ] tiles, int x, int y,
            int width, int height, int level, String texture )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int i = 0; i < width; ++i )
            {
                int tx = x + i;
                if ( tx >= 0 && tx < tw )
                {
                    for ( int j = 0; j < height; ++j )
                    {
                        int ty = y + j;

                        if ( ty >= 0 && ty < th )
                            tiles[ tx, ty ].SetFloor( level, texture );
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
        /// <param name="height">Depth of the floor</param>
        /// <param name="level">Height of the floor from the ground</param>
        /// <param name="textureFunc">Function deciding which texture to apply to a floor tile.
        /// Params are: int horzpos, int vertpos</param>
        /// <param name="tiles">Tile array to build the floor in</param>
        public static void BuildFloor( TileBuilder[ , ] tiles, int x, int y,
            int width, int height, int level, Func<int, int, String> textureFunc )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int i = 0; i < width; ++i )
            {
                int tx = x + i;
                if ( tx >= 0 && tx < tw )
                {
                    for ( int j = 0; j < height; ++j )
                    {
                        int ty = y + j;

                        if ( ty >= 0 && ty < th )
                            tiles[ tx, ty ].SetFloor( level, textureFunc( i, j ) );
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
        /// <param name="height">Depth of the roof</param>
        /// <param name="level">Height of the roof from the ground</param>
        /// <param name="texture">Tile texture name</param>
        /// <param name="tiles">Tile array to build the roof in</param>
        public static void BuildRoof( TileBuilder[ , ] tiles, int x, int y,
            int width, int height, int level, String texture )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int i = 0; i < width; ++i )
            {
                int tx = x + i;
                if ( tx >= 0 && tx < tw )
                {
                    for ( int j = 0; j < height; ++j )
                    {
                        int ty = y + j;

                        if ( ty >= 0 && ty < th )
                            tiles[ tx, ty ].SetRoof( level, texture );
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
        /// <param name="height">Depth of the roof</param>
        /// <param name="level">Height of the roof from the ground</param>
        /// <param name="textureFunc">Function deciding which texture to apply to a roof tile.
        /// Params are: int horzpos, int vertpos</param>
        /// <param name="tiles">Tile array to build the roof in</param>
        public static void BuildRoof( TileBuilder[ , ] tiles, int x, int y,
            int width, int height, int level, Func<int, int, String> textureFunc )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int i = 0; i < width; ++i )
            {
                int tx = x + i;
                if ( tx >= 0 && tx < tw )
                {
                    for ( int j = 0; j < height; ++j )
                    {
                        int ty = y + j;

                        if ( ty >= 0 && ty < th )
                            tiles[ tx, ty ].SetRoof( level, textureFunc( i, j ) );
                    }
                }
            }
        }

        /// <summary>
        /// Sets the wall indices of a row of tiles to build a wall
        /// </summary>
        /// <param name="x">Horizontal position of the wall</param>
        /// <param name="y">Vertical position of the wall</param>
        /// <param name="face">Direction the wall is facing</param>
        /// <param name="width">Width of the wall</param>
        /// <param name="height">Height of the wall</param>
        /// <param name="texture">Tile texture name</param>
        /// <param name="tiles">Tile array to build the wall in</param>
        public static void BuildWall( TileBuilder[ , ] tiles, int x, int y, Face face,
            int width, int height, String texture )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int j = 0; j < width; ++j )
            {
                int tx = x + ( face == Face.North || face == Face.South ? j : 0 );
                int ty = y + ( face == Face.East || face == Face.West ? j : 0 );

                if ( tx >= 0 && tx < tw && ty >= 0 && ty < th )
                    for ( int i = 0; i < height; ++i )
                        tiles[ tx, ty ].SetWall( face, i, texture );
            }
        }

        /// <summary>
        /// Sets the wall indices of a row of adjacent tiles to build a wall
        /// </summary>
        /// <param name="x">Horizontal position of the wall</param>
        /// <param name="y">Vertical position of the wall</param>
        /// <param name="face">Direction the wall is facing</param>
        /// <param name="width">Width of the wall</param>
        /// <param name="height">Height of the wall</param>
        /// <param name="inTexture">Interior tile texture name</param>
        /// <param name="exTexture">Exterior tile texture name</param>
        /// <param name="tiles">Tile array to build the wall in</param>
        public static void BuildWall( TileBuilder[ , ] tiles, int x, int y, Face face,
            int width, int height, String inTexture, String exTexture )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int j = 0; j < width; ++j )
            {
                int tx = x + ( face == Face.North || face == Face.South ? j : 0 );
                int ty = y + ( face == Face.East || face == Face.West ? j : 0 );

                if ( tx >= 0 && tx < tw && ty >= 0 && ty < th )
                    for ( int i = 0; i < height; ++i )
                        tiles[ tx, ty ].SetWall( face, i, inTexture );

                tx += face.GetNormalX();
                ty += face.GetNormalY();

                if ( tx >= 0 && tx < tw && ty >= 0 && ty < th )
                    for ( int i = 0; i < height; ++i )
                        tiles[ tx, ty ].SetWall( face.GetOpposite(), i, exTexture );
            }
        }

        /// <summary>
        /// Sets the wall indices of a row of adjacent tiles to build a wall
        /// </summary>
        /// <param name="x">Horizontal position of the wall</param>
        /// <param name="y">Vertical position of the wall</param>
        /// <param name="face">Direction the wall is facing</param>
        /// <param name="width">Width of the wall</param>
        /// <param name="height">Height of the wall</param>
        /// <param name="textureFunc">Function deciding which texture to apply to a wall tile.
        /// Params are: int horzpos, int level, bool isInterior</param>
        /// <param name="tiles">Tile array to build the wall in</param>
        public static void BuildWall( TileBuilder[ , ] tiles, int x, int y, Face face,
            int width, int height, Func<int, int, bool, String> textureFunc )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int j = 0; j < width; ++j )
            {
                int tx = x + ( face == Face.North || face == Face.South ? j : 0 );
                int ty = y + ( face == Face.East || face == Face.West ? j : 0 );

                if ( tx >= 0 && tx < tw && ty >= 0 && ty < th )
                    for ( int i = 0; i < height; ++i )
                        tiles[ tx, ty ].SetWall( face, i, textureFunc( j, i, true ) );

                tx += face.GetNormalX();
                ty += face.GetNormalY();

                if ( tx >= 0 && tx < tw && ty >= 0 && ty < th )
                    for ( int i = 0; i < height; ++i )
                        tiles[ tx, ty ].SetWall( face.GetOpposite(), i, textureFunc( j, i, false ) );
            }
        }
    }
}
