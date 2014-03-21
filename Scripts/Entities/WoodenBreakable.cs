using System;

using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public class WoodenBreakable : Breakable
    {
        public int MinPlanks { get; set; }
        public int MaxPlanks { get; set; }

        public int AveragePlanks
        {
            get
            {
                return (MinPlanks + MaxPlanks) / 2;
            }
        }

        public WoodenBreakable(Entity ent)
            : base(ent)
        {
            MinPlanks = 1;
            MaxPlanks = 1;
        }

        public WoodenBreakable SetMinPlanks(int value)
        {
            MinPlanks = value;
            return this;
        }

        public WoodenBreakable SetMaxPlanks(int value)
        {
            MaxPlanks = value;
            return this;
        }

        protected override void OnBreak()
        {
            int planks = Tools.Random.Next(MinPlanks, MaxPlanks + 1);

            if (planks <= 0) return;

            var pile = Entity.Create(Entity.World, "wood pile");

            pile.GetComponent<WoodPile>().SetPlankCount(planks);
            pile.Position2D = Entity.Position2D;

            pile.Spawn();
        }
    }
}
