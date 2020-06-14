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
        public bool Cache { get; protected set; }

        protected PluginContextManager() { }

        protected PluginContextManager(AssemblyLoadContext context, bool cache)
        {
            this.Context = context;
            this.Cache = cache;

            if (Cache)
            {
                this._cacheDir = new DirectoryInfo(".cache");
                if (!_cacheDir.Exists) _cacheDir.Create();
            }
        }

        public static GistPluginContextManager NewGistPluginContextManager(
            string gist,
            AssemblyLoadContext context,
            bool enableCache = false) => new GistPluginContextManager(gist, context, enableCache);

        public static GithubPluginContextManager NewGithubPluginContextManager(
            string user,
            string repo,
            AssemblyLoadContext context,
            bool cache = false) => new GithubPluginContextManager(user, repo, context, cache);

        protected string GetVersionString(AssemblyName assemblyName)
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

        public void Execute(
            AssemblyName assemblyName,
            string pluginName)
        {
            Assembly plugin = Context.Assemblies.FirstOrDefault(x => x.GetName().Name == assemblyName.Name);

            if (plugin == null)
            {
                MemoryStream pluginStream = null;

                if (Cache && PluginExistsInCache(assemblyName))
                {
                    using var fs = new FileStream(GetPluginPath(assemblyName), FileMode.Open);
                    pluginStream = new MemoryStream();
                    fs.CopyTo(pluginStream);
                    pluginStream.Position = 0;

                }
                else
                {
                    pluginStream = this.FetchPlugin(assemblyName);

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

            if (specificVersionPluginPath == path)
            {
                // Plugin assembly version is 0.0.0.0
                return;
            }

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

        protected MemoryStream GetStreamFromUri(string uri)
        {
            using (var http = new HttpClient())
            {
                //var response = http.GetStringAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult();
                // var response = http.GetStreamAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult(); // Not supported :(

                using (var task = http.GetAsync(uri))
                {
                    using (HttpResponseMessage response = task.ConfigureAwait(false).GetAwaiter().GetResult())
                    {
                        //HttpResponseMessage response = ; // Not supported :(

                        var rawStream = new MemoryStream();

                        response.Content.CopyToAsync(rawStream).ConfigureAwait(false).GetAwaiter().GetResult();

                        rawStream.Position = 0;

                        var gunzipStream = new MemoryStream();
                        using (var gz = new GZipStream(rawStream, CompressionMode.Decompress))
                        {
                            gz.CopyTo(gunzipStream);
                        }

                        gunzipStream.Position = 0;
                        rawStream.Dispose();
                        return gunzipStream;
                    }
                }
            }
        }

        protected abstract MemoryStream FetchPlugin(AssemblyName assemblyName);

    }
}
