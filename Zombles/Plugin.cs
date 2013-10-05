using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Zombles.Geometry;
using Zombles.Graphics;
using Zombles.Entities;

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
        private static List<Type> _sPluginTypeList = new List<Type>();
        private static Dictionary<String, Plugin> _sRegistered;

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
                for ( int i = 0; i < _sPluginTypeList.Count + 1; ++i )
                {
                    if ( i == _sPluginTypeList.Count )
                    {
                        _sPluginTypeList.Add( t );
                        break;
                    }

                    Type reg = _sPluginTypeList[ i ];
                    if ( reg.HasAttribute<PluginRequirementsAttribute>( false ) )
                    {
                        String[] reqs = reg.GetAttribute<PluginRequirementsAttribute>( false ).Requirements;
                        if ( reqs.Contains( t.FullName ) )
                        {
                            _sPluginTypeList.Insert( i, t );
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

            _sRegistered = new Dictionary<string, Plugin>();

            foreach ( Type t in _sPluginTypeList )
            {
                ConstructorInfo cons = t.GetConstructor( new Type[ 0 ] );
                Plugin plg = (Plugin) cons.Invoke( new object[ 0 ] );
                _sRegistered.Add( t.FullName, plg );
                plg.OnInitialize();
            }
        }

        public static void CityGenerated()
        {
            foreach ( Plugin plg in _sRegistered.Values )
                plg.OnCityGenerated();
        }

        public static void Think( double dt )
        {
            foreach ( Plugin plg in _sRegistered.Values )
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
