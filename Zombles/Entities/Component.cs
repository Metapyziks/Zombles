using System;
using System.Reflection;

using OpenTK;

using Zombles.Geometry;

namespace Zombles.Entities
{
    public abstract class Component
    {
        public static T Create<T>(Entity ent)
            where T : Component
        {
            Type t = typeof(T);
            ConstructorInfo c = t.GetConstructor(new Type[] { typeof(Entity) });
            if (c == null)
                throw new MissingMethodException("Type " + t.FullName + " is missing a valid constructor.");

            return (T) c.Invoke(new object[] { ent });
        }

        protected uint ID
        {
            get { return Entity.ID; }
        }

        protected World City
        {
            get { return Entity.City; }
        }

        protected Vector3 Position
        {
            get { return Entity.Position; }
        }

        protected Vector2 Position2D
        {
            get { return Entity.Position2D; }
        }

        public readonly Entity Entity;

        protected Component(Entity ent)
        {
            Entity = ent;
        }

        public virtual void OnSpawn() { }

        public virtual void OnThink(double dt) { }

        public virtual void OnRemove() { }
    }
}
