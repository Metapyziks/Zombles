using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.CodeDom.Compiler;

using Microsoft.CSharp;

using ResourceLib;

namespace Zombles
{
    public class RScriptManager : RManager
    {
        public RScriptManager()
            : base( typeof( ScriptFile ), 1, "cs" )
        {

        }

        public override ResourceItem[] LoadFromFile( string keyPrefix, string fileName, string fileExtension, FileStream stream )
        {
            StreamReader reader = new StreamReader( stream );
            String contents = reader.ReadToEnd();

            ScriptFile file = new ScriptFile( keyPrefix + fileName, contents );
            Scripts.Register( file );
            ResourceItem[] items = new ResourceItem[]
        {
            new ResourceItem( keyPrefix + fileName, file )
        };

            return items;
        }

        public override object LoadFromArchive( BinaryReader stream )
        {
            ScriptFile sf = new ScriptFile( stream );
            Scripts.Register( sf );
            return sf;
        }

        public override void SaveToArchive( BinaryWriter stream, object item )
        {
            ( item as ScriptFile ).WriteToStream( stream );
        }
    }

    public class ScriptFile
    {
        public readonly String Name;
        public readonly String Contents;

        public ScriptFile( String name, String contents )
        {
            Name = name;
            Contents = contents;
        }

        public ScriptFile( BinaryReader reader )
        {
            Name = reader.ReadString();
            Contents = reader.ReadString();
        }

        public void WriteToStream( BinaryWriter writer )
        {
            writer.Write( Name );
            writer.Write( Contents );
        }
    }

    public static class Scripts
    {
        private static List<ScriptFile> stScripts = new List<ScriptFile>();

        private static Assembly stCompiledAssembly;

        internal static void Register( ScriptFile file )
        {
            for ( int i = 0; i < stScripts.Count; ++i )
                if ( stScripts[ i ].Name == file.Name )
                {
                    stScripts[ i ] = file;
                    return;
                }

            stScripts.Add( file );
        }

        public static void Compile()
        {
            CompilerParameters compParams = new CompilerParameters();

            List<String> myAllowedAssemblies = new List<String>
            {
                Assembly.GetAssembly( typeof( Math ) ).Location,
                Assembly.GetAssembly( typeof( Stopwatch ) ).Location,
                Assembly.GetAssembly( typeof( System.Linq.Enumerable ) ).Location,
                Assembly.GetAssembly( typeof( OpenTK.Vector2 ) ).Location,
                Assembly.GetAssembly( typeof( System.Drawing.Rectangle ) ).Location,
                Assembly.GetAssembly( typeof( ResourceItem ) ).Location,
                Assembly.GetExecutingAssembly().Location
            };

            compParams.ReferencedAssemblies.AddRange( myAllowedAssemblies.ToArray() );

            Dictionary<string,string> providerOptions = new Dictionary<string, string>();
            providerOptions.Add( "CompilerVersion", "v4.0" );

            CodeDomProvider compiler = new CSharpCodeProvider( providerOptions );

            compParams.GenerateExecutable = false;
            compParams.GenerateInMemory = true;

            String[] sources = new String[ stScripts.Count ];

            for ( int i = 0; i < stScripts.Count; ++i )
                sources[ i ] = stScripts[ i ].Contents;

            CompilerResults results = compiler.CompileAssemblyFromSource( compParams, sources );

            if ( results.Errors.Count > 0 )
            {
                Debug.WriteLine( results.Errors.Count + " error" + ( results.Errors.Count != 1 ? "s" : "" ) + " while compiling Scripts!" );
                foreach ( CompilerError error in results.Errors )
                {
                    if ( error.FileName != "" )
                        Debug.WriteLine( ( error.IsWarning ? "Warning" : "Error" ) + " in '" + error.FileName + "', at line " + error.Line );

                    Debug.WriteLine( error.ErrorText );
                }
                return;
            }

            stCompiledAssembly = results.CompiledAssembly;
        }

        public static void Initialise()
        {
            MethodInfo info;

            foreach ( Type t in stCompiledAssembly.GetTypes() )
                if ( ( info = t.GetMethod( "Initialise", BindingFlags.Static | BindingFlags.NonPublic ) ) != null )
                    info.Invoke( null, new object[ 0 ] );
        }

        public static Type GetType( String typeName )
        {
            if ( stCompiledAssembly == null )
                Compile();

            return stCompiledAssembly.GetType( typeName ) ??
                Assembly.GetExecutingAssembly().GetType( typeName );
        }

        public static Type[] GetTypes( Type baseType )
        {
            if ( stCompiledAssembly == null )
                Compile();

            Type[] types = stCompiledAssembly.GetTypes();

            List<Type> matchingTypes = new List<Type>();

            foreach ( Type t in types )
                if ( t.DoesExtend( baseType ) )
                    matchingTypes.Add( t );

            return matchingTypes.ToArray();
        }

        public static object CreateInstance( String typeName, params object[] args )
        {
            Type t = GetType( typeName );
            Type[] argTypes = new Type[ args.Length ];
            for ( int i =0; i < args.Length; ++i )
                argTypes[ i ] = args[ i ].GetType();

            return t.GetConstructor( argTypes ).Invoke( args );
        }

        public static T CreateInstance<T>( String typeName, params object[] args )
            where T : class
        {
            return CreateInstance( typeName, args ) as T;
        }
    }
}
