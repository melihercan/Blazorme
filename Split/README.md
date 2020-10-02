# Blazorme.Split
Split component library for Blazor apps. The library provides resizeable split views (panes).

Use `DemoApp` as a reference for your implementation.

![alt text](https://github.com/melihercan/Blazorme/blob/master/doc/Split.gif)
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
In your Blazor page, add the top level `Split` entry and `SplitPane` sub-entires to form the split view. Here is a simple example:
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
## Parameters
``` cs
    public enum SplitDirection
    {
        Horizontal,
        Vertical
    }
```
``` cs
    public enum SplitGutterAlign
    {
        Center,
        Start,
        End,
    }
```
### Split
Parameter | Type | Default | Description
--- | --- | --- | ---
DefaultMinSize | int | 100 | Minimum size of each pane in pixels. It will be used if min size is not specified in `SplitPane`.
ExpandToMin | bool | false | Grow initial sizes to min size.
GutterSize | int | 10 | Gutter size in pixels.
GutterAlign | SplitGutterAlign | SplitGutterAlign.Center | Gutter alignment between elements. 
GutterColor | string | "#cfcfcf" | Gutter color in HTML hex string.
SnapOffset | int | 30 | Snap to min size offset in pixels.
DragInterval | int | 1 | Number of pixels to drag.
Direction | SplitDirection | SplitDirection.Horizontal | Direction to split, horizontal or vertical.
Cursor | string | "col-resize" or "row-resize" depending on direction | [Cursor](https://www.w3schools.com/cssref/pr_class_cursor.asp) to display while dragging.
### SplitPane
Parameter | Type | Default | Description
--- | --- | --- | ---
SizeInPercentage | int | 0 | Pane size in percentage.
MinSize | int? | null | Minimum size of the pane in pixels.
## Example
From demo app:
``` html
<div class="container-fluid border border-primary" style="width: 100%; height: 600px">
    <Split GutterSize="5">
        <SplitPane SizeInPercentage="20" MinSize="0">
            Lorem ipsum dolor sit amet, consectetur adipiscing elit. In vel ullamcorper ipsum, at blandit leo. Sed egestas est tellus, nec rutrum leo ultricies vitae. Praesent scelerisque libero in lacus gravida, a ultrices tortor rutrum. Donec et justo nibh. Nulla congue volutpat sapien, eu pretium sapien porttitor id. Vivamus sit amet aliquet libero. Vestibulum sit amet ex pharetra, dapibus enim nec, ullamcorper metus. Integer pellentesque aliquam aliquet.
        </SplitPane>
        <SplitPane SizeInPercentage="50" MinSize="0">
            <Split Direction=@SplitDirection.Vertical GutterSize="5">
                <SplitPane MinSize="0">
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit. In vel ullamcorper ipsum, at blandit leo. Sed egestas est tellus, nec rutrum leo ultricies vitae. Praesent scelerisque libero in lacus gravida, a ultrices tortor rutrum. Donec et justo nibh. Nulla congue volutpat sapien, eu pretium sapien porttitor id. Vivamus sit amet aliquet libero. Vestibulum sit amet ex pharetra, dapibus enim nec, ullamcorper metus. Integer pellentesque aliquam aliquet.
                </SplitPane>
                <SplitPane MinSize="0">
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit. In vel ullamcorper ipsum, at blandit leo. Sed egestas est tellus, nec rutrum leo ultricies vitae. Praesent scelerisque libero in lacus gravida, a ultrices tortor rutrum. Donec et justo nibh. Nulla congue volutpat sapien, eu pretium sapien porttitor id. Vivamus sit amet aliquet libero. Vestibulum sit amet ex pharetra, dapibus enim nec, ullamcorper metus. Integer pellentesque aliquam aliquet.
                </SplitPane>
            </Split>
        </SplitPane>
        <SplitPane SizeInPercentage="30" MinSize="0">
            Lorem ipsum dolor sit amet, consectetur adipiscing elit. In vel ullamcorper ipsum, at blandit leo. Sed egestas est tellus, nec rutrum leo ultricies vitae. Praesent scelerisque libero in lacus gravida, a ultrices tortor rutrum. Donec et justo nibh. Nulla congue volutpat sapien, eu pretium sapien porttitor id. Vivamus sit amet aliquet libero. Vestibulum sit amet ex pharetra, dapibus enim nec, ullamcorper metus. Integer pellentesque aliquam aliquet.
        </SplitPane>
    </Split>
</div>
````
