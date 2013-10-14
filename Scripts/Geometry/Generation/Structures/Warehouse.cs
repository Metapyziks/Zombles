using System;

using OpenTK;

using Zombles;
using Zombles.Geometry;
using Zombles.Geometry.Generation;
using Zombles.Entities;
using ResourceLibrary;

namespace Zombles.Scripts.Geometry.Generation.Structures
{
    public class Warehouse : StructureGenerator
    {
        private CratePile _crateGen;

        public Face EntranceFaces { get; set; }

        public Warehouse(Face entranceFaces = Face.All)
        {
            _crateGen = new CratePile();
            _crateGen.MaxHeight = 2;

            EntranceFaces = entranceFaces;
        }

        public override void Generate(District district, TileBuilder[,] tiles, Random rand)
        {
            if (EntranceFaces != Face.None) {
                int rheight = rand.Next(3) + 4;

                ResourceLocator wallGroup = "wall/brick" + rand.Next(2);

                GenHelper.BuildRoof(tiles, X, Y, SizeX, SizeY, rheight, "floor/roof/0");

                Func<int,int,Face,bool,ResourceLocator> texFunc = (horzpos, level, face, isInterior) => {
                    if (level < rheight)
                        return rand.NextTexture(wallGroup, 4);

                    return wallGroup["7"];
                };
                
                GenHelper.BuildWall(tiles, X, Y, Face.North, SizeX, rheight + 1, texFunc);
                GenHelper.BuildWall(tiles, X, Y, Face.West, SizeY, rheight + 1, texFunc);
                GenHelper.BuildWall(tiles, X, Y + SizeY - 1, Face.South, SizeX, rheight + 1, texFunc);
                GenHelper.BuildWall(tiles, X + SizeX - 1, Y, Face.East, SizeY, rheight + 1, texFunc);

                var entrFace = rand.NextFace(EntranceFaces);
                var doorway = new GarageDoor {
                    Direction = entrFace,
                    LeftBorderTile = wallGroup["8"],
                    RightBorderTile = wallGroup["9"],
                    BothBorderTile = wallGroup["a"],
                    Width = rand.Next(2, 4),
                    Height = 3,
                    DoorTile = "wall/garage/0",
                    DoorPosition = 2
                };

                switch (entrFace) {
                    case Face.North:
                        doorway.X = X + 1;
                        doorway.Y = Y;
                        break;
                    case Face.South:
                        doorway.X = X + 1;
                        doorway.Y = Y + SizeY - 1;
                        break;
                    case Face.East:
                        doorway.X = X + SizeX - 1;
                        doorway.Y = Y + 1;
                        break;
                    case Face.West:
                        doorway.X = X;
                        doorway.Y = Y + 1;
                        break;
                }

                doorway.Generate(district, tiles, rand);
                doorway.GenerateOpposite(district, tiles, rand);

                new Rooms.Warehouse {
                    X = X, Y = Y, SizeX = SizeX, SizeY = SizeY, Height = rheight
                }.Generate(district, tiles, rand);
            }
        }
    }
}
