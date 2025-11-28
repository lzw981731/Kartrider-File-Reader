using eP.Command;
using KartLibrary.Xml;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using eP.Testing;

namespace KartLibrary.Tests
{
    internal class Program
    {
        static unsafe void Main(string[] args)
        {
            TestProgram testProgram = new TestProgram();
            testProgram.Run();
        }
    }
}


