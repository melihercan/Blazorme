# Blazorme.Diff
Diff component library for Blazor apps.
The library will render diff of the two input strings in different output display formats. Output is always in HTML and currently 3 output display formats are provided via `OutputFormat` parameter:
* Inline: Inlined format. Intented for HTML inputs although plain text can also be used.
* Row: Line by line format for text inputs.
* Column: Side by side format for text inputs.

For `html diff`, two show difference levels are provided via `Style` parameter:
* Word: Diff will be displayed based on words. This is default.
* Char: Diff will be displayed based on characters.

Use `DemoApp` as a reference for your implementation. 

## Third Party Packages
The following packages have been used:
* htmldiff.net: [GitHub](https://github.com/Rohland/htmldiff.net), [NuGet](https://www.nuget.org/packages/htmldiff.net/) 
* diff: [GitHub](https://github.com/kpdecker/jsdiff), [jsdelivr](https://www.jsdelivr.com/package/npm/diff) (JS interop)
* diff2html: [GitHub](https://github.com/rtfpessoa/diff2html), [jsdelivr](https://www.jsdelivr.com/package/npm/diff2html) (JS interop)  
## Installation
* Install NuGet package:
```
  Install-Package Blazorme.Diff
```
* Using:

In `_Imports.razor` add:
```
  @using Blazorme
```
For API calls from code behine add:
```
  using Blazorme;
```
* Inject

If you like to use API from code behind, inject `IDiff`:
```cs
  [Inject]
  private IDiff DiffApi { get; set; }
```
* JS and CSS references:

Add the following lines to your `index.html` (WebAsembly) or `_Host.cshtml` (Server) files:
```html
    <!-- inside head section -->
    <link rel="stylesheet"
          href="https://cdn.jsdelivr.net/npm/diff2html@3.1.7/bundles/css/diff2html.min.css"
          integrity="sha256-JDuTv80/2mUu1FBkviyttybv8oWSYmqVttPo7VlCXfE="
          crossorigin="anonymous">

    <!-- inside body section -->
    <script src="https://cdn.jsdelivr.net/npm/diff@4.0.2/dist/diff.min.js" 
            integrity="sha256-xofEpXTFTnsOK+GIsjgJc1ZN0kSE3KsTtZJ2GQaWs3I=" 
            crossorigin="anonymous">
    </script>
    <script src="https://cdn.jsdelivr.net/npm/diff2html@3.1.7/bundles/js/diff2html.min.js" 
            integrity="sha256-jaOrunaAmlbF5x0BUXSJbKimY9Urt8yORnOg3A9BDfM=" 
            crossorigin="anonymous">
    </script>
```
* Service addition:

Add the following entires to `Program.cs` in `Main` (WebAssembly) or `Startup.cs` in `Configure` (Server) function. 
```cs
  using Blazorme;
  
  ...
  
  Main
  {
      ...
      builder.Services.AddDiff();
      ...
  }

  Configure
  {
      ...
      services.AddDiff();
      ...
  }
```
## Parameters
``` cs
    public class DiffInputTitle
    {
        public const string First = "First";
        public const string Second = "Second";
    }
```
``` cs
   public enum DiffOutputFormat
    {
        Inline,
        Row,
        Column,
    }
```
``` cs
    public enum DiffStyle
    {
        Word,
        Char
    }
```

Parameter | Type | Default | Description
--- | --- | --- | ---
FirstInput | string | "" | First input string to diff.
SecondInput | string | "" | Second input string to diff.
FirstTitle | string | DiffInputTitle.First | First title.
SecondInput | string | DiffInputTitle.Second | Second title.
OutputFormat | DiffOutputFormat | DiffOutputFormat.Inline | Output format to display.
Style | DiffStyle | DiffStyle.Word | Showing difference levels, word or character. 
## Component Usage
In your Blazor page, add the `Diff` entry for the desired output format as shown below:
### For Inline format
No need to specify titles or style for inline as they are are not used for this format.
Also OutputFormat is Inline by default, hence no need to specify the OutputFormat explicitly.
```html
  <Diff FirstInput="first input text"
        SecondInput="second input text"/>
```
### For Row or Column formats
```html
  <Diff FirstInput="first input text"
        SecondInput="second input text"
        FirstTitle="first title text displayed on the top of the rendered table"
        SecondTitle="second title text displayed on the top of the rendered table"
        OutputFormat=@DiffOutputFormat.Row
        Style=@DiffStyle.Word />
        or
        OutputFormat=@DiffOutputFormat.Column
        Style=@DiffStyle.Char />
```
## API Usage
The Diff component can also be used as a library to get `diff` or `html diff` output strings with API calls from code behind. The API interface is defined in `IDiff.cs` as follows:
```cs
        public Task<string> GetAsync(string firstInput, string secondInput, 
            string firstTitle = DiffInputTitle.First, string secondTitle = DiffInputTitle.Second);

        public Task<string> GetHtmlAsync(string firstInput, string secondInput,
            string firstTitle = DiffInputTitle.First, string secondTitle = DiffInputTitle.Second,
            DiffOutputFormat outputFormat = DiffOutputFormat.Inline,
            DiffStyle style = DiffStyle.Word);
```
### Example
Inject IDiff and call API functions. 
```cs
       [Inject]
        private IDiff DiffApi { get; set; }

        ...
        
        public async Task Sample()
        {
            var firstInput = "Hello World";
            var secondInput = "Hell World!";
            var diff = await DiffApi.GetAsync(firstInput, secondInput);
            var diffHtml = await DiffApi.GetHtmlAsync(firstInput, secondInput, 
                DiffInputTitle.First, DiffInputTitle.Second, 
                DiffOutputFormat.Row, DiffStyle.Word);
        }
```
The above code will produce the following outputs:

diff:
```
===================================================================
--- First
+++ Second
@@ -1,1 +1,1 @@
-Hello World
\ No newline at end of file
+Hell World!
\ No newline at end of file
```
diffHtml:
```html
<div class="d2h-wrapper">
    <div id="d2h-096910" class="d2h-file-wrapper" data-lang="">
    <div class="d2h-file-header">
    <span class="d2h-file-name-wrapper">
    <svg aria-hidden="true" class="d2h-icon" height="16" version="1.1" viewBox="0 0 12 16" width="12">
        <path d="M6 5H2v-1h4v1zM2 8h7v-1H2v1z m0 2h7v-1H2v1z m0 2h7v-1H2v1z m10-7.5v9.5c0 0.55-0.45 1-1 1H1c-0.55 0-1-0.45-1-1V2c0-0.55 0.45-1 1-1h7.5l3.5 3.5z m-1 0.5L8 2H1v12h10V5z"></path>
    </svg>    <span class="d2h-file-name">First → Second</span>
    <span class="d2h-tag d2h-moved d2h-moved-tag">RENAMED</span></span>
    </div>
    <div class="d2h-file-diff">
        <div class="d2h-code-wrapper">
            <table class="d2h-diff-table">
                <tbody class="d2h-diff-tbody">
                <tr>
    <td class="d2h-code-linenumber d2h-info"></td>
    <td class="d2h-info">
        <div class="d2h-code-line d2h-info">@@ -1,1 +1,1 @@</div>
    </td>
</tr><tr>
    <td class="d2h-code-linenumber d2h-del d2h-change">
      <div class="line-num1">1</div>
<div class="line-num2"></div>
    </td>
    <td class="d2h-del d2h-change">
        <div class="d2h-code-line d2h-del d2h-change">
            <span class="d2h-code-line-prefix">-</span>
            <span class="d2h-code-line-ctn"><del>Hello</del> World</span>
        </div>
    </td>
</tr><tr>
    <td class="d2h-code-linenumber d2h-ins d2h-change">
      <div class="line-num1"></div>
<div class="line-num2">1</div>
    </td>
    <td class="d2h-ins d2h-change">
        <div class="d2h-code-line d2h-ins d2h-change">
            <span class="d2h-code-line-prefix">+</span>
            <span class="d2h-code-line-ctn"><ins>Hell</ins> World<ins>!</ins></span>
        </div>
    </td>
</tr>
                </tbody>
            </table>
        </div>
    </div>
</div>
</div>
```
