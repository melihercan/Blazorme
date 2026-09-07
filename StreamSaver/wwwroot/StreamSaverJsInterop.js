// Bridges Blazorme.StreamSaver onto StreamSaver.js.
//
// This is an ES module so it can be imported through IJSObjectReference (JS isolation), which is
// what replaced the .NET 5/6 preview System.Private.Runtime.InteropServices.JavaScript API this
// library originally used. StreamSaver.js itself is a classic script that assigns the global
// `streamSaver`, so it is still loaded via a <script> tag by the host page.

export function createWriter(fileName) {
    return streamSaver.createWriteStream(fileName).getWriter();
}

// `data` arrives as a Uint8Array: .NET marshals byte[] to JS as a Uint8Array directly, which is
// exactly the shape writer.write expects. Earlier attempts at this library passed base64 instead.
export function write(writer, data) {
    return writer.write(data);
}

export function close(writer) {
    return writer.close();
}
