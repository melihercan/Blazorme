using System.Reflection;

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
            .ThenByDescending(File.GetLastWriteTimeUtc)
            .ToList();

        return candidates.Count > 0
            ? candidates[0]
            : throw new FileNotFoundException($"No build of {assemblyName}.dll found under '{bin}'.");
    }

    /// <summary>Every directory a metadata resolver should search for dependency assemblies.</summary>
    internal static IEnumerable<string> ProbingDirectories()
    {
        yield return Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        yield return Path.GetDirectoryName(typeof(TestAssemblies).Assembly.Location)!;

        foreach (var name in Names)
        {
            yield return Path.GetDirectoryName(LocateDll(name))!;
        }
    }
}
