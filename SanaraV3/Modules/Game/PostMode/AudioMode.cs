using Discord;
using Discord.Audio;
using SanaraV3.Modules.Game.Impl;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Game.PostMode
{
    public class AudioMode : IPostMode
    {
        public Task PostAsync(IMessageChannel chan, string text, AGame sender)
        {
            var quizzAudio = (IAudioGame)sender;
            var process = quizzAudio.GetNewProcess();
            process = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel fatal -i - -af volume=0.2 -f s16le -ac 2 -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            });
            var inputTask = Task.Run(async () =>
            {
                var stream = await StaticObjects.HttpClient.GetStreamAsync(text);
                await stream.CopyToAsync(process.StandardInput.BaseStream);
                process.StandardInput.Close();
            });
            var outputTask = Task.Run(async () =>
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
            Task.WaitAll(inputTask, outputTask);
            return Task.CompletedTask;
        }
    }
}
