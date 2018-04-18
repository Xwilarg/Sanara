/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.

using Discord;
using Discord.Audio;
using Discord.Commands;
using MediaToolkit;
using MediaToolkit.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoLibrary;

namespace SanaraV2
{
    public class RadioModule : ModuleBase
    {
        Program p = Program.p;

        public class RadioChannel
        {
            public RadioChannel(IVoiceChannel chan, IMessageChannel msgChan, IAudioClient audioClient)
            {
                m_chan = chan;
                m_musics = new Dictionary<string, string>();
                m_guildId = chan.GuildId;
                m_process = null;
                m_audioClient = audioClient;
                m_msgChan = msgChan;

                if (!Directory.Exists("Saves/Radio"))
                    Directory.CreateDirectory("Saves/Radio");
                if (!Directory.Exists("Saves/Radio/" + m_guildId))
                    Directory.CreateDirectory("Saves/Radio/" + m_guildId);
                foreach (string file in Directory.GetFiles("Saves/Radio/" + m_guildId))
                {
                    File.Delete(file);
                }
            }

            public void AddMusic(string musicName, string title)
            {
                m_musics.Add(musicName, title);
            }

            public async void Play()
            {
                if (m_musics.Count == 0 || (m_process != null && !m_process.HasExited))
                    return;
                await m_msgChan.SendMessageAsync("Now playing " + m_musics.First().Value);
                m_process = Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $"-hide_banner -loglevel panic -i \"{m_musics.First().Key}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                });
                using (Stream output = m_process.StandardOutput.BaseStream)
                using (AudioOutStream discord = m_audioClient.CreatePCMStream(AudioApplication.Mixed))
                {
                    await output.CopyToAsync(discord);
                    await discord.FlushAsync();
                }
                m_musics.Remove(m_musics.First().Value);
            }

            private IVoiceChannel m_chan;
            private IMessageChannel m_msgChan;
            private Dictionary<string, string> m_musics;
            public ulong m_guildId { private set; get; }
            private Process m_process;
            private IAudioClient m_audioClient;
        }

        [Command("Add radio", RunMode = RunMode.Async), Summary("Add songs to the radio")]
        public async Task addRadio(params string[] words)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            if (Context.User.Id != Sentences.ownerId)
            {
                await ReplyAsync(Sentences.betaFeature);
                return;
            }
            if (words.Length == 0)
            {
                await ReplyAsync(Sentences.radioNeedArg);
                return;
            }
            if (!p.radios.Any(x => x.m_guildId == Context.Guild.Id))
                await launchRadio(null);
            string url = await YoutubeModule.GetYoutubeVideo(words, Context.Channel);
            RadioChannel radio = p.radios.Find(x => x.m_guildId == Context.Guild.Id);
            downloadAudio(url, radio);
            radio.Play();
        }

        [Command("Launch radio", RunMode = RunMode.Async), Summary("Launch radio")]
        public async Task launchRadio(params string[] words)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            if (Context.User.Id != Sentences.ownerId)
            {
                await ReplyAsync(Sentences.betaFeature);
                return;
            }
            if (p.radios.Any(x => x.m_guildId == Context.Guild.Id))
            {
                await ReplyAsync(Sentences.radioAlreadyStarted);
                return;
            }
            IGuildUser guildUser = Context.User as IGuildUser;
            if (guildUser.VoiceChannel == null)
                await ReplyAsync(Sentences.radioNeedChannel);
            else
            {
                IAudioClient audioClient = await guildUser.VoiceChannel.ConnectAsync();
                p.radios.Add(new RadioChannel(guildUser.VoiceChannel, Context.Channel, audioClient)); // You need opus.dll and libsodium.dll
            }
        }

        public void downloadAudio(string url, RadioChannel radio)
        {
            YouTube youTube = YouTube.Default;
            YouTubeVideo video = youTube.GetVideo(url);
            File.WriteAllBytes("Saves/Radio/" + radio.m_guildId + "/" + Program.cleanWord(video.Title) + "." + video.FileExtension, video.GetBytes());
            MediaFile inputFile = new MediaFile { Filename = "Saves/Radio/" + radio.m_guildId + "/" + Program.cleanWord(video.Title) + "." + video.FileExtension };
            MediaFile outputFile = new MediaFile { Filename = "Saves/Radio/" + radio.m_guildId + "/" + Program.cleanWord(video.Title) + ".mp3"};
            using (Engine engine = new Engine())
            {
                engine.Convert(inputFile, outputFile);
            }
            radio.AddMusic("Saves/Radio/" + radio.m_guildId + "/" + Program.cleanWord(video.Title) + ".mp3", video.Title);
            File.Delete("Saves/Radio/" + radio.m_guildId + "/" + Program.cleanWord(video.Title) + "." + video.FileExtension);
        }
    }
}