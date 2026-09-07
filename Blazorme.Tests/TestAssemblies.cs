using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Blazorme.Tests;

/// <summary>
/// Locates the built library assemblies on disk so the public-API baseline can cover every
/// library, including the ones a net10.0 test project cannot reference at all
/// (StreamSaver and FFmpeg target net5.0 only).
/// </summary>
internal static class TestAssemblies
{
    /// <summary>Assembly simple names, in the order they appear in the approved baseline.</summary>
    internal static readonly string[] Names =
    [
        "Blazorme.Diff",
        "Blazorme.Split",
        "Blazorme.StreamSaver",
        "Blazorme.TestHost",
        "Blazorme.FFmpeg",
    ];

    internal static string RepositoryRoot { get; } = FindRepositoryRoot();

    /// <summary>The build configuration this test run was compiled in, e.g. "Debug".</summary>
    internal static string Configuration { get; } =
        typeof(TestAssemblies).Assembly
            .GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "Debug";

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(Path.GetDirectoryName(typeof(TestAssemblies).Assembly.Location)!);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Blazorme.sln")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new InvalidOperationException("Could not locate Blazorme.sln above the test assembly.");
        }

        return dir.FullName;
    }

    /// <summary>
    /// The newest build of <paramref name="assemblyName"/> found under its project's bin folder.
    /// Deliberately framework-agnostic: the libraries multi-target today and will single-target
    /// net10.0 after the migration, and the baseline must survive that move.
    /// </summary>
    internal static string LocateDll(string assemblyName)
    {
        var project = assemblyName["Blazorme.".Length..];
        var bin = Path.Combine(RepositoryRoot, project, "bin");

        if (!Directory.Exists(bin))
        {
            throw new FileNotFoundException(
                $"'{bin}' does not exist. Build the whole solution before running the API baseline: dotnet build Blazorme.sln");
        }

        var configuration = Path.DirectorySeparatorChar + Configuration + Path.DirectorySeparatorChar;

        var candidates = Directory.GetFiles(bin, assemblyName + ".dll", SearchOption.AllDirectories)
            .Where(p => !p.Contains(Path.DirectorySeparatorChar + "ref" + Path.DirectorySeparatorChar))
            // Match the test run's own configuration first, so a stale Release build cannot
            // shadow a fresh Debug one just by having a newer timestamp.
            .OrderByDescending(p => p.Contains(configuration, StringComparison.OrdinalIgnoreCase))
            // Then the newest target framework. TestHost multi-targets, and stale folders from
            // earlier target frameworks linger in bin, so timestamp alone is not deterministic.
            .ThenByDescending(FrameworkRank)
            .ThenByDescending(File.GetLastWriteTimeUtc)
            .ToList();

        return candidates.Count > 0
            ? candidates[0]
            : throw new FileNotFoundException($"No build of {assemblyName}.dll found under '{bin}'.");
    }

    /// <summary>
    /// Orders build output folders by target framework, newest first: net10.0 beats net8.0 beats
    /// net5.0, and anything unrecognised (netstandard2.1, say) sorts last.
    /// </summary>
    private static int FrameworkRank(string path)
    {
        var folder = Path.GetFileName(Path.GetDirectoryName(path)) ?? string.Empty;
        var match = System.Text.RegularExpressions.Regex.Match(folder, @"^net(\d+)\.");
        return match.Success ? int.Parse(match.Groups[1].Value) : -1;
    }

    /// <summary>The target frameworks a project currently declares, newest first.</summary>
    internal static IReadOnlyList<string> TargetFrameworks(string projectFile)
    {
        var xml = File.ReadAllText(Path.Combine(RepositoryRoot, projectFile));
        var match = System.Text.RegularExpressions.Regex.Match(
            xml, @"<TargetFrameworks?>([^<]+)</TargetFrameworks?>");

        return match.Success
            ? match.Groups[1].Value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : throw new InvalidOperationException($"No TargetFramework(s) in '{projectFile}'.");
    }

    /// <summary>The built assembly for one specific target framework.</summary>
    internal static string LocateDll(string assemblyName, string targetFramework)
    {
        var project = assemblyName["Blazorme.".Length..];
        var path = Path.Combine(RepositoryRoot, project, "bin", Configuration, targetFramework,
            assemblyName + ".dll");

        return File.Exists(path)
            ? path
            : throw new FileNotFoundException(
                $"No {targetFramework} build of {assemblyName}.dll. Build the solution first.", path);
    }

    /// <summary>
    /// The version of <paramref name="simpleName"/> that <paramref name="dllPath"/> was compiled
    /// against, read straight out of the assembly-reference table. Nothing is loaded for
    /// execution, so this works for any target framework.
    /// </summary>
    internal static Version? ReferencedAssemblyVersion(string dllPath, string simpleName)
    {
        using var stream = File.OpenRead(dllPath);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();

        foreach (var handle in metadata.AssemblyReferences)
        {
            var reference = metadata.GetAssemblyReference(handle);
            if (metadata.GetString(reference.Name) == simpleName)
            {
                return reference.Version;
            }
        }

        return null;
    }

    /// <summary>Every directory a metadata resolver should search for dependency assemblies.</summary>
    internal static IEnumerable<string> ProbingDirectories()
    {
        yield return Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        yield return Path.GetDirectoryName(typeof(TestAssemblies).Assembly.Location)!;

        // Every framework folder of every library, so a multi-targeted assembly's references
        // resolve whichever framework it was built for.
        foreach (var name in Names)
        {
            var project = name["Blazorme.".Length..];
            var bin = Path.Combine(RepositoryRoot, project, "bin");
            if (!Directory.Exists(bin)) continue;

            foreach (var dll in Directory.GetFiles(bin, name + ".dll", SearchOption.AllDirectories))
            {
                yield return Path.GetDirectoryName(dll)!;
            }
        }
    }
}
