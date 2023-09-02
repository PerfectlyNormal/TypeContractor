using System.Reflection;
using System.Runtime.InteropServices;
using TypeContractor.Logger;

namespace TypeContractor.Tool;

internal static class ReflectionContextHelper
{
    internal static MetadataLoadContext GetMetadataContext(string packPath, string assemblyPath, ILog logger)
    {
        return new(GetResolver(packPath, assemblyPath, logger));
    }

    internal static PathAssemblyResolver GetResolver(string packPath, string assemblyPath, ILog logger)
    {
        // Get the array of runtime assemblies.
        logger.LogDebug($"Adding {RuntimeEnvironment.GetRuntimeDirectory()} to list of assemblies");
        var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
        var runtimeFiles = runtimeAssemblies.Select(ass => Path.GetFileName(ass)).ToList();

        // Get the .NET Core assemblies
        var netcoreDirectory = GetNetCorePack(packPath, "Microsoft.NETCore.App.Ref")
            ?? throw new FileNotFoundException($"Unable to find Microsoft.NETCore.App.Ref v6.0.x references. Searched in {packPath}.");
        logger.LogDebug($"Adding {netcoreDirectory} to list of assemblies");
        var netcoreAssemblies = Directory
            .GetFiles(netcoreDirectory, "*.dll")
            .Where(ass => !runtimeFiles.Contains(Path.GetFileName(ass)));

        // Get the ASP.NET Core assemblies
        var aspnetDirectory = GetNetCorePack(packPath, "Microsoft.AspNetCore.App.Ref")
            ?? throw new FileNotFoundException($"Unable to find Microsoft.AspNetCore.App.Ref v6.0.x references. Searched in {packPath}.");
        logger.LogDebug($"Adding {aspnetDirectory} to list of assemblies"); 
        var aspnetAssemblies = Directory.GetFiles(aspnetDirectory, "*.dll");

        // Get the app-specific assemblies
        logger.LogDebug($"Adding {Path.GetDirectoryName(assemblyPath)} to list of assemblies");
        var appAssemblies = Directory.GetFiles(Path.GetDirectoryName(assemblyPath)!, "*.dll");

        // Create the list of assembly paths consisting of runtime assemblies and the inspected assembly.
        var paths = runtimeAssemblies.Concat(netcoreAssemblies).Concat(aspnetAssemblies).Concat(appAssemblies);

        return new PathAssemblyResolver(paths);
    }

    internal static string? GetNetCorePack(string packPath, string packName)
    {
        var packPrefix = @$"{packPath}\{packName}";
        if (!Directory.Exists(packPrefix))
            return null;

        var availablePacks = Directory.EnumerateDirectories(packPrefix, "6.0.*");
        var packDirectory = availablePacks.OrderByDescending(x => x).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(packDirectory))
            return null;

        packDirectory = Path.Combine(packDirectory, "ref", "net6.0");
        if (!Directory.Exists(packDirectory))
            return null;

        return packDirectory;
    }
}
