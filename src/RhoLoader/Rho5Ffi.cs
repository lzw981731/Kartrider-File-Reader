using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RhoLoader
{
    /// <summary>
    /// P/Invoke 桥接：调用 native/rho5-ffi (Rust cdylib) 提供的 RHO5 读写能力。
    /// DLL 名 rho5ffi，随 RhoLoader.exe 同目录分发。
    /// </summary>
    public static class Rho5Ffi
    {
        private const string DllName = "rho5ffi.dll";

        [StructLayout(LayoutKind.Sequential)]
        public struct FfiEntry
        {
            public IntPtr Path;
            public IntPtr Archive;
            public long CompressedSize;
            public long PlaintextSize;
        }

        // ---------- extern ----------
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr rho5_open(IntPtr dataDir);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void rho5_close(IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int rho5_list(IntPtr handle, out IntPtr entries, out long count);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void rho5_free_entries(IntPtr entries, long count);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int rho5_extract(IntPtr handle, IntPtr path, out IntPtr data, out long len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void rho5_free_bytes(IntPtr ptr, long len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int rho5_replace(IntPtr handle, IntPtr path, IntPtr data, long len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int rho5_last_error(IntPtr buf, int bufCap);

        // ---------- 托管包装 ----------

        /// <summary>打开 Data 目录（KR 区）。返回句柄；失败抛异常。</summary>
        public static IntPtr Open(string dataDir)
        {
            IntPtr dirPtr = Marshal.StringToCoTaskMemUTF8(dataDir);
            try
            {
                IntPtr handle = rho5_open(dirPtr);
                if (handle == IntPtr.Zero)
                    throw new InvalidOperationException(LastError());
                return handle;
            }
            finally
            {
                Marshal.FreeCoTaskMem(dirPtr);
            }
        }

        public static void Close(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
                rho5_close(handle);
        }

        public sealed class Entry
        {
            public string Path { get; init; } = "";
            public string Archive { get; init; } = "";
            public long CompressedSize { get; init; }
            public long PlaintextSize { get; init; }
        }

        public static List<Entry> List(IntPtr handle)
        {
            if (rho5_list(handle, out IntPtr entries, out long count) != 0)
                throw new InvalidOperationException(LastError());
            var result = new List<Entry>();
            try
            {
                int size = Marshal.SizeOf<FfiEntry>();
                for (long i = 0; i < count; i++)
                {
                    FfiEntry raw = Marshal.PtrToStructure<FfiEntry>(entries + (nint)(i * size));
                    result.Add(new Entry
                    {
                        Path = Marshal.PtrToStringUTF8(raw.Path) ?? "",
                        Archive = Marshal.PtrToStringUTF8(raw.Archive) ?? "",
                        CompressedSize = raw.CompressedSize,
                        PlaintextSize = raw.PlaintextSize,
                    });
                }
            }
            finally
            {
                rho5_free_entries(entries, count);
            }
            return result;
        }

        public static byte[] Extract(IntPtr handle, string path)
        {
            IntPtr pathPtr = Marshal.StringToCoTaskMemUTF8(path);
            try
            {
                if (rho5_extract(handle, pathPtr, out IntPtr data, out long len) != 0)
                    throw new InvalidOperationException(LastError());
                if (len <= 0)
                {
                    rho5_free_bytes(data, len);
                    return Array.Empty<byte>();
                }
                byte[] result = new byte[len];
                Marshal.Copy(data, result, 0, (int)len);
                rho5_free_bytes(data, len);
                return result;
            }
            finally
            {
                Marshal.FreeCoTaskMem(pathPtr);
            }
        }

        public static void Replace(IntPtr handle, string path, byte[] data)
        {
            IntPtr pathPtr = Marshal.StringToCoTaskMemUTF8(path);
            IntPtr dataPtr = Marshal.AllocHGlobal(Math.Max(1, data.Length));
            try
            {
                Marshal.Copy(data, 0, dataPtr, data.Length);
                int rc = rho5_replace(handle, pathPtr, dataPtr, data.Length);
                if (rc != 0)
                    throw new InvalidOperationException(LastError());
            }
            finally
            {
                Marshal.FreeCoTaskMem(pathPtr);
                Marshal.FreeHGlobal(dataPtr);
            }
        }

        public static string LastError()
        {
            byte[] buf = new byte[2048];
            int len = rho5_last_error(Marshal.UnsafeAddrOfPinnedArrayElement(buf, 0), buf.Length);
            if (len <= 0) return "未知错误";
            int n = Math.Min(len - 1, buf.Length - 1);
            return System.Text.Encoding.UTF8.GetString(buf, 0, n);
        }
    }
}