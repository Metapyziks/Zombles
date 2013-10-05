using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.CodeDom.Compiler;

using Microsoft.CSharp;

using ResourceLibrary;

namespace Zombles
{
    public class ScriptFile
    {
        [ResourceTypeRegistration]
        public static void RegisterResourceType()
        {
            Archive.Register<ScriptFile>(ResourceFormat.Compressed, SaveScriptFile, LoadScriptFile,
                ".cs");
        }

        public static void SaveScriptFile(Stream stream, ScriptFile resource)
        {
            var writer = new StreamWriter(stream);
            writer.Write(resource.Contents);
            writer.Flush();
        }

        public static ScriptFile LoadScriptFile(Stream stream)
        {
            return new ScriptFile(new StreamReader(stream).ReadToEnd());
        }

        public String Contents { get; private set; }

        public ScriptFile(String contents)
        {
            Contents = contents;
        }
    }

    public static class ScriptManager
    {
        private static List<ScriptFile> stScripts = new List<ScriptFile>();

        private static Assembly stCompiledAssembly;

        private static void Compile()
        {
            CompilerParameters compParams = new CompilerParameters();

            List<String> myAllowedAssemblies = new List<String>
            {
                Assembly.GetAssembly( typeof( Math ) ).Location,
                Assembly.GetAssembly( typeof( Stopwatch ) ).Location,
                Assembly.GetAssembly( typeof( System.Linq.Enumerable ) ).Location,
                Assembly.GetAssembly( typeof( OpenTK.Vector2 ) ).Location,
                Assembly.GetAssembly( typeof( System.Drawing.Rectangle ) ).Location,
                Assembly.GetAssembly( typeof( Archive ) ).Location,
                Assembly.GetExecutingAssembly().Location
            };

            compParams.ReferencedAssemblies.AddRange(myAllowedAssemblies.ToArray());

            Dictionary<string,string> providerOptions = new Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", "v4.0");

            CodeDomProvider compiler = new CSharpCodeProvider(providerOptions);

            compParams.GenerateExecutable = false;
            compParams.GenerateInMemory = true;
            compParams.TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);
            compParams.TempFiles.KeepFiles = true;
            compParams.IncludeDebugInformation = true;

            String[] sources = new String[stScripts.Count];

            for (int i = 0; i < stScripts.Count; ++i)
                sources[i] = stScripts[i].Contents;

            CompilerResults results = compiler.CompileAssemblyFromSource(compParams, sources);

            if (results.Errors.Count > 0) {
                Debug.WriteLine(results.Errors.Count + " error" + (results.Errors.Count != 1 ? "s" : "") + " while compiling Scripts!");
                foreach (CompilerError error in results.Errors) {
                    if (error.FileName != "")
                        Debug.WriteLine((error.IsWarning ? "Warning" : "Error") + " in '" + error.FileName + "', at line " + error.Line);

                    Debug.WriteLine(error.ErrorText);
                }
                return;
            }

            stCompiledAssembly = results.CompiledAssembly;
        }

        private static void DiscoverScripts(IEnumerable<String> locator)
        {
            var locatorArr = locator.ToArray();
            foreach (var name in Archive.GetAllNames<ScriptFile>(locator)) {
                stScripts.Add(Archive.Get<ScriptFile>(locatorArr, name));
            }

            foreach (var name in Archive.GetAllNames<ScriptFile>(locator)) {
                DiscoverScripts(locator.Concat(new String[] { name }));
            }
        }

        public static void Initialize()
        {
            DiscoverScripts(new String[] { "scripts" });

            Compile();

            MethodInfo info;
            foreach (Type t in stCompiledAssembly.GetTypes())
                if ((info = t.GetMethod("Initialize", BindingFlags.Static | BindingFlags.NonPublic)) != null)
                    info.Invoke(null, new object[0]);
        }

        public static Type GetType(String typeName)
        {
            return stCompiledAssembly.GetType(typeName) ??
                Assembly.GetExecutingAssembly().GetType(typeName);
        }

        public static Type[] GetTypes(Type baseType)
        {
            Type[] types = stCompiledAssembly.GetTypes();

            List<Type> matchingTypes = new List<Type>();

            foreach (Type t in types)
                if (t.DoesExtend(baseType))
                    matchingTypes.Add(t);

            return matchingTypes.ToArray();
        }

        public static object CreateInstance(String typeName, params object[] args)
        {
            Type t = GetType(typeName);
            Type[] argTypes = new Type[args.Length];
            for (int i =0; i < args.Length; ++i)
                argTypes[i] = args[i].GetType();

            return t.GetConstructor(argTypes).Invoke(args);
        }

        public static T CreateInstance<T>(String typeName, params object[] args)
            where T : class
        {
            return CreateInstance(typeName, args) as T;
        }
    }
}
