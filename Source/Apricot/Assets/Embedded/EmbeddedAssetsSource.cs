using System.Reflection;
using Apricot.Assets.Models;
using Apricot.Assets.Sources;
using Apricot.Utils;

namespace Apricot.Assets.Embedded;

/// <summary>
/// Loads embedded assets by their logical names. As a first segment in path excepts assembly name. <br/>
/// <br/>
/// Not to make readable all embedded files in every assebly it expects that all assets are stored with Assets/ at the
/// beginning.
/// </summary>
public class EmbeddedAssetsSource : IAssetsSource
{
    public static string Scheme => "embedded";
    
    string IAssetsSource.Scheme => Scheme;

    public IEnumerable<string> ListAssetsPaths(string localPath, ListAssetsType listType)
    {
        var (assemblyName, logicalName) = ParseLocalPath(localPath);

        if (assemblyName == string.Empty) // all assemblies
        {
            if (listType != ListAssetsType.Recursive) yield break;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var path in ListAssetsPaths(assembly, "", listType))
                {
                    yield return path;
                }
            }
        }
        else
        {
            var assembly = FindAssembly(assemblyName);

            if (assembly is null)
            {
                throw new FileNotFoundException($"Could not find assembly {assemblyName}");
            }

            foreach (var assetsPath in ListAssetsPaths(assembly, logicalName, listType))
            {
                yield return assetsPath;
            }
        }
    }

    /// <summary>
    /// Opens stream to asset located at path.
    /// </summary>
    /// <param name="localPath">Path to asset (without scheme).</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public Stream Open(string localPath)
    {
        var (assemblyName, logicalName) = ParseLocalPath(localPath);
        var assembly = FindAssembly(assemblyName);

        if (assembly is null)
        {
            throw new AssetNotFoundException($"Could not find assembly {assemblyName}");
        }

        var fileStream = assembly.GetManifestResourceStream(logicalName);

        if (fileStream is null)
        {
            throw new AssetNotFoundException(
                $"Could not locate resource with logical name of {logicalName} in {assembly.FullName}"
            );
        }

        return fileStream;
    }

    private static IEnumerable<string> ListAssetsPaths(Assembly assembly, string prefix, ListAssetsType listType)
    {
        var filePaths = assembly.GetManifestResourceNames();
        var actualPrefix = PathUtils.NormalizePath($"Assets/{prefix}");
        var substringIndex = "Assets/".Length;
        var assemblyName = assembly.GetName().Name;

        foreach (var path in filePaths)
        {
            var purePath = PathUtils.NormalizePath(path);

            if (listType == ListAssetsType.Recursive)
            {
                if (purePath.StartsWith(actualPrefix))
                {
                    yield return $"{assemblyName}/{path[substringIndex..]}";
                }
            }
            else
            {
                var parentDir = PathUtils.GetParent(purePath);
                if (actualPrefix == parentDir)
                {
                    yield return $"{assemblyName}/{path[substringIndex..]}";
                }
            }
        }
    }

    private static (string AssemblyName, string LogicalName) ParseLocalPath(string localPath)
    {
        var sepIndex = localPath.IndexOf("/", StringComparison.InvariantCultureIgnoreCase);

        return sepIndex < 0
            ? (localPath, string.Empty)
            : (localPath[..sepIndex], localPath[(sepIndex + 1)..]);
    }

    private static Assembly? FindAssembly(string assemblyName) => AppDomain
        .CurrentDomain
        .GetAssemblies()
        .FirstOrDefault(a => a.GetName().Name == assemblyName);
}
