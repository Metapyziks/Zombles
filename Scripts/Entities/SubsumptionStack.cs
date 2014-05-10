using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    public class SubsumptionStack : HumanControl, IEnumerable<SubsumptionStack.Layer>
    {
        public abstract class Layer
        {
            private SubsumptionStack _stack;

            internal void SetStack(SubsumptionStack stack)
            {
                _stack = stack;
                Next = stack._top;
                stack._top = this;
            }

            protected Vector3 Position { get { return _stack.Position; } }
            protected Vector2 Position2D { get { return _stack.Position2D; } }

            protected World World { get { return _stack.World; } }
            protected Entity Entity { get { return _stack.Entity; } }
            protected Human Human { get { return _stack.Human; } }

            internal Layer Next { get; private set; }

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
                if (Next != null) Next.Spawn();
                OnSpawn();
            }

            internal void Remove()
            {
                if (Next != null) Next.Remove();
                OnRemove();
            }

            internal bool Think(double dt)
            {
                if (OnThink(dt)) {
                    return true;
                } else if (Next != null) {
                    return Next.Think(dt);
                } else {
                    return false;
                }
            }
        }

        private static Stopwatch _timer = new Stopwatch();

        public static double GetLastThinkTime()
        {
            var time = _timer.Elapsed.TotalMilliseconds;
            _timer.Reset();

            return time;
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

            _timer.Start();

            if (_top != null && Human.Health.IsAlive) _top.Think(dt);

            _timer.Stop();
        }

        public override void MovementCommand(Vector2 dest)
        {
            foreach (var layer in this) {
                if (layer is Behaviours.PlayerMovementCommand) {
                    var plyMove = (Behaviours.PlayerMovementCommand) layer;
                    plyMove.Destination = dest;
                }
            }
        }

        public IEnumerator<SubsumptionStack.Layer> GetEnumerator()
        {
            var cur = _top;

            while (cur != null) {
                yield return cur;
                cur = cur.Next;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
