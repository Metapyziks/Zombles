using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.Rooms
{
    public class Warehouse : RoomGenerator
    {
        public override void Generate(District district, TileBuilder[,] tiles, Random rand)
        {
            if (rand.NextDouble() < 0.5) {
                GenHelper.BuildFloor(tiles, X, Y, SizeX, SizeY, 0,
                    (horz, vert) => rand.NextTexture("floor/planks", 4));
            }

            new Structures.CratePile {
                X = X + 1, Y = Y + 1, SizeX = SizeX - 2, SizeY = SizeY - 2,
                CrateFrequency = rand.NextDouble() * 0.5
            }.Generate(district, tiles, rand);
        }
    }
}
