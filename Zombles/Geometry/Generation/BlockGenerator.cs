using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public abstract class BlockGenerator
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
            for ( int i = 0; i < width; ++i ) for ( int j = 0; j < height; ++j )
                {
                    int tx = x + i;
                    int ty = y + j;
                    tiles[ tx, ty ].SetFloor( level, texture );
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
        public static void BuildFloor( int x, int y, int width, int height, int level,
            Func<int, int, String> textureFunc, TileBuilder[ , ] tiles )
        {
            for ( int i = 0; i < width; ++i ) for ( int j = 0; j < height; ++j )
                {
                    int tx = x + i;
                    int ty = y + j;
                    tiles[ tx, ty ].SetFloor( level, textureFunc( i, j ) );
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
        public static void BuildRoof( int x, int y, int width, int height, int level,
            String texture, TileBuilder[ , ] tiles )
        {
            for ( int i = 0; i < width; ++i ) for ( int j = 0; j < height; ++j )
                {
                    int tx = x + i;
                    int ty = y + j;
                    tiles[ tx, ty ].SetRoof( level, texture );
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
        public static void BuildRoof( int x, int y, int width, int height, int level,
            Func<int, int, String> textureFunc, TileBuilder[ , ] tiles )
        {
            for ( int i = 0; i < width; ++i ) for ( int j = 0; j < height; ++j )
                {
                    int tx = x + i;
                    int ty = y + j;
                    tiles[ tx, ty ].SetRoof( level, textureFunc( i, j ) );
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
        public static void BuildWall( int x, int y, Face face, int width, int height,
            String texture, TileBuilder[ , ] tiles )
        {
            for ( int j = 0; j < width; ++j )
            {
                int tx = x + ( face == Face.North || face == Face.South ? j : 0 );
                int ty = y + ( face == Face.East || face == Face.West ? j : 0 );
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
        public static void BuildWall( int x, int y, Face face, int width, int height,
            String inTexture, String exTexture, TileBuilder[ , ] tiles )
        {
            for ( int j = 0; j < width; ++j )
            {
                int tx = x + ( face == Face.North || face == Face.South ? j : 0 );
                int ty = y + ( face == Face.East || face == Face.West ? j : 0 );
                for ( int i = 0; i < height; ++i )
                {
                    tiles[ tx, ty ].SetWall( face, i, inTexture );
                    tiles[ tx + face.GetNormalX(), ty + face.GetNormalY() ]
                        .SetWall( face.GetOpposite(), i, exTexture );
                }
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
        public static void BuildWall( int x, int y, Face face, int width, int height,
            Func<int, int, bool, String> textureFunc, TileBuilder[ , ] tiles )
        {
            for ( int j = 0; j < width; ++j )
            {
                int tx = x + ( face == Face.North || face == Face.South ? j : 0 );
                int ty = y + ( face == Face.East || face == Face.West ? j : 0 );
                for ( int i = 0; i < height; ++i )
                {
                    tiles[ tx, ty ].SetWall( face, i, textureFunc( j, i, true ) );
                    tiles[ tx + face.GetNormalX(), ty + face.GetNormalY() ]
                        .SetWall( face.GetOpposite(), i, textureFunc( j, i, false ) );
                }
            }
        }

        public readonly int MinShortSide;
        public readonly int MinLongSide;
        public readonly int MaxShortSide;
        public readonly int MaxLongSide;

        protected BlockGenerator( int minSide, int maxSide )
        {
            MinShortSide = minSide;
            MinLongSide = minSide;
            MaxShortSide = maxSide;
            MaxLongSide = maxSide;
        }

        protected BlockGenerator( int minShortSide, int minLongSide, int maxShortSide, int maxLongSide )
        {
            MinShortSide = minShortSide;
            MinLongSide = minLongSide;
            MaxShortSide = maxShortSide;
            MaxLongSide = maxLongSide;
        }

        public bool WillFit( int width, int height, bool acceptLarger = false )
        {
            int sh = Math.Min( width, height );
            int ln = Math.Max( width, height );

            return sh >= MinShortSide && ln >= MinLongSide &&
                ( acceptLarger || ( ln <= MaxLongSide && sh <= MaxShortSide ) );
        }

        public Block Generate( int x, int y, int width, int height, int borderLeft, int borderTop,
            int borderRight, int borderBottom, int seed = 0 )
        {
            Random rand = ( seed == 0 ? new Random() : new Random( seed ) );
            return Generate( x, y, width, height, borderLeft, borderTop, borderRight, borderBottom, rand );
        }

        public Block Generate( int x, int y, int width, int height, int borderLeft, int borderTop,
            int borderRight, int borderBottom, Random rand )
        {
            TileBuilder[,] tiles = new TileBuilder[ width, height ];

            for ( int tx = 0; tx < width; ++tx ) for ( int ty = 0; ty < height; ++ty )
                tiles[ tx, ty ] = new TileBuilder();

            Func<int, int, String> roadFunc = delegate( int tx, int ty )
            {
                return rand.NextTexture( "floor_road_", 0, 4 );
            };

            int innerLeft = borderLeft ;
            int innerTop = borderTop;
            int innerRight = width - borderRight;
            int innerBottom = height - borderBottom;
            int innerWidth = width - borderLeft - borderRight;
            int innerHeight = height - borderTop - borderBottom;

            BuildFloor( 0, 0, borderLeft, height, 0, roadFunc, tiles );
            BuildFloor( borderLeft, 0, innerWidth, borderTop, 0, roadFunc, tiles );
            BuildFloor( innerRight, 0, borderRight, height, 0, roadFunc, tiles );
            BuildFloor( borderLeft, innerBottom, innerWidth, borderBottom, 0, roadFunc, tiles );

            int texOffset = 8; bool horz = true;
            Func<int, int, String> pavementFunc = delegate( int tx, int ty )
            {
                if ( ( horz && tx % 8 == 4 ) || ( !horz && ty % 8 == 4 ) )
                    return "floor_pavement_" + ( texOffset + 2 ).ToString( "X" ).ToLower();

                return rand.NextTexture( "floor_pavement_", texOffset, texOffset + 2 );
            };

            BuildFloor( innerLeft - 1, innerTop - 1, 1, 1, 0, "floor_pavement_b", tiles );
            BuildFloor( innerLeft, innerTop - 1, innerWidth, 1, 0, pavementFunc, tiles );
            texOffset = 4; horz = false;
            BuildFloor( innerRight, innerTop - 1, 1, 1, 0, "floor_pavement_7", tiles );
            BuildFloor( innerRight, innerTop, 1, innerHeight, 0, pavementFunc, tiles );
            texOffset = 0; horz = true;
            BuildFloor( innerRight, innerBottom, 1, 1, 0, "floor_pavement_3", tiles );
            BuildFloor( innerLeft, innerBottom, innerWidth, 1, 0, pavementFunc, tiles );
            texOffset = 12; horz = true;
            BuildFloor( innerLeft - 1, innerBottom, 1, 1, 0, "floor_pavement_f", tiles );
            BuildFloor( innerLeft - 1, innerTop, 1, innerHeight, 0, pavementFunc, tiles );

            Generate( width, height, borderLeft, borderTop, borderRight, borderBottom, tiles, rand );

            Block block = new Block( x, y, width, height );
            block.BuildTiles( tiles );
            return block;
        }

        protected abstract void Generate( int width, int height, int borderLeft, int borderTop,
            int borderRight, int borderBottom, TileBuilder[ , ] tiles, Random rand );
    }
}
