using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public abstract class BuildingGenerator
    {
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

        protected void BuildFloor( int x, int y, int width, int height,
            Func<int, int, String> textureFunc, TileBuilder[ , ] tiles )
        {

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
        protected void BuildWall( int x, int y, Face face, int width, int height,
            Func<int,int,bool,String> textureFunc, TileBuilder[ , ] tiles )
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
    }
}
