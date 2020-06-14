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
        protected AssemblyLoadContext Context;
        protected DirectoryInfo _cacheDir;
        protected PluginContextManager() { }


        public static GistPluginContextManager NewGistPluginContextManager(
            string gist,
            AssemblyLoadContext context,
            bool enableCache = false) => new GistPluginContextManager(gist, context, enableCache);

        protected static string GetVersionString(AssemblyName assemblyName)
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

        protected void CachePlugin(
            AssemblyName assemblyName,
            MemoryStream pluginStream)
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

        protected void CopyAssemblyToSpecificVersionFolder(
            AssemblyName assemblyName,
            MemoryStream pluginStream)
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

        protected string GetPluginPath(AssemblyName assemblyName)
        {
            return Path.Combine(_cacheDir.FullName, assemblyName.Name, GetVersionString(assemblyName), $"{assemblyName.Name}.dll");
        }

        protected bool PluginExistsInCache(AssemblyName assemblyName)
        {
            var path = GetPluginPath(assemblyName);
            return File.Exists(path);
        }

    }
}
