using System.Linq;

using OpenTK;

using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public class Plank : Item
    {
        public Plank(Entity ent) : base(ent) { }

        public override void OnDrop(Entity holder)
        {
            base.OnDrop(holder);

            var pile = World.GetBlock(Entity.Position2D)
                .Where(x => x.HasComponent<WoodPile>())
                .FirstOrDefault(x => World.Difference(Entity.Position2D, x.Position2D)
                    .LengthSquared < 1f);

            if (pile == null) {
                var tile = World.GetTile(Position2D);

                pile = Entity.Create(World, "wood pile");
                pile.Position2D = new Vector2(tile.X + .5f, tile.Y + .5f);
                pile.GetComponent<WoodPile>().SetPlankCount(1);
                pile.Spawn();

                Entity.Remove();
            } else {
                pile.GetComponent<WoodPile>().AddPlank(Entity);
            }
        }
    }
}
