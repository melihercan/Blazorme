using FluentAssertions;
using Xunit;

namespace Blazorme.Tests;

/// <summary>
/// Guards for Blazorme.TestHost, which multi-targets net8.0 and net10.0 so that projects on
/// .NET 8 have a working version. .NET 5 to .NET 9 cannot consume a net10.0-only package, and the
/// only version they could otherwise resolve is 1.0.0, which crashes at runtime.
///
/// These tests exist because of exactly how 1.0.0 broke. It declared
/// Microsoft.AspNetCore.Components 3.1.10 unconditionally AND 5.0.0 for net5.0. Duplicate
/// PackageReference items do not merge — the first wins and the conditional one is silently
/// discarded (NU1504) — so BOTH shipped assets were bound to ASP.NET Core 3.1 while the package
/// advertised .NET 5 support. Nothing caught it for five years, because nothing ever asserted what
/// the shipped assets were actually bound to.
/// </summary>
public class MultiTargetingTests
{
    private const string Assembly = "Blazorme.TestHost";
    private const string Project = "TestHost/TestHost.csproj";

    public static TheoryData<string> ShippedFrameworks()
    {
        var data = new TheoryData<string>();
        foreach (var tfm in TestAssemblies.TargetFrameworks(Project))
        {
            data.Add(tfm);
        }

        return data;
    }

    [Fact]
    public void TestHost_ships_more_than_one_framework()
    {
        // If this ever drops to one, .NET 8 consumers silently fall back to the broken 1.0.0.
        TestAssemblies.TargetFrameworks(Project).Should().Contain(["net8.0", "net10.0"]);
    }

    [Theory]
    [MemberData(nameof(ShippedFrameworks))]
    public void Each_shipped_framework_binds_to_its_own_Components_major(string targetFramework)
    {
        // The 1.0.0 regression, asserted directly: what does the built asset actually reference?
        var expectedMajor = int.Parse(targetFramework["net".Length..].Split('.')[0]);

        var components = TestAssemblies.ReferencedAssemblyVersion(
            TestAssemblies.LocateDll(Assembly, targetFramework),
            "Microsoft.AspNetCore.Components");

        components.Should().NotBeNull("the asset must reference ASP.NET Core Components at all");
        components!.Major.Should().Be(expectedMajor,
            "the {0} asset must be compiled against ASP.NET Core {1}, not whatever a duplicate "
            + "PackageReference happened to resolve first", targetFramework, expectedMajor);
    }

    [Fact]
    public void Every_framework_exposes_the_same_public_surface()
    {
        // A consumer moving between .NET 8 and .NET 10 must see an identical API.
        var dumps = TestAssemblies.TargetFrameworks(Project)
            .Select(tfm => (tfm, api: PublicApiDumper.Dump(
                [(Assembly, TestAssemblies.LocateDll(Assembly, tfm))])))
            .ToList();

        foreach (var (tfm, api) in dumps.Skip(1))
        {
            api.Should().Be(dumps[0].api,
                "the {0} surface must match {1}", tfm, dumps[0].tfm);
        }
    }
}
