using OpenTK;
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
                if (_next != null && _next.Think(dt)) {
                    return true;
                } else {
                    return OnThink(dt);
                }
            }
        }

        private Layer _top;

        public void Push<T>()
            where T : Layer, new()
        {
            var layer = new T();
            layer.SetStack(this);
        }

        public override void OnSpawn()
        {
            if (_top != null) _top.Spawn();
        }

        public override void OnRemove()
        {
            if (_top != null) _top.Remove();
        }

        public override void OnThink(double dt)
        {
            if (_top != null) _top.Think(dt);
        }
    }
}
