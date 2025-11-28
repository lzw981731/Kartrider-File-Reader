using System.Reflection;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(eP.Command.CommandHotReloadHandler))]
namespace eP.Command;

internal static class CommandHotReloadHandler
{
    public static void UpdateApplication(Type[]? types)
    {
        
    }
}