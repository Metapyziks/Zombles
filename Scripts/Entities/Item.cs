using OpenTK;
using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public class Item : Component
    {
        public Entity Holder { get; private set; }

        public bool IsHeld { get { return Holder != null; } }

        public Item(Entity ent)
            : base(ent) { }

        public virtual bool CanPickup(Entity holder)
        {
            return World.Difference(Entity.Position2D, holder.Position2D).LengthSquared < 1f;
        }

        public virtual bool OnPickup(Entity holder)
        {
            Holder = holder;
            Entity.Position = new Vector3(holder.Position2D.X, 0.3f, holder.Position2D.Y);
            return true;
        }

        public virtual void OnDrop(Entity holder)
        {
            if (Holder == holder) Holder = null;
            Entity.Position = new Vector3(holder.Position2D.X, 0f, holder.Position2D.Y);
            return;
        }
    }
}
