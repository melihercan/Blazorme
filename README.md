# Blazorme.Diff
Diff component library for Blazor apps. 
## Introduction
The idea is to render diff of the two input strings in different output display formats. Output is always in HTML and currently 3 output display formats are provided:
* Inline: Inlined format. Intented for HTML inputs although plain text can also be used.
* Row: Line by line format for text inputs.
* Column: Side by side format for text inputs.

Please use `TestApp` as a reference for your implementation. 

![alt text](https://github.com/melihercan/gifs/blob/master/Diff.gif)
## Third Party Packages

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


## Component Usage
In your Blazor page add the Diff entry for the desired output format as shown below:
### For Inline format
No need to specify titles for inline as they are are not used for this format.
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
        SecondTitle="second title text displayed on the top of the rendered able"
        OutputFormat=@DiffOutputFormat.Row />
        or
        OutputFormat=@DiffOutputFormat.Column />
```
## API Usage
The Diff component can also be used as a library to get `diff` or `html diff` output strings with API calls from code behind. The API interface is defined in `IDiff.cs` as follows:
```cs
        public Task<string> GetAsync(string firstInput, string secondInput, 
            string firstTitle = DiffInputTitle.First, string secondTitle = DiffInputTitle.Second);

        public Task<string> GetHtmlAsync(string firstInput, string secondInput,
            string firstTitle = DiffInputTitle.First, string secondTitle = DiffInputTitle.Second,
            DiffOutputFormat outputFormat = DiffOutputFormat.Inline);
```
### Examples
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
                DiffOutputFormat.Row);
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
