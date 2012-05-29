using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Zombles.Graphics;

namespace Zombles.Entities
{
    public abstract class Component
    {
        public static T Create<T>( Entity ent )
            where T : Component
        {
            Type t = typeof( T );
            ConstructorInfo c = t.GetConstructor( new Type[] { typeof( Entity ) } );
            if ( c == null )
                throw new MissingMethodException( "Type " + t.FullName + " is missing a valid constructor." );

            return (T) c.Invoke( new object[] { ent } );
        }

        public readonly Entity Entity;

        protected Component( Entity ent )
        {
            Entity = ent;
        }

        public virtual void OnSpawn()
        {

        }

        public virtual void OnThink( double dt )
        {

        }

        public virtual void OnRemove()
        {

        }
    }
}
