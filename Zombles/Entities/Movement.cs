using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Zombles.Entities
{
    public class Movement : Component
    {
        private Collision _collision;
        private Vector2 _velocity;

        public Vector2 Velocity
        {
            get { return _velocity; }
            set
            {
                _velocity = value;
                IsMoving = _velocity.X != 0.0f || _velocity.Y != 0.0f;
            }
        }
        public bool IsMoving { get; private set; }

        public Movement(Entity ent)
            : base(ent)
        {
            Velocity = new Vector2();
        }

        public override void OnSpawn()
        {
            _collision = null;

            if (Entity.HasComponent<Collision>())
                _collision = Entity.GetComponent<Collision>();
        }

        public void Stop()
        {
            Velocity = new Vector2();
        }

        public override void OnThink(double dt)
        {
            if (Velocity.LengthSquared > 0.0f)
                Move(Velocity * (float) dt);
        }

        public void Move(Vector2 move)
        {
            if (_collision != null) {
                Entity.Position2D = _collision.TryMove(move);
            }
        }
    }
}
