using FluentAssertions;
using Xunit;

namespace Blazorme.Tests;

/// <summary>
/// The additive-only guarantee, enforced mechanically.
///
/// Every Blazorme package is on nuget.org and real applications depend on it, so no public member
/// may be renamed, removed, or have its type changed by the modernization. New capability is added
/// *beside* the old surface, never in place of it.
///
/// This baseline was captured from the 2021 code, before any change, and is the reference the whole
/// migration is diffed against. When a change is intentional, review the diff and copy
/// <c>PublicApi.received.txt</c> from the test output directory over <c>PublicApi.approved.txt</c>.
/// Never weaken the assertion.
/// </summary>
public class PublicApiSurfaceTests
{
    [Fact]
    public void Public_surface_matches_the_approved_baseline()
    {
        var received = PublicApiDumper.Dump(TestAssemblies.Names);

        var receivedPath = Path.Combine(AppContext.BaseDirectory, "PublicApi.received.txt");
        File.WriteAllText(receivedPath, received);

        var approvedPath = Path.Combine(TestAssemblies.RepositoryRoot, "Blazorme.Tests", "PublicApi.approved.txt");
        File.Exists(approvedPath).Should().BeTrue(
            "the baseline must be checked in; generate it from '{0}'", receivedPath);

        var approved = File.ReadAllText(approvedPath);

        Normalize(received).Should().Be(
            Normalize(approved),
            "the public API is additive-only. If this change is intentional, review the diff and copy "
            + "'{0}' over 'Blazorme.Tests/PublicApi.approved.txt' in the same commit.",
            receivedPath);
    }

    private static string Normalize(string text) =>
        text.Replace("\r\n", "\n").TrimEnd();
}
