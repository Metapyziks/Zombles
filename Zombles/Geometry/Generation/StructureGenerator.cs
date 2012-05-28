using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public abstract class StructureGenerator
    {
        public void Generate( District district, TileBuilder[ , ] tiles, int x, int y, int width, int height, int seed = 0 )
        {
            Random rand = ( seed == 0 ? new Random() : new Random( seed ) );
            Generate( district, tiles, x, y, width, height, rand );
        }

        public abstract void Generate( District district, TileBuilder[ , ] tiles, int x, int y, int width, int height, Random rand );
    }
}
