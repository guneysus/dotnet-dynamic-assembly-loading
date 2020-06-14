using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;

namespace TinyAgent
{
    public abstract class PluginContextManager
    {
        protected DirectoryInfo _cacheDir;
        protected PluginContextManager() { }

    }

    public class GithubPluginContextManager : PluginContextManager
    {
        public readonly string Repo;
        public readonly string User;
    }

    public class GistPluginContextManager : PluginContextManager
    {
        public readonly AssemblyLoadContext Context;
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


        public static GistPluginContextManager New(string gist, AssemblyLoadContext context, bool enableCache = false) => new GistPluginContextManager(gist, context, enableCache);

        public void Execute(AssemblyName assemblyName, string pluginName)
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

        private bool PluginExistsInCache(AssemblyName assemblyName)
        {
            var path = GetPluginPath(assemblyName);
            return File.Exists(path);
        }

        private string GetPluginPath(AssemblyName assemblyName)
        {
            return Path.Combine(_cacheDir.FullName, assemblyName.Name, GetVersionString(assemblyName), $"{assemblyName.Name}.dll");
        }

        private void CachePlugin(AssemblyName assemblyName, MemoryStream pluginStream)
        {
            string path = GetPluginPath(assemblyName);
            string pluginDir = Path.GetDirectoryName(path);

            if (!Directory.Exists(pluginDir))
            {
                _ = Directory.CreateDirectory(pluginDir);
            }

            using var fs = new FileStream(path, FileMode.Create);
            pluginStream.CopyTo(fs);
            fs.Close();

            #region Copy if latest, to the corressponding folder
            if (GetVersionString(assemblyName) == "latest")
            {
                CopyAssemblyToSpecificVersionFolder(assemblyName, pluginStream);
            }
            #endregion

        }

        private void CopyAssemblyToSpecificVersionFolder(AssemblyName assemblyName, MemoryStream pluginStream)
        {
            var path = GetPluginPath(assemblyName);

            #region Read assembly into the memory stream
            using var ms = new MemoryStream();
            using var latest = new FileStream(path, FileMode.Open);
            latest.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            #endregion

            Assembly assembly = Assembly.Load(ms.ToArray());

            string specificVersionPluginPath = GetPluginPath(assembly.GetName());
            var specificVersionCacheDir = Path.GetDirectoryName(specificVersionPluginPath);

            if (!Directory.Exists(specificVersionCacheDir))
            {
                _ = Directory.CreateDirectory(specificVersionCacheDir);
            }

            using var specificVersionFs = new FileStream(specificVersionPluginPath, FileMode.Create);
            pluginStream.CopyTo(specificVersionFs);
            specificVersionFs.Close();
        }

        private static string GetVersionString(AssemblyName assemblyName)
        {
            string version = string.Empty;

            if (assemblyName.Version == null
                || assemblyName.Version == new Version(0, 0)
                || assemblyName.Version == new Version(0, 0, 0)
                || assemblyName.Version == new Version(0, 0, 0, 0))
            {
                version = "latest";
            }
            else
            {
                version = assemblyName.Version.ToString();
            }
            return version;

        }
    }
}
