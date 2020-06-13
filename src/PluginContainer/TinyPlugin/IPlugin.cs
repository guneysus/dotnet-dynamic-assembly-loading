using System;

namespace TinyPlugin
{
    public interface IPlugin
    {
        void Load();
        void Run();
    }
}
