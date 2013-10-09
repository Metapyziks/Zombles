using System;

using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.Blocks
{
    public class Warehouse : BlockGenerator
    {
        private Structures.Warehouse _buildingGen;

        public Warehouse()
            : base(1.0, 8, 12, 16, 24)
        {
            _buildingGen = new Structures.Warehouse();
        }

        protected override void Generate(District district, TileBuilder[,] tiles,
            int borderLeft, int borderTop,
            int borderRight, int borderBottom, Random rand)
        {
            _buildingGen.EntranceFaces =
                (borderLeft > 1 ? Face.West : Face.None) |
                (borderTop > 1 ? Face.North : Face.None) |
                (borderRight > 1 ? Face.East : Face.None) |
                (borderBottom > 1 ? Face.South : Face.None);

            _buildingGen.X = borderLeft;
            _buildingGen.Y = borderTop;
            _buildingGen.SizeX = district.Width - borderLeft - borderRight;
            _buildingGen.SizeY = district.Height - borderTop - borderBottom;

            _buildingGen.Generate(district, tiles, rand);
        }
    }
}
