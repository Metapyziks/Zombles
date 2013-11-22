using System;
using System.Collections.Generic;
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

            protected World World { get { return _stack.World; } }
            protected Entity Entity { get { return _stack.Entity; } }
            protected Human Human { get { return _stack.Human; } }

            protected IEnumerable<Entity> SearchNearbyEnts(float radius)
            {
                var iter = new NearbyEntityEnumerator(World, Position2D, radius);
                while (iter.MoveNext()) yield return iter.Current;
            }

            protected IEnumerable<Entity> SearchNearbyVisibleEnts(float radius, Func<Entity, Vector2, bool> where)
            {
                var trace = new TraceLine(World);
                trace.Origin = Entity.Position2D;
                trace.HitGeometry = true;
                trace.HitEntities = false;
                trace.HullSize = Entity.GetComponent<Collision>().Size;

                foreach (var ent in SearchNearbyEnts(radius)) {
                    if (!where(ent, World.Difference(Entity.Position2D, ent.Position2D))) continue;

                    trace.Target = ent.Position2D;

                    if (trace.GetResult().Hit) continue;

                    yield return ent;
                }
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
