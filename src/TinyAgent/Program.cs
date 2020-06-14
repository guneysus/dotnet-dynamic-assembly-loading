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
                //var gistPluginManager = PluginContextManager.NewGistPluginContextManager(
                //    gist: "a6d1cd71142d9c2c8c6533cb918f1ca2",
                //    context: AssemblyLoadContextFactory.New(isCollectible: true),
                //    cache: true);

                var githubPluginManager = PluginContextManager.NewGithubPluginContextManager(
                    "guneysus",
                    repo: "dotnet-core-plugins",
                    context: AssemblyLoadContextFactory.New(isCollectible: true),
                    cache: true
                    );

                //var helloPluginStream = pluginManager
                //    .GetGzippedPlugin(
                //        assemblyName: new AssemblyName("HelloPlugin")
                //        {
                //            Version = Version.Parse("0.0.1")
                //        });

                //pluginManager.GetGzippedPlugin(new AssemblyName("HelloPlugin")).Load(pluginManager.Context);
                //helloPluginStream.Load(pluginManager.Context);

                // gistPluginManager.Execute(new AssemblyName("HelloPlugin"), "HelloPlugin");
                githubPluginManager.Execute(new AssemblyName("HelloAlertPlugin"), "HelloAlertPlugin");

                Console.ReadKey();
            }
        }
    }
}
