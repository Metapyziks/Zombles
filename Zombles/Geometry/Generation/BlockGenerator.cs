using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Zombles.Geometry.Generation
{
    public abstract class BlockGenerator
    {
        private static BlockGenerator[] stGenerators;

        public static BlockGenerator[] GetAll()
        {
            if ( stGenerators == null )
                FindGenerators();

            return stGenerators;
        }
        
        public static bool WillAnyFit( int width, int height, bool acceptLarger = false )
        {
            if ( stGenerators == null )
                FindGenerators();

            foreach ( BlockGenerator gen in stGenerators )
                if ( gen.Frequency > 0.0 && gen.WillFit( width, height, acceptLarger ) )
                    return true;

            return false;
        }

        public static BlockGenerator GetRandom( int width, int height, Random rand )
        {
            if ( stGenerators == null )
                FindGenerators();

            BlockGenerator cur = null;
            double freq = double.MaxValue;
            foreach ( BlockGenerator gen in stGenerators )
            {
                if ( gen.WillFit( width, height ) )
                {
                    double val = gen.Frequency;
                    if ( val <= 0.0 )
                        val = double.MaxValue / 2.0;
                    else
                        val = rand.NextDouble() / val;

                    if ( val < freq )
                    {
                        cur = gen;
                        freq = val;
                    }
                }
            }

            return cur;
        }

        public static BlockGenerator[] GetAllRandom( int width, int height, Random rand )
        {
            if ( stGenerators == null )
                FindGenerators();

            List<BlockGenerator> gens = new List<BlockGenerator>();
            foreach ( BlockGenerator gen in stGenerators )
                if ( gen.WillFit( width, height ) )
                    gens.Add( gen );

            return gens.OrderBy( x => x.Frequency <= 0.0
                ? double.MaxValue : rand.NextDouble() / x.Frequency ).ToArray();
        }

        public static void FindGenerators()
        {
            Type[] types = Scripts.GetTypes( typeof( BlockGenerator ) );
            List<BlockGenerator> valid = new List<BlockGenerator>();

            for ( int i = 0; i < types.Length; ++i )
            {
                ConstructorInfo cns = types[ i ].GetConstructor( new Type[ 0 ] );
                if( cns != null )
                    valid.Add( cns.Invoke( new object[ 0 ] ) as BlockGenerator );
            }

            stGenerators = valid.ToArray();
        }

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
        public static void BuildFloor( int x, int y, int width, int height, int level,
            Func<int, int, String> textureFunc, TileBuilder[ , ] tiles )
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

                        if( ty >= 0 && ty < th )
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
        public static void BuildRoof( int x, int y, int width, int height, int level,
            String texture, TileBuilder[ , ] tiles )
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
        public static void BuildRoof( int x, int y, int width, int height, int level,
            Func<int, int, String> textureFunc, TileBuilder[ , ] tiles )
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
        public static void BuildWall( int x, int y, Face face, int width, int height,
            String texture, TileBuilder[ , ] tiles )
        {
            int tw = tiles.GetLength( 0 );
            int th = tiles.GetLength( 1 );

            for ( int j = 0; j < width; ++j )
            {
                int tx = x + ( face == Face.North || face == Face.South ? j : 0 );
                int ty = y + ( face == Face.East || face == Face.West ? j : 0 );

                if( tx >= 0 && tx < tw && ty >= 0 && ty < th )
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
        public static void BuildWall( int x, int y, Face face, int width, int height,
            Func<int, int, bool, String> textureFunc, TileBuilder[ , ] tiles )
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

        public readonly double Frequency;

        public readonly int MinShortSide;
        public readonly int MinLongSide;
        public readonly int MaxShortSide;
        public readonly int MaxLongSide;

        protected BlockGenerator( double frequency, int minSide, int maxSide )
        {
            Frequency = frequency;

            MinShortSide = minSide;
            MinLongSide = minSide;
            MaxShortSide = maxSide;
            MaxLongSide = maxSide;
        }

        protected BlockGenerator( double frequency, int minShortSide, int minLongSide, int maxShortSide, int maxLongSide )
        {
            Frequency = frequency;

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

            int innerLeft = borderLeft;
            int innerTop = borderTop;
            int innerRight = width - borderRight;
            int innerBottom = height - borderBottom;
            int innerWidth = width - borderLeft - borderRight;
            int innerHeight = height - borderTop - borderBottom;

            Func<int, int, String> roadFunc = delegate( int tx, int ty )
            {
                return rand.NextTexture( "floor_road_", 0, 4 );
            };

            Func<int, int, String> innerFunc = delegate( int tx, int ty )
            {
                return "floor_concrete_0";
            };

            BuildFloor( 0, 0, width, height, 0, roadFunc, tiles );
            BuildFloor( innerLeft - 1, innerTop - 1, innerWidth + 2, innerHeight + 2, 0, innerFunc, tiles );

            int texOffset = 0; bool horz = false;
            Func<int, int, String> pavementFunc = delegate( int tx, int ty )
            {
                if ( ( horz && tx % 8 == 4 ) || ( !horz && ty % 8 == 4 ) )
                    return "floor_pavement_" + ( texOffset + 2 ).ToString( "X" ).ToLower();

                return rand.NextTexture( "floor_pavement_", texOffset, texOffset + 2 );
            };

            if ( borderBottom > 1 )
            {
                texOffset = 0; horz = true;
                BuildFloor( innerLeft - 1, innerBottom, innerWidth + 2, 1, 0, pavementFunc, tiles );
            }
            if ( borderRight > 1 )
            {
                texOffset = 4; horz = false;
                BuildFloor( innerRight, innerTop - 1, 1, innerHeight + 2, 0, pavementFunc, tiles );
            }
            if ( borderTop > 1 )
            {
                texOffset = 8; horz = true;
                BuildFloor( innerLeft - 1, innerTop - 1, innerWidth + 2, 1, 0, pavementFunc, tiles );
            }
            if ( borderLeft > 1 )
            {
                texOffset = 12; horz = false;
                BuildFloor( innerLeft - 1, innerTop - 1, 1, innerHeight + 2, 0, pavementFunc, tiles );
            }

            if( borderBottom > 1 && borderRight > 1 )
                BuildFloor( innerRight, innerBottom, 1, 1, 0, "floor_pavement_3", tiles );
            if ( borderTop > 1 && borderRight > 1 )
                BuildFloor( innerRight, innerTop - 1, 1, 1, 0, "floor_pavement_7", tiles );
            if ( borderTop > 1 && borderLeft > 1 )
                BuildFloor( innerLeft - 1, innerTop - 1, 1, 1, 0, "floor_pavement_b", tiles );
            if ( borderBottom > 1 && borderLeft > 1 )
                BuildFloor( innerLeft - 1, innerBottom, 1, 1, 0, "floor_pavement_f", tiles );

            Generate( width, height, borderLeft, borderTop, borderRight, borderBottom, tiles, rand );

            Block block = new Block( x, y, width, height );
            block.BuildTiles( tiles );
            return block;
        }

        protected abstract void Generate( int width, int height, int borderLeft, int borderTop,
            int borderRight, int borderBottom, TileBuilder[ , ] tiles, Random rand );
    }
}
