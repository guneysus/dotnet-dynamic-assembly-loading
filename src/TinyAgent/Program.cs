using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;

namespace TinyAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            for (; ; )
            {
                GistPluginContextManager pluginManager = GistPluginContextManager.New(
                    gist: "a6d1cd71142d9c2c8c6533cb918f1ca2",
                    context: AssemblyLoadContextFactory.New(isCollectible: true),
                    enableCache: true
                    );
                // https://github.com/guneysus/dotnet-core-plugins/raw/master/plugins/HelloPlugin/latest/HelloPlugin.dll

                //var helloPluginStream = pluginManager
                //    .GetGzippedPlugin(
                //        assemblyName: new AssemblyName("HelloPlugin")
                //        {
                //            Version = Version.Parse("0.0.1")
                //        });

                //pluginManager.GetGzippedPlugin(new AssemblyName("HelloPlugin")).Load(pluginManager.Context);
                //helloPluginStream.Load(pluginManager.Context);


                pluginManager.Execute(new AssemblyName("HelloPlugin")
                {
                    Version = new Version(0, 0)
                }, "HelloPlugin");
                Console.ReadKey();
            }
        }
    }
}
