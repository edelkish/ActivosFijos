using System.Runtime.InteropServices;

namespace ActivosFijos.Clases
{
    internal class clNativeMethods
    {
        //, SetLastError=true
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string dllName);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
        //[DllImport("CifradoLibreria.dll",
        //    CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        //public static extern string DesCifrar1(string a);//, string clave);
        //[DllImport("kernel32.dll", EntryPoint = "LoadLibrary", SetLastError = true)]
        //public static extern IntPtr LoadLibrary1(string lpFileName);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate string DesCifrar1(string number);
    }
}
