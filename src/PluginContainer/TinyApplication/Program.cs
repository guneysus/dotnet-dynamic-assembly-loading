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
            var assembly = Assembly.Load(PluginManager.GetAssemblyByteArrayFromFile(assemblyName));
            Log($"RESOLVED ASSEMBLY: {assembly.FullName} at {context.Name}");
            return assembly;
        }

        private static void Context_Unloading(AssemblyLoadContext self)
        {
            Log($"UNLOADING {self.Name}");
        }
    }

    public class Program
    {
        private static void Main(string[] args)
        {
            ExecutePlugin();
        }

        public static void ExecutePlugin()
        {
            var context = new PluginLoadContext(name: "Sandbox", isCollectible: false);

            //context.Resolving += Context_Resolving;
            //context.Unloading += Context_Unloading;

            LoadPlugin(context, new AssemblyName("HelloPlugin"));

            context.Assemblies.ToList().ForEach(x =>
            {
                Log($"LOADED ASSEMBLY {x.FullName}");
                x.ExportedTypes.ToList().ForEach(x =>
                {
                    Log($"EXPORTED TYPE: {x.Name}");
                });

                Log($"INVOKING ENTRYPOINT of {x.FullName}");
                x.EntryPoint.Invoke(null, new object[] { new string[] { } });

            });

        }

        //private static void Context_Unloading(AssemblyLoadContext self)
        //{
        //    Log($"UNLOADING {self.Name}");
        //}

        //private static System.Reflection.Assembly Context_Resolving(AssemblyLoadContext context, System.Reflection.AssemblyName assemblyName)
        //{
        //    Log($"RESOLVING ASSEMBLY: {assemblyName.FullName} at {context.Name}");
        //    var assembly = Assembly.Load(PluginManager.GetAssemblyByteArrayFromFile(assemblyName));
        //    Log($"RESOLVED ASSEMBLY: {assembly.FullName} at {context.Name}");
        //    return assembly;
        //}


    }

    public static class PluginManager
    {
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

        public static MemoryStream GetAssemblyMemoryStreamFromURL(AssemblyName name, string pluginsBaseUrl = "https://gist.githubusercontent.com/guneysus/3e598b15a673b44bdfac22b17602df3d/raw")
        {
            Log("START Get Assembly Stream");

            using (var http = new HttpClient())
            {
                var uri = new Uri(new Uri(pluginsBaseUrl), relativeUri: "TinyLib.dll-1.0.2.dll.b64.gz");

                Log("DOWNLOAD STARTING");
                var response = http.GetStringAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult();
                Log("DOWNLOAD FINISHED");

                var rawCompressedAssembly = Convert.FromBase64String(response);

                var rawStream = new MemoryStream();

                using (var gz = new GZipStream(new MemoryStream(rawCompressedAssembly), CompressionMode.Decompress))
                {
                    gz.CopyTo(rawStream);
                }

                rawStream.Position = 0;

                Log("RETURN Get Assembly Stream");
                return rawStream;
            }
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

        public static byte[] GetAssemblyByteArrayFromFile(AssemblyName name)
        {
            return File.ReadAllBytes(GetPluginPath(name));
        }

        private static string GetPluginPath(AssemblyName name)
        {
            return @$"D:\repos\dotnet-dynamic-assembly-loading\release\plugins\netstandard2.0\{name.Name}.dll";
        }
    }


}
