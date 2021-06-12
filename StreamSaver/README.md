# Blazorme.StreamSave
JSInterop of [StreamSaver](https://github.com/jimmywarting/StreamSaver.js) package for Blazor WASM. 
With this package, you can download files by writing incoming data chunks directly to the file instead of accumulating them in memory. 
This will prevent memory overflows for very large files.

It provides a very simple API:
```cs
    public interface IStreamSaver
    {
        Task<Stream> CreateWritableFileStreamAsync(string fileName);
    }
```
With this call, a C# stream will be returned to the caller. The caller can write(append) data chunks to the file by calling the `Stream` interface `WriteAsync` call. 
After all chunks are written, `Close` call on the stream will complete the file download.

## Installation
* Install NuGet package:
```
  Install-Package Blazorme.StreamSaver
```
## Using

In `_Imports.razor` add:
```
  @using Blazorme
```
In a `razor` file add:
```cs
  [Inject]
  IStreamSaver StreamSaver { get; set; }
```
From code behind, inject the interface to your constructor:
```cs
  using Blazorme;
```
```cs
  Xxx(IStreamSaver streamSaver, ...)
```

## JS references

Add the following lines to your `index.html` file:
```html

<!-- inside body section -->
    <script src="_content/Blazorme.StreamSaver/polyfill.min.js"></script>
    <script src="_content/Blazorme.StreamSaver/StreamSaver.min.js"></script>
```
## Service addition

Add the following to `Program.cs`: 
```cs
  using Blazorme;
  
  Main
  {
      ...
      builder.Services.AddStreamSaver();
      ...
  }
```

