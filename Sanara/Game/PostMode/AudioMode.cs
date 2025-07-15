﻿using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Compatibility;
using System.Diagnostics;

namespace Sanara.Game.PostMode
{
    public class AudioMode : IPostMode
    {
        public Task PostAsync(IServiceProvider provider, CommonMessageChannel chan, string text, AGame sender)
        {
            var quizzAudio = (IAudioGame)sender;
            quizzAudio.GetNewProcess();
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel fatal -i - -af volume=0.2 -f s16le -ac 2 -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            });
            quizzAudio.SetCurrentProcess(process);
            _ = Task.Run(async () =>
            {
                var stream = await provider.GetRequiredService<HttpClient>().GetStreamAsync(text);
                await stream.CopyToAsync(process.StandardInput.BaseStream);
                process.StandardInput.Close();
            });
            _ = Task.Run(async () =>
            {
                using Stream output = process.StandardOutput.BaseStream;
                try
                {
                    await output.CopyToAsync(quizzAudio.GetAudioOutStream());
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            });
            return Task.CompletedTask;
        }
    }
}
