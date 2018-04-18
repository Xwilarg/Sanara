﻿/// This file is part of Sanara.
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
using System;
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

        public struct Song
        {
            public Song(string mpath, string mtitle, string murl)
            {
                path = mpath;
                title = mtitle;
                url = murl;
            }

            public string path;
            public string title;
            public string url;
        }

        public class RadioChannel
        {
            public RadioChannel(IVoiceChannel chan, IMessageChannel msgChan, IAudioClient audioClient)
            {
                m_chan = chan;
                m_musics = new List<Song>();
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

            public bool ContainMusic(string url)
            {
                return (m_musics.Any(x => x.url == url));
            }

            public void AddMusic(string path, string title, string url)
            {
                m_musics.Add(new Song(path, title.Substring(0, title.Length - 9), url));
            }

            public async Task Stop()
            {
                for (int i = 1; i < m_musics.Count; i++)
                    File.Delete(m_musics[i].path);
                if (m_process != null && !m_process.HasExited)
                    m_process.Kill();
                await m_audioClient.StopAsync();
            }

            public string GetPlaylist()
            {
                if (m_process == null || m_process.HasExited)
                    return (Sentences.radioNoSong);
                string finalStr = "🎵 Current:" + m_musics[0].title + Environment.NewLine;
                for (int i = 1; i < m_musics.Count; i++)
                    finalStr += i + ". " + m_musics[i].title + Environment.NewLine;
                return (finalStr);
            }

            public bool CanAddMusic()
            {
                return (m_musics.Count <= 11);
            }

            public async void Play()
            {
                if (m_musics.Count == 0 || (m_process != null && !m_process.HasExited))
                    return;
                await m_msgChan.SendMessageAsync("Now playing " + m_musics[0].title);
                m_process = Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $"-hide_banner -loglevel panic -i \"{m_musics[0].path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                });
                using (Stream output = m_process.StandardOutput.BaseStream)
                using (AudioOutStream discord = m_audioClient.CreatePCMStream(AudioApplication.Mixed))
                {
                    try
                    {
                        await output.CopyToAsync(discord);
                    } catch (OperationCanceledException) {
                        while (true)
                        {
                            try {
                                File.Delete(m_musics[0].path);
                                break;
                            } catch (IOException) { }
                        }
                    }
                    await discord.FlushAsync();
                }
                File.Delete(m_musics[0].path);
                m_musics.RemoveAt(0);
                Console.WriteLine(m_musics.Count);
                Play();
            }

            private IVoiceChannel m_chan;
            private IMessageChannel m_msgChan;
            private List<Song> m_musics;
            public ulong m_guildId { private set; get; }
            private Process m_process;
            private IAudioClient m_audioClient;
        }

        [Command("Add radio", RunMode = RunMode.Async), Summary("Add songs to the radio"), Alias("Radio add")]
        public async Task addRadio(params string[] words)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            if (Context.User.Id != Sentences.ownerId)
                await ReplyAsync(Sentences.betaFeature);
            else if (words.Length == 0)
                await ReplyAsync(Sentences.radioNeedArg);
            else if (p.radios.Any(x => x.m_guildId == Context.Guild.Id) && !p.radios.Find(x => x.m_guildId == Context.Guild.Id).CanAddMusic())
                await ReplyAsync(Sentences.radioTooMany);
            else
            {
                if (!p.radios.Any(x => x.m_guildId == Context.Guild.Id))
                    await StartRadio(Context.Channel);
                Tuple<string, string> youtubeResult = await YoutubeModule.GetYoutubeVideo(words, Context.Channel);
                if (youtubeResult != null)
                {
                    YouTubeVideo video = GetYoutubeVideo(youtubeResult.Item1);
                    if (video == null)
                        await ReplyAsync(Sentences.cantDownload);
                    else
                    {
                        RadioChannel radio = p.radios.Find(x => x.m_guildId == Context.Guild.Id);
                        if (radio.ContainMusic(youtubeResult.Item1))
                            await ReplyAsync(Sentences.radioAlreadyInList);
                        else
                        {
                            await ReplyAsync(youtubeResult.Item2 + " was added to the list.");
                            DownloadAudio(video, radio, youtubeResult.Item1);
                            radio.Play();
                        }
                    }
                }
            }
        }

        [Command("Launch radio", RunMode = RunMode.Async), Summary("Launch radio"), Alias("Radio launch")]
        public async Task launchRadio(params string[] words)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            if (Context.User.Id != Sentences.ownerId)
            {
                await ReplyAsync(Sentences.betaFeature);
                return;
            }
            await StartRadio(Context.Channel);
        }

        [Command("Playlist radio", RunMode = RunMode.Async), Summary("Launch radio"), Alias("Radio playlist")]
        public async Task listRadio(params string[] words)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            if (Context.User.Id != Sentences.ownerId)
                await ReplyAsync(Sentences.betaFeature);
            else if (!p.radios.Any(x => x.m_guildId == Context.Guild.Id))
                await ReplyAsync(Sentences.radioNotStarted);
            else
                await ReplyAsync(p.radios.Find(x => x.m_guildId == Context.Guild.Id).GetPlaylist());
        }

        private async Task StartRadio(IMessageChannel chan)
        {
            if (p.radios.Any(x => x.m_guildId == Context.Guild.Id))
            {
                await chan.SendMessageAsync(Sentences.radioAlreadyStarted);
                return;
            }
            IGuildUser guildUser = Context.User as IGuildUser;
            if (guildUser.VoiceChannel == null)
                await chan.SendMessageAsync(Sentences.radioNeedChannel);
            else
            {
                IAudioClient audioClient = await guildUser.VoiceChannel.ConnectAsync();
                p.radios.Add(new RadioChannel(guildUser.VoiceChannel, Context.Channel, audioClient)); // You need opus.dll and libsodium.dll
            }
        }

        [Command("Stop radio", RunMode = RunMode.Async), Summary("Stop radio"), Alias("Radio stop")]
        public async Task stopRadio(params string[] words)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            if (Context.User.Id != Sentences.ownerId)
            {
                await ReplyAsync(Sentences.betaFeature);
                return;
            }
            RadioChannel radio = p.radios.Find(x => x.m_guildId == Context.Guild.Id);
            if (radio == null)
                await ReplyAsync(Sentences.radioNotStarted);
            else
            {
                await radio.Stop();
                p.radios.Remove(radio);
                await ReplyAsync(Sentences.doneStr);
            }
        }

        private YouTubeVideo GetYoutubeVideo(string url)
        {
            YouTube youTube = YouTube.Default;
            YouTubeVideo video;
            try
            {
                video = youTube.GetVideo(url);
            }
            catch (InvalidOperationException)
            {
                return (null);
            }
            return (video);
        }

        private void DownloadAudio(YouTubeVideo video, RadioChannel radio, string url)
        {
            File.WriteAllBytes("Saves/Radio/" + radio.m_guildId + "/" + Program.cleanWord(video.Title) + "." + video.FileExtension, video.GetBytes());
            MediaFile inputFile = new MediaFile { Filename = "Saves/Radio/" + radio.m_guildId + "/" + Program.cleanWord(video.Title) + "." + video.FileExtension };
            MediaFile outputFile = new MediaFile { Filename = "Saves/Radio/" + radio.m_guildId + "/" + Program.cleanWord(video.Title) + ".mp3"};
            using (Engine engine = new Engine())
            {
                engine.Convert(inputFile, outputFile);
            }
            radio.AddMusic("Saves/Radio/" + radio.m_guildId + "/" + Program.cleanWord(video.Title) + ".mp3", video.Title, url);
            File.Delete("Saves/Radio/" + radio.m_guildId + "/" + Program.cleanWord(video.Title) + "." + video.FileExtension);
        }
    }
}