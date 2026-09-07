# Blazorme.TestHost
Support library for Blazor component unit testing. The source code is taken from Steve Sanderson's [BlazorUnitTestingPrototype](https://github.com/SteveSandersonMS/BlazorUnitTestingPrototype) project. See his [blog post](https://blog.stevensanderson.com/2019/08/29/blazor-unit-testing-prototype/) for more details. His project is available only as source code; this package makes it usable as a dependency.

## Consider bUnit first
[bUnit](https://github.com/bUnit-dev/bUnit) grew out of the same prototype and is the actively maintained library for this job. For a new test suite, prefer it. This package exists for projects already built on it.

## Requirements
Requires **.NET 10**. Earlier versions of this package targeted `netstandard2.1` and `net5.0`; those are no longer supported.

> **Version 1.0.0 did not work on .NET 5 or later**, despite advertising that it did. Duplicate
> `PackageReference` items meant its `net5.0` build was compiled against ASP.NET Core 3.1, whose
> `RenderTreeFrame` exposed public fields that became properties in .NET 5. Rendering a component
> threw `MissingFieldException` at runtime. Fixed in this release.

## Installation
```
  Install-Package Blazorme.TestHost
```

## Usage
```cs
  using Blazorme;

  var host = new TestHost();
  host.AddService<IMyService, IMyService>(myServiceStub);

  var component = host.AddComponent<MyComponent>();

  component.Find("button.submit")?.Click();
  component.GetMarkup().Should().Contain("Saved");
```

`Find` returns `null` when the selector matches nothing; `FindAll` returns every match. Both accept
CSS selectors, resolved by [Fizzler](https://github.com/atifaziz/Fizzler) over the rendered markup.
