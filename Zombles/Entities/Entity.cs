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
        private static uint stNextID = 0;

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

        public static Entity Create( City city, String type )
        {
            BuilderInfo info = stEntBuilders[ type ];
            Entity ent = ( info.Base != null ? Create( city, info.Base ) : Create( city ) );
            info.Builder( ent );
            return ent;
        }

        private List<Component> myComps;
        private Dictionary<Type, Component> myCompDict;
        private Vector3 myPosition;
        private bool myPosChanged;

        public readonly uint ID;

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

        public Vector2 Position2D
        {
            get { return new Vector2( myPosition.X, myPosition.Z ); }
            set
            {
                myPosition.X = value.X;
                myPosition.Z = value.Y;
                if ( myPosition.X < 0 || myPosition.X >= City.Width )
                    myPosition.X -= (int) Math.Floor( myPosition.X / City.Width ) * City.Width;
                if ( myPosition.Z < 0 || myPosition.Z >= City.Height )
                    myPosition.Z -= (int) Math.Floor( myPosition.Z / City.Height ) * City.Height;
                myPosChanged = true;
            }
        }

        private Entity( City city )
        {
            ID = stNextID++;
            City = city;

            myComps = new List<Component>();
            myCompDict = new Dictionary<Type, Component>();
            myPosition = new Vector3();
            myPosChanged = true;

            IsValid = false;
        }

        public T AddComponent<T>()
            where T : Component
        {
            T comp = Component.Create<T>( this );
            Type type = typeof( T );

            do
                myCompDict.Add( type, comp );
            while ( ( type = type.BaseType ) != typeof( Component ) );

            myComps.Add( comp );

            return comp;
        }

        public void RemoveComponent<T>()
            where T : Component
        {
            T comp = GetComponent<T>();
            Type type = typeof( T );

            do
                myCompDict.Remove( type );
            while ( ( type = type.BaseType ) != typeof( Component ) );

            myComps.Remove( comp );
        }

        public TNew SwapComponent<TOld, TNew>()
            where TOld : Component
            where TNew : Component
        {
            TOld old = GetComponent<TOld>();

            if( IsValid )
                old.OnRemove();

            Type type = old.GetType();

            do
                myCompDict.Remove( type );
            while ( ( type = type.BaseType ) != typeof( Component ) );

            TNew comp = Component.Create<TNew>( this );
            type = typeof( TNew );

            do
                myCompDict.Add( type, comp );
            while ( ( type = type.BaseType ) != typeof( Component ) );

            myComps[ myComps.IndexOf( old ) ] = comp;

            return comp;
        }

        public bool HasComponent<T>()
            where T : Component
        {
            return myCompDict.ContainsKey( typeof( T ) );
        }

        public bool HasComponent( Type t )
        {
            return myCompDict.ContainsKey( t );
        }

        public T GetComponent<T>()
            where T : Component
        {
            return (T) myCompDict[ typeof( T ) ];
        }

        public Component GetComponent( Type t )
        {
            return myCompDict[ t ];
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

        public void UpdateComponents()
        {
            foreach ( Component comp in this )
                comp.OnSpawn();
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
            for( int i = myComps.Count - 1; i >= 0; -- i )
                myComps[ i ].OnThink( dt );
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
            return myComps.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
