using System.Reflection;

namespace KartLibrary.Resource;

internal class AssemblyResourceManager
{
    public static byte[]? GetEmbeddedResource(string name)
    {
        Assembly? libraryAssembly = System.Reflection.Assembly.GetAssembly(typeof(AssemblyResourceManager));
        if (libraryAssembly is null)
            return null;
        Stream? resourceStream = libraryAssembly.GetManifestResourceStream(name);
        if (resourceStream is null)
            return null;
        byte[] data = new byte[resourceStream.Length];
        resourceStream.Read(data, 0, data.Length);
        resourceStream.Dispose();
        return data;
    }
}