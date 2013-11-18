using System;

using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public class Breakable : Component
    {
        protected Health Health { get; private set; }

        public Breakable(Entity ent)
            : base(ent) { }

        protected void OnKilled(Object sender, KilledEventArgs e)
        {
            OnBreak();
            Entity.Remove();
        }

        protected abstract void OnBreak();

        public override void OnSpawn()
        {
            Health = Entity.GetComponentOrNull<Health>();

            if (Health != null) {
                Health.Killed += OnKilled;
            }
        }

        public override void OnRemove()
        {
            if (Health != null) {
                Health.Killed -= OnKilled;
            }

            Health = null;
        }
    }
}
