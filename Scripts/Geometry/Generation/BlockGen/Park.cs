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

        protected override void Generate( TileBuilder[ , ] tiles, int width, int height,
            int borderLeft, int borderTop,
            int borderRight, int borderBottom, Random rand )
        {
            myParkGen.Fence = rand.NextDouble() < 0.75;

            myParkGen.Generate( tiles, borderLeft, borderTop,
                width - borderLeft - borderRight,
                height - borderTop - borderBottom, rand );
        }
    }
}
