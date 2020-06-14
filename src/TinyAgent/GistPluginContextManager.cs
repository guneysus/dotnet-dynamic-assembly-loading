using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;

namespace TinyAgent
{

    public class GistPluginContextManager : PluginContextManager
    {
        public readonly string Gist;

        public GistPluginContextManager(
            string gist,
            AssemblyLoadContext context,
            bool enableCache = false)
        {
            Gist = gist;
            this.Context = context;
            if (enableCache)
            {
                this._cacheDir = new DirectoryInfo(".cache");
                if (!_cacheDir.Exists) _cacheDir.Create();
            }
        }

        public void Execute(
            AssemblyName assemblyName,
            string pluginName)
        {
            Assembly plugin = Context.Assemblies.FirstOrDefault(x => x.GetName().Name == assemblyName.Name);

            if (plugin == null)
            {
                MemoryStream pluginStream = null;

                if (PluginExistsInCache(assemblyName))
                {
                    using var fs = new FileStream(GetPluginPath(assemblyName), FileMode.Open);
                    pluginStream = new MemoryStream();
                    fs.CopyTo(pluginStream);
                    pluginStream.Position = 0;

                }
                else
                {
                    pluginStream = this.GetGzippedPluginFromGist(assemblyName);

                    if (_cacheDir != default)
                    {
                        CachePlugin(assemblyName, pluginStream);
                        pluginStream.Seek(0, SeekOrigin.Begin);
                    }
                }



                pluginStream.Load(Context);
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
