using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using static TinyApplication.Helpers;

namespace TinyApplication
{
    public class PluginLoadContext : AssemblyLoadContext
    {
        public PluginLoadContext(string? name, bool isCollectible = false) : base(name, isCollectible)
        {
            this.Resolving += Context_Resolving;
            this.Unloading += Context_Unloading;
        }

        private static System.Reflection.Assembly Context_Resolving(AssemblyLoadContext context, System.Reflection.AssemblyName assemblyName)
        {
            Log($"RESOLVING ASSEMBLY: {assemblyName.FullName} at {context.Name}");
            var assembly = Assembly.Load(GetAssemblyByteArrayFromFile(assemblyName));
            Log($"RESOLVED ASSEMBLY: {assembly.FullName} at {context.Name}");
            return assembly;
        }

        private static void Context_Unloading(AssemblyLoadContext self)
        {
            Log($"UNLOADING {self.Name}");
        }

        public static void Execute(AssemblyName assemblyName)
        {
            var context = new PluginLoadContext(name: "Sandbox", isCollectible: false);

            context.LoadPlugin(assemblyName);

            var plugin = context.Assemblies.First(x => x.GetName().Name == assemblyName.Name);

            Log($"LOADED ASSEMBLY {plugin.FullName}");
            plugin.ExportedTypes.ToList().ForEach(x =>
                {
                    Log($"EXPORTED TYPE: {x.Name}");
                });

            Log($"INVOKING ENTRYPOINT of {plugin.FullName}");
            plugin.EntryPoint.Invoke(null, new object[] { new string[] { } });


        }

        public void LoadPlugin(AssemblyName assemblyName)
        {
            Log($"LOADING PLUGIN {assemblyName.Name}");
            //this.LoadFromStream(GetAssemblyMemoryStreamFromFile(assemblyName));
            this.LoadFromStream(GetAssemblyMemoryStreamFromGIST(assemblyName, "a6d1cd71142d9c2c8c6533cb918f1ca2", true));
            Log($"LOADED PLUGIN {assemblyName.Name}");
        }

        public static byte[] GetAssemblyByteArrayFromFile(AssemblyName name)
        {
            return File.ReadAllBytes(GetPluginPath(name));
        }

        public static Stream GetAssemblyMemoryStreamFromFile(AssemblyName name)
        {
            var rawStream = new MemoryStream();
            using (var fs = new FileStream(GetPluginPath(name), FileMode.Open))
            {
                fs.CopyTo(rawStream);
            }
            rawStream.Position = 0;
            return rawStream;

        }

        private static string GetPluginPath(AssemblyName name)
        {
            return @$"D:\repos\dotnet-dynamic-assembly-loading\release\plugins\netstandard2.0\{name.Name}.dll";
        }

        public static async Task<MemoryStream> GetAssemblyMemoryStreamAsync(AssemblyName name)
        {
            using (var http = new HttpClient())
            {
                const string url = "https://gist.githubusercontent.com/guneysus/3e598b15a673b44bdfac22b17602df3d/raw/TinyLib.dll-1.0.2.dll.b64.gz";

                var response = await http.GetStringAsync(url);
                var rawCompressedAssembly = Convert.FromBase64String(response);

                var rawStream = new MemoryStream();

                using (var gz = new GZipStream(new MemoryStream(rawCompressedAssembly), CompressionMode.Decompress))
                {
                    gz.CopyTo(rawStream);
                }

                return rawStream;
            }
        }

        public static Stream GetAssemblyMemoryStreamFromGIST(AssemblyName assemblyName, string gistId, bool gzipped = false)
        {
            var suffix = gzipped ? ".gz" : string.Empty;
            var uri = $"https://gist.githubusercontent.com/guneysus/{gistId}/raw/{assemblyName.Name}.dll{suffix}";

            Log("START Get Assembly Stream");

            using (var http = new HttpClient())
            {

                Log("DOWNLOAD STARTING");
                //var response = http.GetStringAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult();
                // var response = http.GetStreamAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult(); // Not supported :(
                var response = http.GetAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult(); // Not supported :(
                Log("DOWNLOAD FINISHED");

                var rawStream = new MemoryStream();

                response.Content.CopyToAsync(rawStream).ConfigureAwait(false).GetAwaiter().GetResult();

                if (gzipped)
                {
                    rawStream.Position = 0;

                    var gunzipStream = new MemoryStream();
                    using (var gz = new GZipStream(rawStream, CompressionMode.Decompress))
                    {
                        gz.CopyTo(gunzipStream);
                    }

                    gunzipStream.Position = 0;
                    rawStream = null;
                    return gunzipStream;
                }


                Log("RETURN Get Assembly Stream");
                return rawStream;
            }
        }
    }

    public class Program
    {
        private static void Main(string[] args)
        {
            var plugin = new AssemblyName("HelloPlugin");
            PluginLoadContext.Execute(plugin);
        }

    }


}
