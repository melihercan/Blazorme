using System.Reflection;
using FluentAssertions;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace Blazorme.Tests;

/// <summary>
/// Characterization and regression tests for Blazorme.TestHost.
///
/// Phase 0 pinned this library as BROKEN on .NET 10: it is Steve Sanderson's
/// BlazorUnitTestingPrototype, and its shipped IL read <c>RenderTreeFrame</c> members that were
/// public fields in ASP.NET Core 3.1 and became properties in .NET 5+, so the runtime threw
/// <see cref="MissingFieldException"/>.
///
/// Phase 1 fixed it with no source change at all. The C# in this library was always written as
/// <c>frame.FrameType</c>, which binds to a property exactly as well as to a field — the break
/// was purely in the stale compiled reference. Retargeting to net10.0 and deduplicating the
/// PackageReference items recompiled it against Components 10.0, and the library works.
///
/// The two tests below were DEFECT pins in Phase 0 and are rewritten here rather than deleted,
/// so the fix is visible in the diff.
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
    public void FIXED_AddComponent_renders_a_component_and_exposes_its_markup()
    {
        // Was: DEFECT_AddComponent_throws_because_RenderTreeFrame_changed_shape_after_3_1,
        // which asserted MissingFieldException. Fixed in Phase 1 by the retarget.
        var diff = Substitute.For<IDiff>();
        diff.GetHtmlAsync(default!, default!, default!, default!, default, default)
            .ReturnsForAnyArgs(Task.FromResult("<ins>added</ins>"));

        var host = new TestHost();
        host.AddService<IDiff, IDiff>(diff);

        var rendered = host.AddComponent<Diff>();

        rendered.Instance.Should().NotBeNull();
        rendered.GetMarkup().Should().Contain("<ins>added</ins>");
    }

    [Fact]
    public void FIXED_the_Fizzler_selector_layer_works_over_the_rendered_markup()
    {
        // GetMarkup/FindAll go through Htmlizer and HtmlAgilityPack, which is the part that
        // walked RenderTreeFrame most heavily. Exercising a selector proves the whole path.
        var diff = Substitute.For<IDiff>();
        diff.GetHtmlAsync(default!, default!, default!, default!, default, default)
            .ReturnsForAnyArgs(Task.FromResult("<p class=\"hit\">found me</p>"));

        var host = new TestHost();
        host.AddService<IDiff, IDiff>(diff);

        var rendered = host.AddComponent<Diff>();

        rendered.Find("p.hit").Should().NotBeNull();
        rendered.Find("p.hit").InnerText.Should().Be("found me");
        rendered.FindAll("p").Should().HaveCount(1);
    }

    [Fact]
    public void FIXED_the_build_binds_to_the_net10_Components_assembly()
    {
        // Was: DEFECT_the_net5_build_was_actually_compiled_against_Components_3_1.
        // TestHost.csproj used to declare Microsoft.AspNetCore.Components 3.1.10 unconditionally
        // AND 5.0.0 for net5.0. Duplicate PackageReference items do not merge — the first won and
        // the conditional one was silently discarded (NU1504), so the package's advertised
        // ".NET5 support" shipped IL bound to 3.1. Phase 1 removed the duplicate.
        var componentsReference = typeof(TestHost).Assembly
            .GetReferencedAssemblies()
            .Single(a => a.Name == "Microsoft.AspNetCore.Components");

        componentsReference.Version.Should().NotBeNull();
        componentsReference.Version!.Major.Should().Be(10);
    }

    [Fact]
    public void RenderTreeFrame_members_are_properties_on_this_runtime()
    {
        // Kept from Phase 0: this is the shape change that broke the old binary, and the reason
        // the fix had to be a recompile rather than a source edit.
        var frameType = typeof(Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrame);

        frameType.GetField("FrameType", BindingFlags.Public | BindingFlags.Instance)
            .Should().BeNull("it was a public field in 3.1 and is no longer one");
        frameType.GetProperty("FrameType", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull("it is a property from .NET 5 onwards");
    }
}
