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
            bool enableCache = false) : base(context, enableCache)
        {
            Gist = gist;
        }


        protected override MemoryStream FetchPlugin(AssemblyName assemblyName)
        {
            var uri = $"https://gist.githubusercontent.com/guneysus/{this.Gist}/raw/{assemblyName.Name}.dll.gz";
            return GetStreamFromUri(uri);
        }
    }
}
