using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;

namespace TinyAgent
{

    public class GistPluginContextManager
    {
        public readonly AssemblyLoadContext Context;
        public readonly string Gist;

        public GistPluginContextManager(string gist, AssemblyLoadContext context)
        {
            Gist = gist;
            this.Context = context;
        }

        protected GistPluginContextManager() { }

        public static GistPluginContextManager New(string gist, AssemblyLoadContext context) => new GistPluginContextManager(gist, context);

        public void Execute(AssemblyName assemblyName, string pluginName)
        {
            Assembly plugin = Context.Assemblies.FirstOrDefault(x => x.GetName().Name == assemblyName.Name);

            if (plugin == null)
            {
                this.GetGzippedPlugin(assemblyName).Load(Context);
                plugin = Context.Assemblies.FirstOrDefault(x => x.GetName().Name == assemblyName.Name);
            }

            object instance = plugin
                .ExportedTypes
                .First(x => x.Name == pluginName)
                .GetConstructor(new Type[] { }).Invoke(new object[] { });

            MethodInfo ctor = instance
                .GetType()
                .GetMethod("Run");

            ctor.Invoke(instance, new object[] { });
        }
    }
}
