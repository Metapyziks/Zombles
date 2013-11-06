using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Geometry;

namespace Zombles.Entities
{
    public delegate void EntityBuilderDelegate(Entity ent);

    public sealed class Entity : IEnumerable<Component>
    {
        private static uint _sNextID = 0;

        private struct BuilderInfo
        {
            public readonly String Name;
            public readonly String Base;
            public readonly EntityBuilderDelegate Builder;

            public BuilderInfo(String name, EntityBuilderDelegate builder)
            {
                Name = name;
                Base = null;
                Builder = builder;
            }

            public BuilderInfo(String name, String baseName, EntityBuilderDelegate builder)
            {
                Name = name;
                Base = baseName;
                Builder = builder;
            }
        }

        private static Dictionary<String, BuilderInfo> _sEntBuilders
            = new Dictionary<string, BuilderInfo>();

        public static void Register(String name, EntityBuilderDelegate builder)
        {
            if (!_sEntBuilders.ContainsKey(name))
                _sEntBuilders.Add(name, new BuilderInfo(name, builder));
            else
                _sEntBuilders[name] = new BuilderInfo(name, builder);
        }

        public static void Register(String name, String baseName, EntityBuilderDelegate builder)
        {
            if (!_sEntBuilders.ContainsKey(name))
                _sEntBuilders.Add(name, new BuilderInfo(name, baseName, builder));
            else
                _sEntBuilders[name] = new BuilderInfo(name, baseName, builder);
        }

        public static Entity Create(World city)
        {
            return new Entity(city);
        }

        public static Entity Create(World city, String type)
        {
            BuilderInfo info = _sEntBuilders[type];
            Entity ent = (info.Base != null ? Create(city, info.Base) : Create(city));
            info.Builder(ent);
            return ent;
        }

        private List<Component> _comps;
        private Dictionary<Type, Component> _compDict;
        private Vector3 _position;
        private bool _posChanged;

        public readonly uint ID;

        public readonly World City;
        public Block Block
        {
            get;
            private set;
        }
        public bool IsValid
        {
            get;
            private set;
        }

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                if (_position.X < 0 || _position.X >= City.Width)
                    _position.X -= (int) Math.Floor(_position.X / City.Width) * City.Width;
                if (_position.Z < 0 || _position.Z >= City.Height)
                    _position.Z -= (int) Math.Floor(_position.Z / City.Height) * City.Height;
                _posChanged = true;
            }
        }

        public Vector2 Position2D
        {
            get { return new Vector2(_position.X, _position.Z); }
            set
            {
                _position.X = value.X;
                _position.Z = value.Y;
                if (_position.X < 0 || _position.X >= City.Width)
                    _position.X -= (int) Math.Floor(_position.X / City.Width) * City.Width;
                if (_position.Z < 0 || _position.Z >= City.Height)
                    _position.Z -= (int) Math.Floor(_position.Z / City.Height) * City.Height;
                _posChanged = true;
            }
        }

        private Entity(World city)
        {
            ID = _sNextID++;
            City = city;

            _comps = new List<Component>();
            _compDict = new Dictionary<Type, Component>();
            _position = new Vector3();
            _posChanged = true;

            IsValid = false;
        }

        public T AddComponent<T>()
            where T : Component
        {
            T comp = Component.Create<T>(this);
            Type type = typeof(T);

            do
                _compDict.Add(type, comp);
            while ((type = type.BaseType) != typeof(Component));

            _comps.Add(comp);

            return comp;
        }

        public void RemoveComponent<T>()
            where T : Component
        {
            T comp = GetComponent<T>();
            Type type = typeof(T);

            do
                _compDict.Remove(type);
            while ((type = type.BaseType) != typeof(Component));

            _comps.Remove(comp);
        }

        public TNew SwapComponent<TOld, TNew>()
            where TOld : Component
            where TNew : Component
        {
            TOld old = GetComponent<TOld>();

            if (IsValid)
                old.OnRemove();

            Type type = old.GetType();

            do
                _compDict.Remove(type);
            while ((type = type.BaseType) != typeof(Component));

            TNew comp = Component.Create<TNew>(this);
            type = typeof(TNew);

            do
                _compDict.Add(type, comp);
            while ((type = type.BaseType) != typeof(Component));

            _comps[_comps.IndexOf(old)] = comp;

            return comp;
        }

        public bool HasComponent<T>()
            where T : Component
        {
            return _compDict.ContainsKey(typeof(T));
        }

        public bool HasComponent(Type t)
        {
            return _compDict.ContainsKey(t);
        }

        public T GetComponent<T>()
            where T : Component
        {
            return (T) _compDict[typeof(T)];
        }

        public T GetComponentOrNull<T>()
            where T : Component
        {
            if (_compDict.ContainsKey(typeof(T))) {
                return (T) _compDict[typeof(T)];
            } else {
                return null;
            }
        }

        public Component GetComponent(Type t)
        {
            return _compDict[t];
        }

        public Component GetComponentOrNull(Type t)
        {
            if (_compDict.ContainsKey(t)) {
                return _compDict[t];
            } else {
                return null;
            }
        }

        public void Spawn()
        {
            if (!IsValid) {
                IsValid = true;
                UpdateBlock();

                foreach (Component comp in this)
                    comp.OnSpawn();
            }
        }

        public void UpdateComponents()
        {
            foreach (Component comp in this)
                comp.OnSpawn();
        }

        public void Remove()
        {
            if (IsValid) {
                foreach (Component comp in this)
                    comp.OnRemove();

                IsValid = false;
            }
        }

        public void Think(double dt)
        {
            for (int i = _comps.Count - 1; i >= 0; --i)
                _comps[i].OnThink(dt);
        }

        public void UpdateBlock()
        {
            if (IsValid && _posChanged) {
                Block newBlock = City.GetBlock(_position.X, _position.Z);
                if (newBlock != Block) {
                    if (Block != null)
                        Block.RemoveEntity(this);
                    Block = newBlock;
                    Block.AddEntity(this);
                }
                _posChanged = false;
            } else if (!IsValid && Block != null) {
                Block.RemoveEntity(this);
                Block = null;
            }
        }

        public IEnumerator<Component> GetEnumerator()
        {
            return _comps.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
