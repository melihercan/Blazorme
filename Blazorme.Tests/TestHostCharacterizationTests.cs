using System.Reflection;
using FluentAssertions;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace Blazorme.Tests;

/// <summary>
/// Characterization tests for Blazorme.TestHost.
///
/// The headline fact: TestHost DOES NOT WORK on .NET 10 today. It is Steve Sanderson's
/// BlazorUnitTestingPrototype, and it reads <c>RenderTreeFrame</c> members that were public
/// fields in ASP.NET Core 3.1 and became properties in .NET 5+. The 3.1-compiled IL does
/// <c>ldfld</c>, so the runtime throws <see cref="MissingFieldException"/>.
///
/// These pin that broken state so the Phase 3 migration is visible in the diff. When TestHost
/// is migrated, REWRITE these tests rather than deleting them.
/// </summary>
public class TestHostCharacterizationTests
{
    [Fact]
    public void The_parts_that_do_not_touch_the_render_tree_still_work()
    {
        var host = new TestHost();
        var diff = Substitute.For<IDiff>();

        host.AddService<IDiff, IDiff>(diff);

        host.Services.Should().NotBeNull();
        host.Services.GetService(typeof(IDiff)).Should().BeSameAs(diff);
    }

    [Fact]
    public void Configuring_services_after_the_renderer_starts_is_rejected()
    {
        var host = new TestHost();
        host.AddService<IDiff, IDiff>(Substitute.For<IDiff>());

        // Touching the renderer is what "starts operation"; WaitForNextRender does that.
        var started = () => host.WaitForNextRender(() => { });
        started.Should().Throw<TimeoutException>("no render is triggered by an empty action");

        var act = () => host.AddService<IJSRuntime, IJSRuntime>(Substitute.For<IJSRuntime>());
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot configure services after the host has started operation");
    }

    [Fact]
    public void DEFECT_AddComponent_throws_because_RenderTreeFrame_changed_shape_after_3_1()
    {
        var host = new TestHost();
        host.AddService<IDiff, IDiff>(Substitute.For<IDiff>());

        var act = () => host.AddComponent<Diff>();

        act.Should().Throw<MissingFieldException>()
            .WithMessage("*Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrame*");
    }

    [Fact]
    public void DEFECT_the_net5_build_was_actually_compiled_against_Components_3_1()
    {
        // TestHost.csproj declares Microsoft.AspNetCore.Components 3.1.10 unconditionally AND
        // 5.0.0 for net5.0. Duplicate PackageReference items do not merge — the first wins and
        // the conditional one is silently discarded (NU1504). So the package's advertised
        // ".NET5 support" ships IL bound to 3.1, which is the direct cause of the test above.
        var componentsReference = typeof(TestHost).Assembly
            .GetReferencedAssemblies()
            .Single(a => a.Name == "Microsoft.AspNetCore.Components");

        componentsReference.Version.Should().Be(new Version(3, 1, 10, 0));
    }

    [Fact]
    public void Evidence_RenderTreeFrame_members_are_properties_on_this_runtime()
    {
        var frameType = typeof(Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrame);

        frameType.GetField("FrameType", BindingFlags.Public | BindingFlags.Instance)
            .Should().BeNull("it was a public field in 3.1 and is no longer one");
        frameType.GetProperty("FrameType", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull("it is a property from .NET 5 onwards");
    }
}
