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

        protected override void Generate( District district, TileBuilder[ , ] tiles,
            int borderLeft, int borderTop,
            int borderRight, int borderBottom, Random rand )
        {
            GenHelper.BuildFloor( tiles, borderLeft, borderTop,
                district.Width - borderLeft - borderRight,
                district.Height - borderTop - borderBottom,
                0, "floor/concrete/0" );
        }
    }
}
