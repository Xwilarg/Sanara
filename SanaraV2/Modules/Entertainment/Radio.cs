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
using SanaraV2.Modules.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VideoLibrary;

namespace SanaraV2.Modules.Entertainment
{
    public class RadioModule : ModuleBase
    {
        Program p = Program.p;

        public class Song
        {
            public Song(string mpath, string mtitle, string murl, IGuildUser me, string guildId)
            {
                path = mpath;
                title = mtitle;
                url = murl;
                downloading = true;
                if (me.GuildPermissions.AttachFiles)
                {
                    using (WebClient wc = new WebClient())
                    {
                        imageName = "Saves/Radio/" + guildId + "/thumbnail" + DateTime.Now.ToString("HHmmssfff") + me.Id.ToString() + ".jpg";
                        wc.DownloadFile("https://img.youtube.com/vi/" + url.Split('=')[1] + "/0.jpg", imageName);
                    }
                }
                else
                    imageName = null;
            }

            public void DeleteThumbnail()
            {
                if (imageName != null)
                    File.Delete(imageName);
            }

            public string path;
            public string title;
            public string url;
            public bool downloading;
            public string imageName;
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
                    File.Delete(file);
            }

            public void StopDownloading(string url)
            {
                m_musics.Find(x => x.url == url).downloading = false;
            }

            public bool ContainMusic(string url)
            {
                return (m_musics.Any(x => x.url == url));
            }

            public void AddMusic(string path, string title, string url, IGuildUser me, string guildId)
            {
                m_musics.Add(new Song(path, title, url, me, guildId));
            }

            public void RemoveSong(string url)
            {
                m_musics.Remove(m_musics.Find(x => x.url == url));
            }

            public async Task<bool> Skip(IMessageChannel chan)
            {
                if (m_process == null)
                    return (false);
                await chan.SendMessageAsync(Sentences.SongSkipped((chan as ITextChannel).GuildId, m_musics[0].title));
                m_process.Kill();
                return (true);
            }

            public async Task Stop()
            {
                for (int i = 1; i < m_musics.Count; i++)
                    File.Delete(m_musics[i].path);
                if (m_process != null && !m_process.HasExited)
                    m_process.Kill();
                await m_audioClient.StopAsync();
            }

            public string GetPlaylist(ulong guildId)
            {
                if (m_process == null || m_process.HasExited)
                    return (Sentences.RadioNoSong(guildId));
                string finalStr = "🎵 " + Sentences.Current(guildId) + " " + m_musics[0].title + Environment.NewLine;
                for (int i = 1; i < m_musics.Count; i++)
                    finalStr += i + ". " + m_musics[i].title + ((m_musics[i].downloading) ? (" " + Sentences.Downloading(guildId)) : ("")) + Environment.NewLine;
                return (finalStr);
            }

            public bool CanAddMusic()
            {
                return (m_musics.Count <= 11);
            }

            public async void Play()
            {
                if (m_musics.Count == 0 || (m_process != null && !m_process.HasExited) || m_musics[0].downloading)
                    return;
                await m_msgChan.SendMessageAsync("Now playing " + m_musics[0].title);
                await m_msgChan.SendFileAsync(m_musics[0].imageName);
                if (!File.Exists("ffmpeg.exe"))
                    throw new FileNotFoundException("ffmped.exe was not found. Please put it near the bot executable.");
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
                            try
                            {
                                m_musics[0].DeleteThumbnail();
                                File.Delete(m_musics[0].path);
                                break;
                            }
                            catch (IOException)
                            { }
                        }
                    }
                    await discord.FlushAsync();
                }
                m_musics[0].DeleteThumbnail();
                File.Delete(m_musics[0].path);
                m_musics.RemoveAt(0);
                Play();
            }

            private readonly IVoiceChannel m_chan;
            private IMessageChannel m_msgChan;
            private List<Song> m_musics;
            public ulong m_guildId { private set; get; }
            private Process m_process;
            private IAudioClient m_audioClient;
        }

        [Command("Add radio", RunMode = RunMode.Async), Summary("Add songs to the radio"), Alias("Radio add")]
        public async Task AddRadio(params string[] words)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            await Utilities.NotAvailable(Context.Channel as ITextChannel);
            return;
            if (p.youtubeService == null)
                await ReplyAsync(Base.Sentences.NoApiKey(Context.Guild.Id));
            else
            if (words.Length == 0)
                await ReplyAsync(Sentences.RadioNeedArg(Context.Guild.Id));
            else if (p.radios.Any(x => x.m_guildId == Context.Guild.Id) && !p.radios.Find(x => x.m_guildId == Context.Guild.Id).CanAddMusic())
                await ReplyAsync(Sentences.RadioTooMany(Context.Guild.Id));
            else
            {
                if (!p.radios.Any(x => x.m_guildId == Context.Guild.Id))
                {
                    if (!await StartRadio(Context.Channel))
                        return;
                }
                Tuple<string, string> youtubeResult = await YoutubeModule.GetYoutubeMostPopular(words, Context.Channel);
                if (youtubeResult != null)
                {
                    RadioChannel radio = p.radios.Find(x => x.m_guildId == Context.Guild.Id);
                    if (radio.ContainMusic(youtubeResult.Item1))
                    {
                        await ReplyAsync(Sentences.RadioAlreadyInList(Context.Guild.Id));
                        return;
                    }
                    radio.AddMusic("Saves/Radio/" + radio.m_guildId + "/" + Utilities.CleanWord(youtubeResult.Item2) + ".mp3", youtubeResult.Item2, youtubeResult.Item1, await Context.Guild.GetUserAsync(Base.Sentences.myId), Context.Guild.Id.ToString());
                    YouTubeVideo video = GetYoutubeVideo(youtubeResult.Item1);
                    if (video == null)
                    {
                        radio.RemoveSong(youtubeResult.Item1);
                        await ReplyAsync(Sentences.CantDownload(Context.Guild.Id));
                    }
                    else
                    {
                        await ReplyAsync(Sentences.SongAdded(Context.Guild.Id, youtubeResult.Item2));
                        DownloadAudio(video, radio, youtubeResult.Item1, Utilities.CleanWord(youtubeResult.Item2));
                    }
                }
            }
        }

        [Command("Launch radio", RunMode = RunMode.Async), Summary("Launch radio"), Alias("Radio launch", "Radio start", "Start radio")]
        public async Task LaunchRadio(params string[] words)
        {
            await Utilities.NotAvailable(Context.Channel as ITextChannel);
            return;
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            if (p.youtubeService == null)
                await ReplyAsync(Base.Sentences.NoApiKey(Context.Guild.Id));
            else
                await StartRadio(Context.Channel);
        }

        [Command("Playlist radio", RunMode = RunMode.Async), Summary("Display the current playlist"), Alias("Radio playlist")]
        public async Task ListRadio(params string[] words)
        {
            await Utilities.NotAvailable(Context.Channel as ITextChannel);
            return;
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            if (!p.radios.Any(x => x.m_guildId == Context.Guild.Id))
                await ReplyAsync(Sentences.RadioNotStarted(Context.Guild.Id));
            else
                await ReplyAsync(p.radios.Find(x => x.m_guildId == Context.Guild.Id).GetPlaylist(Context.Guild.Id));
        }

        [Command("Skip radio", RunMode = RunMode.Async), Summary("Skip the current song"), Alias("Radio skip")]
        public async Task SkipRadio(params string[] words)
        {
            await Utilities.NotAvailable(Context.Channel as ITextChannel);
            return;
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            RadioChannel radio = p.radios.Find(x => x.m_guildId == Context.Guild.Id);
            if (radio == null)
                await ReplyAsync(Sentences.RadioNotStarted(Context.Guild.Id));
            else
            {
                bool suceed = await radio.Skip(Context.Channel);
                if (!suceed)
                    await ReplyAsync(Sentences.RadioNoSong(Context.Guild.Id));
            }
        }

        private async Task<bool> StartRadio(IMessageChannel chan)
        {
            if (p.radios.Any(x => x.m_guildId == Context.Guild.Id))
            {
                await chan.SendMessageAsync(Sentences.RadioAlreadyStarted(Context.Guild.Id));
                return (true);
            }
            IGuildUser guildUser = Context.User as IGuildUser;
            if (guildUser.VoiceChannel == null)
            {
                await chan.SendMessageAsync(Sentences.RadioNeedChannel(Context.Guild.Id));
                return (false);
            }
            IAudioClient audioClient = await guildUser.VoiceChannel.ConnectAsync();
            p.radios.Add(new RadioChannel(guildUser.VoiceChannel, Context.Channel, audioClient)); // You need opus.dll and libsodium.dll
            return (true);
        }

        [Command("Stop radio", RunMode = RunMode.Async), Summary("Stop radio"), Alias("Radio stop")]
        public async Task StopRadio(params string[] words)
        {
            await Utilities.NotAvailable(Context.Channel as ITextChannel);
            return;
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            RadioChannel radio = p.radios.Find(x => x.m_guildId == Context.Guild.Id);
            if (radio == null)
                await ReplyAsync(Sentences.RadioNotStarted(Context.Guild.Id));
            else
            {
                await radio.Stop();
                p.radios.Remove(radio);
                await ReplyAsync(Base.Sentences.DoneStr(Context.Guild.Id));
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

        private void DownloadAudio(YouTubeVideo video, RadioChannel radio, string url, string title)
        {
            File.WriteAllBytes("Saves/Radio/" + radio.m_guildId + "/" + title + "." + video.FileExtension, video.GetBytes());
            MediaFile inputFile = new MediaFile { Filename = "Saves/Radio/" + radio.m_guildId + "/" + title + "." + video.FileExtension };
            MediaFile outputFile = new MediaFile { Filename = "Saves/Radio/" + radio.m_guildId + "/" + title + ".mp3"};
            using (Engine engine = new Engine())
            {
                engine.Convert(inputFile, outputFile);
            }
            radio.StopDownloading(url);
            File.Delete("Saves/Radio/" + radio.m_guildId + "/" + title + "." + video.FileExtension);
            radio.Play();
        }
    }
}