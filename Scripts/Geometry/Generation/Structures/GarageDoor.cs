using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResourceLibrary;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.Structures
{
    public class GarageDoor : Doorway
    {
        public ResourceLocator DoorTile { get; set; }
        public int DoorPosition { get; set; }

        public override void Generate(District district, TileBuilder[,] tiles, Random rand)
        {
            base.Generate(district, tiles, rand);

            GenHelper.BuildWall(tiles, X, Y, Direction, Width, DoorPosition, Height - DoorPosition, DoorTile);
        }
    }
}
