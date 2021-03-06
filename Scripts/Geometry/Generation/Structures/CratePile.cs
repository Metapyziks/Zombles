﻿using System;

using OpenTK;

using Zombles.Entities;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.Structures
{
    public class CratePile : StructureGenerator
    {
        public int MaxHeight { get; set; }
        public double CrateFrequency { get; set; }

        public CratePile()
        {
            MaxHeight = 2;
            CrateFrequency = 0.1;
        }

        public override void Generate(District district, TileBuilder[,] tiles, Random rand)
        {
            for (int x = 0; x < SizeX; ++x) {
                for (int y = 0; y < SizeY; ++y) {
                    if (rand.NextDouble() <= CrateFrequency) {
                        var crateClass = rand.NextDouble() < 2.0 / 3.0
                            ? "small crate"
                            : "large crate";
                        var crate = Entity.Create(district.World, crateClass);
                        crate.Position2D = new Vector2(district.X + X + x + 0.5f, district.Y + Y + y + 0.5f);
                        district.PlaceEntity(crate);
                    }
                }
            }
        }
    }
}
