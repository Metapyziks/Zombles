using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Zombles.Geometry;
using Zombles.Graphics;

namespace Zombles
{
    public class PluginRequirementsAttribute : Attribute
    {
        public readonly String[] Requirements;

        public PluginRequirementsAttribute( params String[] args )
        {
            Requirements = args;
        }
    }

    public abstract class Plugin
    {
        private static List<Type> stPluginTypeList = new List<Type>();
        private static Dictionary<String, Plugin> stRegistered;

        protected static ZomblesGame Game { get; private set; }

        internal static void SetGame( ZomblesGame game )
        {
            Game = game;
        }

        public static void Register<T>()
            where T : Plugin
        {
            Register( typeof( T ) );
        }

        public static void Register( Type t )
        {
            ConstructorInfo c = t.GetConstructor( new Type[ 0 ] );

            if ( c != null )
            {
                for ( int i = 0; i < stPluginTypeList.Count + 1; ++i )
                {
                    if ( i == stPluginTypeList.Count )
                    {
                        stPluginTypeList.Add( t );
                        break;
                    }

                    Type reg = stPluginTypeList[ i ];
                    if ( reg.HasAttribute<PluginRequirementsAttribute>( false ) )
                    {
                        String[] reqs = reg.GetAttribute<PluginRequirementsAttribute>( false ).Requirements;
                        if ( reqs.Contains( t.FullName ) )
                        {
                            stPluginTypeList.Insert( i, t );
                            break;
                        }
                    }
                }
            }
        }

        public static void Initialize()
        {
            foreach ( Type t in ScriptManager.GetTypes( typeof( Plugin ) ) )
                Register( t );

            stRegistered = new Dictionary<string, Plugin>();

            foreach ( Type t in stPluginTypeList )
            {
                ConstructorInfo cons = t.GetConstructor( new Type[ 0 ] );
                Plugin plg = (Plugin) cons.Invoke( new object[ 0 ] );
                stRegistered.Add( t.FullName, plg );
                plg.OnInitialize();
            }
        }

        public static void CityGenerated()
        {
            foreach ( Plugin plg in stRegistered.Values )
                plg.OnCityGenerated();
        }

        public static void Think( double dt )
        {
            foreach ( Plugin plg in stRegistered.Values )
                plg.OnThink( dt );
        }

        public Plugin()
        {

        }

        protected virtual void OnInitialize()
        {

        }

        protected virtual void OnCityGenerated()
        {

        }

        protected virtual void OnThink( double dt )
        {

        }
    }
}
