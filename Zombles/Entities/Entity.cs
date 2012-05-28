using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Geometry;

namespace Zombles.Entities
{
    public sealed class Entity : IEnumerable<Component>
    {
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
                if ( !myPosition.Equals( value ) )
                {
                    myPosition = value;
                    myPosChanged = true;
                }
            }
        }

        public Entity( City city )
        {
            City = city;

            myComponents = new Dictionary<Type, Component>();
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
                myComponents.Add( typeof( T ), comp );
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
