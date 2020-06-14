using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;

namespace TinyAgent
{

    public class GithubPluginContextManager : PluginContextManager
    {
        public string Repo { get; protected set; }
        public string User { get; protected set; }

        public GithubPluginContextManager(string user,
            string repo,
            AssemblyLoadContext context,
            bool enableCache) : base(context, enableCache)
        {
            this.User = user;
            this.Repo = repo;
        }

        protected override MemoryStream FetchPlugin(AssemblyName assemblyName)
        {
            var uri = $"https://github.com/{this.User}/{this.Repo}/raw/master/plugins/{assemblyName.Name}/latest/{assemblyName.Name}.dll.gz";
            return GetStreamFromUri(uri);
        }
    }
}
