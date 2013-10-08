using System;

using OpenTK;

using Zombles;
using Zombles.Geometry;
using Zombles.Geometry.Generation;
using Zombles.Entities;
using ResourceLibrary;

namespace Zombles.Scripts.Geometry.Generation.StructureGen
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

        public override void Generate(District district, TileBuilder[,] tiles, int x, int y, int width, int height, Random rand)
        {
            if (EntranceFaces != Face.None) {
                int rheight = rand.Next(3) + 4;

                ResourceLocator wallGroup = "wall/brick" + rand.Next(2);

                GenHelper.BuildRoof(tiles, x, y, width, height, rheight, "floor/roof/0");

                Func<int,int,Face,bool,ResourceLocator> texFunc = (horzpos, level, face, isInterior) => {
                    if (level < rheight)
                        return rand.NextTexture(wallGroup, 4);

                    return wallGroup["7"];
                };

                if (Tools.Random.NextDouble() < 0.5) {
                    GenHelper.BuildFloor(tiles, x, y, width, height, 0,
                        (horz, vert) => Tools.Random.NextTexture("floor/planks", 4));
                }

                GenHelper.BuildWall(tiles, x, y, Face.North, width, rheight + 1, texFunc);
                GenHelper.BuildWall(tiles, x, y, Face.West, height, rheight + 1, texFunc);
                GenHelper.BuildWall(tiles, x, y + height - 1, Face.South, width, rheight + 1, texFunc);
                GenHelper.BuildWall(tiles, x + width - 1, y, Face.East, height, rheight + 1, texFunc);

                Face entrance = rand.NextFace(EntranceFaces);
                int entranceSize = rand.Next(2, 4);
                bool open = false;
                if (((int) entrance & 0x5) != 0) {
                    int entranceCount = Math.Min((height - 1) / (entranceSize + 1), 3);
                    int entranceOffset = rand.Next(1, height - (entranceSize + 1) * entranceCount);
                    int entranceX = entrance == Face.West ? x : x + width;
                    int entranceY = y + entranceOffset;
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
                    int entranceCount = Math.Min((width - 1) / (entranceSize + 1), 3);
                    int entranceOffset = rand.Next(1, width - (entranceSize + 1) * entranceCount);
                    int entranceX = x + entranceOffset;
                    int entranceY = entrance == Face.North ? y : y + height;
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
