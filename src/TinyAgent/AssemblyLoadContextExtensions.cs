using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;

namespace TinyAgent
{

    public static class AssemblyLoadContextExtensions
    {
        public static MemoryStream GetGzippedPluginFromGist(this GistPluginContextManager context,
            AssemblyName assemblyName)
        {
            var uri = $"https://gist.githubusercontent.com/guneysus/{context.Gist}/raw/{assemblyName.Name}.dll.gz";
            return GetStreamFromUri(uri);
        }

        public static MemoryStream GetGzippedPluginFromGithub(this GithubPluginContextManager context,
            AssemblyName assemblyName)
        {
            var uri = $"https://github.com/{context.User}/{context.Repo}/raw/master/plugins/{assemblyName.Name}/latest/{assemblyName.Name}.dll.gz";
            return GetStreamFromUri(uri);
        }

        public static MemoryStream GetStreamFromUri(string uri)
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

        public static AssemblyLoadContext Load(this MemoryStream pluginMemoryStream, AssemblyLoadContext context)
        {
            context.LoadFromStream(pluginMemoryStream);
            return context;
        }
    }
}
