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

        public BuildingGenerator()
            : this( 1, 1 )
        {

        }

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
