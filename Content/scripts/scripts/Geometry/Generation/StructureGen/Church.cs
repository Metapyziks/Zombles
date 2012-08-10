using System;
using OpenTK;
using Zombles;
using Zombles.Geometry;
using Zombles.Geometry.Generation;
using Zombles.Entities;

namespace Zombles.Scripts.Geometry.Generation.StructureGen
{
    class Church : StructureGenerator
    {

        public Face EntranceFace { get; set; }

        public Church( Face entranceFace = Face.All )
        {

            EntranceFace = entranceFace;

        }

        public override void Generate(District district, TileBuilder[,] tiles, int x, int y, int width, int height, Random rand)
        {
            Func<int, int, Face, bool, string> wallFunc =
                delegate(int horzPos, int level, Face face, bool isInterior)
                {
                    return "wall_brick3_0";


                };

            Func<int, int, string> floorFunc =
                delegate( int horzPos, int vertPos )
                {

                    return "floor_church_0";

                };

            GenHelper.BuildFloor(tiles, x, y, width, height, 0, floorFunc);


        }

    }
}
