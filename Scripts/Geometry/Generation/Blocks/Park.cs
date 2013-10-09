using System;

using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.Blocks
{
    public class Park : BlockGenerator
    {
        private Structures.Park _parkGen;

        public Park()
            : base(1.0 / 32.0, 8, 32)
        {
            _parkGen = new Structures.Park();
        }

        protected override void Generate(District district, TileBuilder[,] tiles,
            int borderLeft, int borderTop,
            int borderRight, int borderBottom, Random rand)
        {
            _parkGen.Fence = rand.NextDouble() < 0.75;
            _parkGen.X = borderLeft;
            _parkGen.Y = borderTop;
            _parkGen.SizeX = district.Width - borderLeft - borderRight;
            _parkGen.SizeY = district.Height - borderTop - borderBottom;

            _parkGen.Generate(district, tiles, rand);
        }
    }
}
