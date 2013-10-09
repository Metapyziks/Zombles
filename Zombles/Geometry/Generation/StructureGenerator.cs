using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public abstract class StructureGenerator
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int SizeX { get; set; }
        public int SizeY { get; set; }

        public abstract void Generate(District district, TileBuilder[,] tiles, Random rand);
    }
}
