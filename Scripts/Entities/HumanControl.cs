using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public class HumanControl : Component
    {
        protected Human Human { get; private set; }

        public HumanControl(Entity ent)
            : base(ent)
        {

        }

        public override void OnSpawn()
        {
            if (Entity.HasComponent<Human>())
                Human = Entity.GetComponent<Human>();
        }

        protected NearbyEntityEnumerator SearchNearbyEnts(float radius)
        {
            return new NearbyEntityEnumerator(World, Position2D, radius);
        }

        public virtual void MovementCommand(Vector2 dest)
        {

        }
    }
}
