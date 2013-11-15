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
        private static List<ScriptFile> _sScripts = new List<ScriptFile>();

        private static Assembly _sCompiledAssembly;

        private static void Compile()
        {
            var compParams = new CompilerParameters();

            var requiredTypes = new[] {
                typeof(Math), typeof(Stopwatch), typeof(System.Linq.Enumerable),
                typeof(OpenTK.Vector2), typeof(System.Drawing.Rectangle), typeof(Archive),
                typeof(OpenTKTK.Scene.Camera), typeof(ScriptManager)
            };

            var allowedAssemblies = requiredTypes.Select(x => Assembly.GetAssembly(x).Location);
            compParams.ReferencedAssemblies.AddRange(allowedAssemblies.ToArray());

            Dictionary<string,string> providerOptions = new Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", "v4.0");

            CodeDomProvider compiler = new CSharpCodeProvider(providerOptions);

            compParams.GenerateExecutable = false;
            compParams.GenerateInMemory = true;
            compParams.TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);
            compParams.TempFiles.KeepFiles = false;
            compParams.IncludeDebugInformation = true;

            String[] sources = new String[_sScripts.Count];

            for (int i = 0; i < _sScripts.Count; ++i)
                sources[i] = _sScripts[i].Contents;

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

            _sCompiledAssembly = results.CompiledAssembly;
        }

        public static void Initialize()
        {
            foreach (var locator in Archive.FindAll<ScriptFile>("scripts", true)) {
                _sScripts.Add(Archive.Get<ScriptFile>(locator));
            }

            Compile();

            MethodInfo info;
            foreach (Type t in _sCompiledAssembly.GetTypes())
                if ((info = t.GetMethod("Initialize", BindingFlags.Static | BindingFlags.NonPublic)) != null)
                    info.Invoke(null, new object[0]);
        }

        public static Type GetType(String typeName)
        {
            return _sCompiledAssembly.GetType(typeName) ??
                Assembly.GetExecutingAssembly().GetType(typeName);
        }

        public static Type[] GetTypes(Type baseType)
        {
            Type[] types = _sCompiledAssembly.GetTypes();

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
