using Blazorme;
using BlazormeStreamSaver;
using DemoApp.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Pages
{
    public partial class StreamSaverDemo
    {
        private StreamSaverModel _streamSaverModel = new();

        private StringBuilder _stringBuilder = new();
        private const string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        private Stream _writableFileStream; 

        [Inject]
        private IStreamSaver StreamSaver { get; set; }

        private void ClearFilename()
        {
            _streamSaverModel.Filename = string.Empty;
        }

        private void AddText()
        {
            _stringBuilder.AppendLine(_streamSaverModel.Text); 
        }

        private void ClearText()
        {
            _streamSaverModel.Text = string.Empty;
        }

        private void AddLoremIpsum()
        {
            for(int i=0; i<_streamSaverModel.NumLoremIpsum; i++)
                _stringBuilder.AppendLine(LoremIpsum);
        }

        private async Task AppendAsync()
        {
            _writableFileStream ??= await StreamSaver.CreateWritableFileStreamAsync(_streamSaverModel.Filename);
            await _writableFileStream.WriteAsync(Encoding.UTF8.GetBytes(_stringBuilder.ToString()));

            _stringBuilder.Clear();
            _streamSaverModel.FilenameDisabled = true;
            ClearText();

            await Task.Delay(1);
        }

        private async Task CloseAsync()
        {
            await ResetAsync();
        }

        private async Task ResetAsync()
        {
            await _writableFileStream?.DisposeAsync().AsTask();
            _writableFileStream = null;
            _stringBuilder.Clear();
            ClearFilename();
            _streamSaverModel.FilenameDisabled = false;
            ClearText();
        }
    }
}
