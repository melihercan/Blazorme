using Microsoft.JSInterop;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme.Bindings.Blazor.Interops
{
    public class StreamSaverJsObjectRef
    {
        [JsonPropertyName("__streamSaverJsObjectRefId")] 
        public int StreamSaverJsObjectRefId { get; set; }
    }
}