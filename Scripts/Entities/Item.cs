using System;

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
            return !IsHeld && World.Difference(Entity.Position2D, holder.Position2D).LengthSquared < 1f;
        }

        public virtual bool OnPickup(Entity holder)
        {
            Holder = holder;
            Entity.Position = new Vector3(holder.Position2D.X, 0.5f, holder.Position2D.Y);
            return true;
        }

        public virtual void OnDrop(Entity holder)
        {
            if (Holder == holder) Holder = null;
            Entity.Position = new Vector3(holder.Position2D.X, 0f, holder.Position2D.Y);

            var render3D = Entity.GetComponentOrNull<Render3D>();
            if (render3D == null) return;

            render3D.SetOffset(new Vector3(0f, 0f, 0f));
            return;
        }

        public override void OnThink(double dt)
        {
            if (!IsHeld) return;

            var movement = Holder.GetComponentOrNull<Movement>();
            if (movement == null) return;

            var render3D = Entity.GetComponentOrNull<Render3D>();
            if (render3D == null) return;

            render3D.SetOffset(new Vector3(0.25f, 0f, 0f));
            render3D.SetRotation((float) Math.Atan2(-movement.Velocity.Y, movement.Velocity.X));
        }
    }
}
