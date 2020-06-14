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
        public readonly string Repo;
        public readonly string User;
    }
}
