using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using ResourceLibrary;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.Structures
{
    public class Doorway : StructureGenerator
    {
        public Face Direction { get; set; }

        public int Width { get { return SizeX; } set { SizeX = value; } }
        public int Height { get { return SizeY; } set { SizeY = value; } }

        public ResourceLocator LeftBorderTile { get; set; }
        public ResourceLocator RightBorderTile { get; set; }
        public ResourceLocator BothBorderTile { get; set; }

        private List<Tuple<TileBuilder, Face, int>> _doorways;

        public Doorway()
        {
            _doorways = new List<Tuple<TileBuilder, Face, int>>();
        }

        private int GetConflict(TileBuilder[,] tiles, int x, int y)
        {
            var tile = _doorways.FirstOrDefault(d => d.Item2 == Direction && d.Item1 == tiles[x, y]);
            if (tile != null) {
                return Math.Min(tile.Item3, Height);
            } else {
                return 0;
            }
        }
        
        public override void Generate(District district, TileBuilder[,] tiles, Random rand)
        {
            var dirX = ((int) Direction & 0x5) == 0 ? 1 : 0;
            var dirY = 1 - dirX;

            var left = Direction == Face.North || Direction == Face.East ? LeftBorderTile : RightBorderTile;
            var right = Direction == Face.North || Direction == Face.East ? RightBorderTile : LeftBorderTile;

            int conflict = GetConflict(tiles, X - dirX * 2, Y - dirY * 2);
            GenHelper.BuildWall(tiles, X - dirX, Y - dirY, Direction, 1, conflict, BothBorderTile);
            GenHelper.BuildWall(tiles, X - dirX, Y - dirY, Direction, 1, conflict, Height - conflict, left);

            GenHelper.BuildWall(tiles, X, Y, Direction, Width, Height, ResourceLocator.None);

            conflict = GetConflict(tiles, X + dirX * (Width + 1), Y - dirY * (Width + 1));
            GenHelper.BuildWall(tiles, X + dirX * Width, Y + dirY * Width, Direction, 1, conflict, BothBorderTile);
            GenHelper.BuildWall(tiles, X + dirX * Width, Y + dirY * Width, Direction, 1, conflict, Height - conflict, right);
        }

        public void GenerateOpposite(District district, TileBuilder[,] tiles, Random rand)
        {
            Direction = Direction.GetOpposite();
            X -= Direction.GetNormalX();
            Y -= Direction.GetNormalY();

            Generate(district, tiles, rand);

            Direction = Direction.GetOpposite();
            X -= Direction.GetNormalX();
            Y -= Direction.GetNormalY();
        }
    }
}
