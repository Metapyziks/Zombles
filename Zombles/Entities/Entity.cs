using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Geometry;

namespace Zombles.Entities
{
    public delegate void EntityBuilderDelegate( Entity ent );

    public sealed class Entity : IEnumerable<Component>
    {
        private struct BuilderInfo
        {
            public readonly String Name;
            public readonly String Base;
            public readonly EntityBuilderDelegate Builder;

            public BuilderInfo( String name, EntityBuilderDelegate builder )
            {
                Name = name;
                Base = null;
                Builder = builder;
            }

            public BuilderInfo( String name, String baseName, EntityBuilderDelegate builder )
            {
                Name = name;
                Base = baseName;
                Builder = builder;
            }
        }

        private static Dictionary<String, BuilderInfo> stEntBuilders
            = new Dictionary<string, BuilderInfo>();

        public static void Register( String name, EntityBuilderDelegate builder )
        {
            if ( !stEntBuilders.ContainsKey( name ) )
                stEntBuilders.Add( name, new BuilderInfo( name, builder ) );
            else
                stEntBuilders[ name ] = new BuilderInfo( name, builder );
        }

        public static void Register( String name, String baseName, EntityBuilderDelegate builder )
        {
            if ( !stEntBuilders.ContainsKey( name ) )
                stEntBuilders.Add( name, new BuilderInfo( name, baseName, builder ) );
            else
                stEntBuilders[ name ] = new BuilderInfo( name, baseName, builder );
        }

        public static Entity Create( City city )
        {
            return new Entity( city );
        }

        public static Entity Create( String type, City city )
        {
            BuilderInfo info = stEntBuilders[ type ];
            Entity ent = ( info.Base != null ? Create( info.Base, city ) : Create( city ) );
            info.Builder( ent );
            return ent;
        }

        private Dictionary<Type, Component> myComponents;
        private Vector3 myPosition;
        private bool myPosChanged;

        public readonly City City;
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
            get { return myPosition; }
            set
            {
                myPosition = value;
                if( myPosition.X < 0 || myPosition.X >= City.Width )
                    myPosition.X -= (int) Math.Floor( myPosition.X / City.Width ) * City.Width;
                if ( myPosition.Z < 0 || myPosition.Z >= City.Height )
                    myPosition.Z -= (int) Math.Floor( myPosition.Z / City.Height ) * City.Height;
                myPosChanged = true;
            }
        }

        private Entity( City city )
        {
            City = city;

            myComponents = new Dictionary<Type, Component>();
            myPosition = new Vector3();
            myPosChanged = true;

            IsValid = false;
        }

        public T SetComponent<T>()
            where T : Component
        {
            T comp = Component.Create<T>( this );
            Type type = typeof( T );

            do
            {
                if ( myComponents.ContainsKey( type ) )
                    myComponents[ type ] = comp;
                else
                    myComponents.Add( type, comp );
            }
            while ( ( type = type.BaseType ) != typeof( Component ) );

            return comp;
        }

        public bool HasComponent<T>()
            where T : Component
        {
            return myComponents.ContainsKey( typeof( T ) );
        }

        public bool HasComponent( Type t )
        {
            return myComponents.ContainsKey( t );
        }

        public T GetComponent<T>()
            where T : Component
        {
            return (T) myComponents[ typeof( T ) ];
        }

        public Component GetComponent( Type t )
        {
            return myComponents[ t ];
        }

        public void Spawn()
        {
            if ( !IsValid )
            {
                IsValid = true;
                UpdateBlock();

                foreach ( Component comp in this )
                    comp.OnSpawn();
            }
        }

        public void Remove()
        {
            if ( IsValid )
            {
                foreach ( Component comp in this )
                    comp.OnRemove();

                IsValid = false;
            }
        }

        public void Think( double dt )
        {
            foreach ( Component comp in this )
                comp.OnThink( dt );
        }

        public void UpdateBlock()
        {
            if ( IsValid && myPosChanged )
            {
                Block newBlock = City.GetBlock( myPosition.X, myPosition.Z );
                if ( newBlock != Block )
                {
                    if ( Block != null )
                        Block.RemoveEntity( this );
                    Block = newBlock;
                    Block.AddEntity( this );
                }
                myPosChanged = false;
            }
            else if ( !IsValid && Block != null )
            {
                Block.RemoveEntity( this );
                Block = null;
            }
        }

        public IEnumerator<Component> GetEnumerator()
        {
            return myComponents.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
