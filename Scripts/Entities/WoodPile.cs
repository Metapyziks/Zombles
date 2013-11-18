using System.Linq;

using OpenTK;

using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    public class WoodPile : Component
    {
        private Face _baseFace; 

        public int Count { get; private set; }

        public WoodPile(Entity ent)
            : base(ent)
        {
            _baseFace = Tools.Random.NextFace();

            Count = 0;
        }

        public WoodPile SetPlankCount(int value)
        {
            while (Count < value) {
                var plank = Entity.Create(Entity.World, "plank");
                AddPlank(plank);

                if (Entity.IsValid) plank.Spawn();
            }

            while (Count > value) {
                TakePlank().Remove();
            }

            return this;
        }

        public void AddPlank(Entity plank)
        {
            if (Entity.Children.Contains(plank)) return;

            Entity.AddChild(plank);

            var tier = (Count / 2);
            var alignment = (Face) (1 << ((_baseFace.GetIndex() + tier) & 3));

            plank.RelativePosition = new Vector3(0f, tier / 8f, 0f);
            plank.RelativePosition2D += alignment.GetNormal() * 0.2f * ((Count & 1) == 1 ? 1 : -1);
            plank.GetComponent<Render3D>().SetRotation(
                alignment.GetIndex() * MathHelper.PiOver2
                + Tools.Random.NextSingle(-MathHelper.Pi / 16f, MathHelper.Pi / 16f));
            
            ++Count;

            if (Count > 4 && !Entity.HasComponent<StaticTile>()) {
                Entity.AddComponent<StaticTile>();
            }
        }

        public Entity TakePlank()
        {
            if (Count == 0) return null;

            --Count;

            if (Count <= 4 && Entity.HasComponent<StaticTile>()) {
                Entity.RemoveComponent<StaticTile>();
            }

            var plank = Entity.Children.Last();
            plank.RelativePosition = new Vector3(0f, 0f, 0f);
            
            Entity.RemoveChild(plank);

            return plank;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
        }
    }
}
