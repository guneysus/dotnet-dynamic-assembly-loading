using System;
using System.Runtime.InteropServices;
using TinyPlugin;

namespace HelloAlertPlugin
{
    public class HelloAlertPlugin : IPlugin
    {
        public void Run()
        {
            var _ = MessageBox(IntPtr.Zero, "Hello", "Hello World", MB_TYPE.MB_OK | MB_TYPE.MB_ICONASTERISK);
        }

        [DllImport("User32.dll")]
        public static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, MB_TYPE uType);
    }

    [Flags]
    public enum MB_TYPE
    {
        MB_OK = 0x00000000,
        MB_ICONASTERISK = 0x00000040
    }

    public static class Application
    {
        public static void Main(string[] args)
        {
            var _ = MessageBox(IntPtr.Zero, "Hello", "Hello World", MB_TYPE.MB_OK | MB_TYPE.MB_ICONASTERISK);

        }

        [DllImport("User32.dll")]
        static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, MB_TYPE uType);
    }
}

