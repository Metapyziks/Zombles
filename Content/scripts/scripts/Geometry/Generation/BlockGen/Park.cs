using System;

using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.BlockGen
{
    public class Park : BlockGenerator
    {
        private StructureGen.Park _parkGen;

        public Park()
            : base(1.0 / 32.0, 8, 32)
        {
            _parkGen = new StructureGen.Park();
        }

        protected override void Generate(District district, TileBuilder[,] tiles,
             int borderLeft, int borderTop,
             int borderRight, int borderBottom, Random rand)
        {
            _parkGen.Fence = rand.NextDouble() < 0.75;

            _parkGen.Generate(district, tiles, borderLeft, borderTop,
                district.Width - borderLeft - borderRight,
                district.Height - borderTop - borderBottom, rand);
        }
    }
}
