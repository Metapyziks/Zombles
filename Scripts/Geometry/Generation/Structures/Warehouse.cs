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

                if (rand.NextDouble() < 0.5) {
                    GenHelper.BuildFloor(tiles, X, Y, SizeX, SizeY, 0,
                        (horz, vert) => rand.NextTexture("floor/planks", 4));
                }

                new CratePile {
                    X = X + 1, Y = Y + 1, SizeX = SizeX - 2, SizeY = SizeY - 2,
                    CrateFrequency = rand.NextDouble() * 0.5
                }.Generate(district, tiles, rand);

                GenHelper.BuildWall(tiles, X, Y, Face.North, SizeX, rheight + 1, texFunc);
                GenHelper.BuildWall(tiles, X, Y, Face.West, SizeY, rheight + 1, texFunc);
                GenHelper.BuildWall(tiles, X, Y + SizeY - 1, Face.South, SizeX, rheight + 1, texFunc);
                GenHelper.BuildWall(tiles, X + SizeX - 1, Y, Face.East, SizeY, rheight + 1, texFunc);

                Face entrance = rand.NextFace(EntranceFaces);
                int entranceSize = rand.Next(2, 4);
                bool open = false;
                if (((int) entrance & 0x5) != 0) {
                    int entranceCount = Math.Min((SizeY - 1) / (entranceSize + 1), 3);
                    int entranceOffset = rand.Next(1, SizeY - (entranceSize + 1) * entranceCount);
                    int entranceX = entrance == Face.West ? X : X + SizeX;
                    int entranceY = Y + entranceOffset;
                    GenHelper.BuildWall(tiles, entranceX, entranceY - 1, Face.West, 1, 3,
                        wallGroup["9"], wallGroup["8"]);
                    for (int i = 0; i < entranceCount; ++i) {
                        int doorOffset = (!open && i == entranceCount - 1 ? 1 : rand.Next(0, 2)) * 2;
                        int doorHeight = 3 - doorOffset;

                        open |= doorOffset >= 2;

                        GenHelper.BuildWall(tiles, entranceX, entranceY + i * (entranceSize + 1),
                            Face.West, entranceSize, doorOffset, null, null);
                        GenHelper.BuildWall(tiles, entranceX, entranceY + i * (entranceSize + 1),
                            Face.West, entranceSize, doorOffset, doorHeight, "wall/garage/0", "wall/garage/0");
                        GenHelper.BuildWall(tiles, entranceX, entranceY + i * (entranceSize + 1) + entranceSize,
                            Face.West, 1, 3, wallGroup["a"], wallGroup["a"]);

                        if (doorOffset >= 2)
                            Waypoint.AddHint(new Vector2(district.X + entranceX,
                                district.Y + entranceY + i * (entranceSize + 1) + entranceSize / 2.0f));
                    }
                    GenHelper.BuildWall(tiles, entranceX,
                        entranceY + entranceCount * (entranceSize + 1) - 1, Face.West, 1, 3,
                        wallGroup["8"], wallGroup["9"]);
                } else {
                    int entranceCount = Math.Min((SizeX - 1) / (entranceSize + 1), 3);
                    int entranceOffset = rand.Next(1, SizeX - (entranceSize + 1) * entranceCount);
                    int entranceX = X + entranceOffset;
                    int entranceY = entrance == Face.North ? Y : Y + SizeY;
                    GenHelper.BuildWall(tiles, entranceX - 1, entranceY, Face.North, 1, 3,
                        wallGroup["8"], wallGroup["9"]);
                    for (int i = 0; i < entranceCount; ++i) {
                        int doorOffset = (!open && i == entranceCount - 1 ? 1 : rand.Next(0, 2)) * 2;
                        int doorHeight = 3 - doorOffset;

                        open |= doorOffset >= 2;

                        GenHelper.BuildWall(tiles, entranceX + i * (entranceSize + 1), entranceY,
                            Face.North, entranceSize, doorOffset, null, null);
                        GenHelper.BuildWall(tiles, entranceX + i * (entranceSize + 1), entranceY,
                            Face.North, entranceSize, doorOffset, doorHeight, "wall/garage/0", "wall/garage/0");
                        GenHelper.BuildWall(tiles, entranceX + i * (entranceSize + 1) + entranceSize, entranceY,
                            Face.North, 1, 3, wallGroup["a"], wallGroup["a"]);

                        if (doorOffset >= 2)
                            Waypoint.AddHint(new Vector2(district.X + entranceX
                                + i * (entranceSize + 1) + entranceSize / 2.0f,
                                district.Y + entranceY));
                    }
                    GenHelper.BuildWall(tiles, entranceX + entranceCount * (entranceSize + 1) - 1,
                        entranceY, Face.North, 1, 3,
                        wallGroup["9"], wallGroup["8"]);
                }
            }
        }
    }
}
