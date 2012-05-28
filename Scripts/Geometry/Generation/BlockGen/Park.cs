using System;

using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.BlockGen
{
    public class Park : BlockGenerator
    {
        private StructureGen.Park myParkGen;

        public Park()
            : base( 1.0 / 32.0, 8, 32 )
        {
            myParkGen = new StructureGen.Park();
        }

        protected override void Generate( District district, TileBuilder[ , ] tiles,
             int borderLeft, int borderTop,
             int borderRight, int borderBottom, Random rand )
        {
            myParkGen.Fence = rand.NextDouble() < 0.75;

            myParkGen.Generate( district, tiles, borderLeft, borderTop,
                district.Width - borderLeft - borderRight,
                district.Height - borderTop - borderBottom, rand );
        }
    }
}
