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
        public async Task PostAsync(IMessageChannel chan, string text, AGame sender)
        {
            var quizzAudio = (QuizzAudio)sender;
            var stream = quizzAudio.GetVoiceSession().CreatePCMStream(AudioApplication.Voice);
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
                await (await StaticObjects.HttpClient.GetStreamAsync(text)).CopyToAsync(process.StandardInput.BaseStream);
                process.StandardInput.Close();
            });
            var outputTask = Task.Run(async () =>
            {
                using Stream output = process.StandardOutput.BaseStream;
                try
                {
                    await output.CopyToAsync(stream);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            });
            Task.WaitAll(inputTask, outputTask);
        }
    }
}
