using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public abstract class BuildingGenerator
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
        public static void BuildFloor( int x, int y, int width, int height, int level,
            String texture, TileBuilder[ , ] tiles )
        {
            BlockGenerator.BuildFloor( x, y, width, height, level, texture, tiles );
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
        public static void BuildFloor( int x, int y, int width, int height, int level,
            Func<int, int, String> textureFunc, TileBuilder[ , ] tiles )
        {
            BlockGenerator.BuildFloor( x, y, width, height, level, textureFunc, tiles );
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
        public static void BuildRoof( int x, int y, int width, int height, int level,
            String texture, TileBuilder[ , ] tiles )
        {
            BlockGenerator.BuildRoof( x, y, width, height, level, texture, tiles );
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
        public static void BuildRoof( int x, int y, int width, int height, int level,
            Func<int, int, String> textureFunc, TileBuilder[ , ] tiles )
        {
            BlockGenerator.BuildRoof( x, y, width, height, level, textureFunc, tiles );
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
        public static void BuildWall( int x, int y, Face face, int width, int height,
            String texture, TileBuilder[ , ] tiles )
        {
            BlockGenerator.BuildWall( x, y, face, width, height, texture, tiles );
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
        public static void BuildWall( int x, int y, Face face, int width, int height,
            String inTexture, String exTexture, TileBuilder[ , ] tiles )
        {
            BlockGenerator.BuildWall( x, y, face, width, height, inTexture, exTexture, tiles );
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
        public static void BuildWall( int x, int y, Face face, int width, int height,
            Func<int, int, bool, String> textureFunc, TileBuilder[ , ] tiles )
        {
            BlockGenerator.BuildWall( x, y, face, width, height, textureFunc, tiles );
        }

        public readonly int MinWidth;
        public readonly int MinHeight;

        protected BuildingGenerator( int minWidth, int minHeight )
        {
            MinWidth = minWidth;
            MinHeight = minHeight;
        }

        public void Generate( int x, int y, int width, int height, TileBuilder[ , ] tiles, int seed = 0 )
        {
            Random rand = ( seed == 0 ? new Random() : new Random( seed ) );
            Generate( x, y, width, height, tiles, rand );
        }

        public abstract void Generate( int x, int y, int width, int height, TileBuilder[ , ] tiles, Random rand );
    }
}
