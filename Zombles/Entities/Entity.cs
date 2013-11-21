using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Geometry;
using Zombles.Graphics;

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

        public static Entity Create(World world)
        {
            return new Entity(world);
        }

        public static Entity Create(World world, String type)
        {
            BuilderInfo info = _sEntBuilders[type];
            Entity ent = (info.Base != null ? Create(world, info.Base) : Create(world));
            ent.PushClassName(type);
            info.Builder(ent);
            return ent;
        }

        private Stack<String> _classNames;

        private List<Component> _comps;
        private Dictionary<Type, Component> _compDict;
        private List<Entity> _children;

        private Block _block;
        private Vector3 _position;
        private bool _posChanged;

        public readonly uint ID;

        public String ClassName { get { return _classNames.LastOrDefault(); } }

        public Entity Parent { get; private set; }

        public IEnumerable<Entity> Children { get { return _children; } }

        public bool HasParent
        {
            get { return Parent != null; }
        }

        public readonly World World;
        public Block Block
        {
            get { return HasParent ? Parent.Block : _block; }
            private set { _block = value; }
        }
        public bool IsValid
        {
            get;
            private set;
        }

        public Vector3 RelativePosition
        {
            get { return _position; }
            set
            {
                _position = value;
                
                if (!HasParent) {
                    if (_position.X < 0 || _position.X >= World.Width)
                        _position.X -= (int) Math.Floor(_position.X / World.Width) * World.Width;
                    if (_position.Z < 0 || _position.Z >= World.Height)
                        _position.Z -= (int) Math.Floor(_position.Z / World.Height) * World.Height;
                }

                _posChanged = true;
            }
        }

        public Vector2 RelativePosition2D
        {
            get { return _position.Xz; }
            set
            {
                RelativePosition = new Vector3(value.X, _position.Y, value.Y);
            }
        }

        public Vector3 Position
        {
            get { return HasParent ? Parent.Position + _position : _position; }
            set
            {
                if (HasParent) {
                    RelativePosition = value - Parent.Position;
                } else {
                    RelativePosition = value;
                }
            }
        }

        public Vector2 Position2D
        {
            get { return Position.Xz; }
            set
            {
                Position = new Vector3(value.X, Position.Y, value.Y);
            }
        }

        private Entity(World world)
        {
            ID = _sNextID++;
            World = world;

            _classNames = new Stack<string>();

            _comps = new List<Component>();
            _compDict = new Dictionary<Type, Component>();
            _position = new Vector3();
            _posChanged = true;

            _children = new List<Entity>();

            IsValid = false;
        }

        private void PushClassName(String className)
        {
            _classNames.Push(className);
        }

        public bool IsOfClass(String className)
        {
            return _classNames.Contains(className);
        }

        public Entity AddChild(Entity child)
        {
            if (_children.Contains(child)) return child;

            if (child.HasParent) {
                child.Parent.RemoveChild(child);
            }

            _children.Add(child);
            
            child.Parent = this;
            child.RelativePosition -= Position;

            if (IsValid && !child.IsValid) child.Spawn();
            else child.UpdateBlock();

            return child;
        }

        public Entity RemoveChild(Entity child)
        {
            if (!_children.Contains(child)) return child;

            _children.Remove(child);

            child.Parent = null;
            child.RelativePosition += Position;
            child.UpdateBlock();

            return child;
        }

        public T AddComponent<T>()
            where T : Component
        {
            T comp = Component.Create<T>(this);
            Type type = typeof(T);

            do _compDict.Add(type, comp);
            while ((type = type.BaseType) != typeof(Component));

            _comps.Add(comp);

            return comp;
        }

        public Entity RemoveComponent<T>()
            where T : Component
        {
            T comp = GetComponent<T>();
            Type type = typeof(T);

            do _compDict.Remove(type);
            while ((type = type.BaseType) != typeof(Component));

            _comps.Remove(comp);

            return this;
        }

        public TNew SwapComponent<TOld, TNew>()
            where TOld : Component
            where TNew : Component
        {
            TOld old = GetComponent<TOld>();

            if (IsValid)
                old.OnRemove();

            Type type = old.GetType();

            do _compDict.Remove(type);
            while ((type = type.BaseType) != typeof(Component));

            TNew comp = Component.Create<TNew>(this);
            type = typeof(TNew);

            do _compDict.Add(type, comp);
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

        public Entity Spawn()
        {
            if (!IsValid) {
                IsValid = true;
                UpdateBlock();

                foreach (var comp in this) comp.OnSpawn();
                foreach (var child in _children) child.Spawn();
            }

            return this;
        }

        public Entity UpdateComponents()
        {
            foreach (var comp in this) comp.OnSpawn();

            return this;
        }

        public void Remove()
        {
            if (IsValid) {
                foreach (var comp in this) comp.OnRemove();
                foreach (var child in _children) child.Remove();

                IsValid = false;
            }
        }

        public void Think(double dt)
        {
            for (int i = _comps.Count - 1; i >= 0; --i)
                _comps[i].OnThink(dt);

            foreach (var child in _children) child.Think(dt);
        }

        public void Render(FlatEntityShader shader)
        {
            var comp = GetComponentOrNull<Render2D>();
            if (comp != null) comp.OnRender(shader);

            foreach (var child in _children) child.Render(shader);
        }

        public void Render(ModelEntityShader shader)
        {
            var comp = GetComponentOrNull<Render3D>();
            if (comp != null) comp.OnRender(shader);

            foreach (var child in _children) child.Render(shader);
        }

        public void UpdateBlock()
        {
            if (IsValid && !HasParent && (_posChanged || _block == null)) {
                Block newBlock = World.GetBlock(_position.X, _position.Z);
                if (newBlock != _block) {
                    if (_block != null)
                        _block.RemoveEntity(this);
                    _block = newBlock;
                    _block.AddEntity(this);
                }
                _posChanged = false;
            } else if ((HasParent || !IsValid) && _block != null) {
                _block.RemoveEntity(this);
                _block = null;
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
