using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public class CityGenerator
    {
        public World Generate(int width, int height, int seed = 0)
        {
            Random rand = (seed == 0 ? new Random() : new Random(seed));
            return Generate(width, height, rand);
        }

        public World Generate(int width, int height, Random rand)
        {
            World world = new World(width, height);
            Subdivide(world.RootDistrict, 0, 3, 3, 3, 3, rand);
            world.FindTileNeighbours();
            world.FindBlockIntersections();
            world.UpdateGeometryVertexBuffer();
            return world;
        }

        private void Subdivide(District district, int depth,
            int borderLeft, int borderTop,
            int borderRight, int borderBottom, Random rand)
        {
            int width = district.Width - borderLeft - borderRight;
            int height = district.Height - borderTop - borderBottom;

            if (BlockGenerator.WillAnyFit(width, height)) {
                BlockGenerator gen = BlockGenerator.GetRandom(width, height, rand);

                if (depth > 3 && rand.Next(width * height) == 0) {
                    district.SetBlock(gen.Generate(district, borderLeft, borderTop,
                        borderRight, borderBottom, rand));
                    return;
                }
            }

            int nextBorder = depth < 4 ? 3 : depth < 6 ? 2 : 1;

            int minHorz = 0;
            double fitHorz = 0.0;
            int minVert = 0;
            double fitVert = 0.0;

            while ((minHorz + nextBorder) * 2 + borderTop + borderBottom <= district.Height &&
                (fitHorz = BlockGenerator.FitnessScore(district.Width - borderLeft - borderRight, minHorz)) == 0.0)
                ++minHorz;

            while ((minVert + nextBorder) * 2 + borderLeft + borderRight <= district.Width &&
                (fitVert = BlockGenerator.FitnessScore(minVert, district.Height - borderTop - borderBottom)) == 0.0)
                ++minVert;

            bool horz = fitHorz > 0.0 && (fitVert == 0.0 || fitHorz > fitVert ||
                (fitHorz == fitVert && rand.NextDouble() > 0.5));

            if (horz) {
                int min = borderTop + nextBorder + minHorz;
                int max = district.Height - borderBottom - minHorz - nextBorder;
                int mid = (min + max) / 2;
                district.Split(true, rand.Next((min + mid) / 2, (mid + max) / 2));
                Subdivide(district.ChildA, depth + 1, borderLeft, borderTop, borderRight, nextBorder, rand);
                Subdivide(district.ChildB, depth + 1, borderLeft, nextBorder, borderRight, borderBottom, rand);
            } else if (fitVert > 0.0) {
                int min = minVert + borderLeft + nextBorder;
                int max = district.Width - borderRight - minVert - nextBorder;
                int mid = (min + max) / 2;
                district.Split(false, rand.Next((min + mid) / 2, (mid + max) / 2));
                Subdivide(district.ChildA, depth + 1, borderLeft, borderTop, nextBorder, borderBottom, rand);
                Subdivide(district.ChildB, depth + 1, nextBorder, borderTop, borderRight, borderBottom, rand);
            } else {
                BlockGenerator gen = BlockGenerator.GetRandom(width, height, rand);
                district.SetBlock(gen.Generate(district, borderLeft, borderTop,
                    borderRight, borderBottom, rand));
            }
        }
    }
}
