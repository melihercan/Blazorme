# Blazorme.Split
Split component library for Blazor apps. The library provides resizeable split views (panes).

Use `DemoApp` as a reference for your implementation.

![alt text](https://github.com/melihercan/Blazorme/blob/master/images/Split.gif)
## Implementation
Currently the library interops by using the `split.js` package: [GitHub](https://github.com/nathancahill/split), [jsdelivr](https://www.jsdelivr.com/package/npm/split.js) 
## Installation
* Install NuGet package:
```
  Install-Package Blazorme.Split
```
* Using:

In `_Imports.razor` add:
```
  @using Blazorme
```
* JS reference:

Add the following line to your `index.html` (WebAsembly) or `_Host.cshtml` (Server) files:
```html
    <!-- inside body section -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/split.js/1.6.0/split.min.js"></script>
```
## Usage
In your Blazor page add the top level `Split` entry and `SplitPane` to form the split view. Here is a simple example:
```html
<div class="container-fluid border border-primary" style="width: 100%; height: 600px">
    <Split>
        <SplitPane SizeInPercentage="20" MinSize="0">
            One
        </SplitPane>
        <SplitPane SizeInPercentage="50" MinSize="0">
            Two
        </SplitPane>
        <SplitPane SizeInPercentage="30" MinSize="0">
            Three
        </SplitPane>
    </Split>
</div>
```


