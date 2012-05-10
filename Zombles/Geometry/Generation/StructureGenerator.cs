using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public abstract class StructureGenerator
    {
        public readonly int MinWidth;
        public readonly int MinHeight;

        protected StructureGenerator( int minWidth, int minHeight )
        {
            MinWidth = minWidth;
            MinHeight = minHeight;
        }

        public void Generate( TileBuilder[ , ] tiles, int x, int y, int width, int height, int seed = 0 )
        {
            Random rand = ( seed == 0 ? new Random() : new Random( seed ) );
            Generate( tiles, x, y, width, height, rand );
        }

        public abstract void Generate( TileBuilder[ , ] tiles, int x, int y, int width, int height, Random rand );
    }
}
