using System;

using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.BlockGen
{
    public class Empty : BlockGenerator
    {
        public Empty()
            : base( 0.0, 0, 256 )
        {

        }

        protected override void Generate( TileBuilder[ , ] tiles, int width, int height, int borderLeft, int borderTop,
            int borderRight, int borderBottom, Random rand )
        {
            GenHelper.BuildFloor( tiles, borderLeft, borderTop,
                width - borderLeft - borderRight,
                height - borderTop - borderBottom,
                0, "floor_concrete_0" );
        }
    }
}
