using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    public class SubsumptionStack : HumanControl
    {
        public abstract class Layer
        {
            private SubsumptionStack _stack;
            private Layer _next;

            internal void SetStack(SubsumptionStack stack)
            {
                _stack = stack;
                _next = stack._top;
                stack._top = this;
            }

            protected Vector3 Position { get { return _stack.Position; } }
            protected Vector2 Position2D { get { return _stack.Position2D; } }

            protected City City { get { return _stack.City; } }
            protected Human Human { get { return _stack.Human; } }

            protected NearbyEntityEnumerator SearchNearbyEnts(float radius)
            {
                return new NearbyEntityEnumerator(City, Position2D, radius);
            }

            protected virtual void OnSpawn() { }
            protected virtual void OnRemove() { }

            protected abstract bool OnThink(double dt);

            internal void Spawn()
            {
                if (_next != null) _next.Spawn();
                OnSpawn();
            }

            internal void Remove()
            {
                if (_next != null) _next.Remove();
                OnRemove();
            }

            internal bool Think(double dt)
            {
                if (OnThink(dt)) {
                    return true;
                } else if (_next != null) {
                    return _next.Think(dt);
                } else {
                    return false;
                }
            }
        }

        private Layer _top;

        public SubsumptionStack(Entity ent)
            : base(ent) { }

        public SubsumptionStack Push<T>()
            where T : Layer, new()
        {
            var layer = new T();
            layer.SetStack(this);
            return this;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            if (_top != null) _top.Spawn();
        }

        public override void OnRemove()
        {
            base.OnRemove();

            if (_top != null) _top.Remove();
        }

        public override void OnThink(double dt)
        {
            base.OnThink(dt);

            if (_top != null && Human.Health.IsAlive) _top.Think(dt);
        }
    }
}
