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
using SanaraV2.Modules.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Entertainment
{
    public class RadioModule : ModuleBase
    {
        Program p = Program.p;

        public class Song
        {
            public Song(string mpath, string mtitle, string murl, string mimageUrl, string mrequester)
            {
                path = mpath;
                title = mtitle;
                url = murl;
                downloading = true;
                imageUrl = mimageUrl;
                requester = mrequester;
            }

            public string path;
            public string title;
            public string url;
            public bool downloading;
            public string imageUrl;
            public string requester;
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

            public async Task<bool> IsChanEmpty()
            {
                return (await m_chan.GetUsersAsync().FlattenAsync()).Count() == 1; // 1 because the bot is in the channel
            }

            public void StopDownloading(string url)
            {
                m_musics.Find(x => x.url == url).downloading = false;
            }

            public bool ContainMusic(string url)
            {
                return (m_musics.Any(x => x.url == url));
            }

            public void AddMusic(string path, string title, string url, string imageUrl, string requester)
            {
                m_musics.Add(new Song(path, title, url, imageUrl, requester));
            }

            public async Task<bool> Skip(IMessageChannel chan)
            {
                if (m_process == null)
                    return false;
                await chan.SendMessageAsync(Sentences.SongSkipped((chan as ITextChannel).GuildId, m_musics[0].title));
                m_process.Kill();
                return true;
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
                    return Sentences.RadioNoSong(guildId);
                string finalStr = "🎵 " + Sentences.Current(guildId) + " " + m_musics[0].title + " requested by " + m_musics[0].requester + Environment.NewLine;
                for (int i = 1; i < m_musics.Count; i++)
                    finalStr += i + ". " + m_musics[i].title + ((m_musics[i].downloading) ? (" " + Sentences.Downloading(guildId)) : ("")) + " requested by " + m_musics[0].requester + Environment.NewLine;
                return finalStr;
            }

            public bool CanAddMusic()
            {
                return m_musics.Count <= 11;
            }

            public async Task Play()
            {
                if (m_musics.Count == 0 || (m_process != null && !m_process.HasExited) || m_musics[0].downloading)
                    return;
                await m_msgChan.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = m_musics[0].title,
                    Url = m_musics[0].url,
                    ImageUrl = m_musics[0].imageUrl,
                    Color = Color.Blue,
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "Requested by " + m_musics[0].requester
                    }
                }.Build());
                if (!File.Exists("ffmpeg.exe"))
                    throw new FileNotFoundException("ffmpeg.exe was not found. Please put it near the bot executable.");
                m_process = Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $"-hide_banner -loglevel panic -i \"{m_musics[0].path}\" -af volume=0.2 -ac 2 -f s16le -ar 48000 pipe:1",
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
                                File.Delete(m_musics[0].path);
                                break;
                            }
                            catch (IOException)
                            { }
                        }
                    }
                    await discord.FlushAsync();
                }
                File.Delete(m_musics[0].path);
                m_musics.RemoveAt(0);
                if (m_musics.Count == 0)
                {
                    await Stop();
                    Program.p.radios.Remove(this);
                }
                else
                    await Play();
            }

            private readonly IVoiceChannel m_chan;
            private IMessageChannel m_msgChan;
            private List<Song> m_musics;
            public ulong m_guildId { private set; get; }
            private Process m_process;
            private IAudioClient m_audioClient;
        }

        [Command("Radio"), Priority(-1)]
        public async Task Help(params string[] args)
        {
            await ReplyAsync("", false, new EmbedBuilder()
            {
                Title = Tools.Sentences.Help(Context.Guild.Id) + " (" + Tools.Sentences.RadioModuleName(Context.Guild.Id) + ")",
                Description = Tools.Sentences.RadioHelp(Context.Guild.Id),
                Color = Color.Purple
            }.Build());
        }

        [Command("Add radio", RunMode = RunMode.Async), Summary("Add songs to the radio"), Alias("Radio add")]
        public async Task AddRadio(params string[] words)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Radio);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Radio);
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
                var result = await Features.Entertainment.YouTube.SearchYouTube(words, Program.p.youtubeService);
                if (result.error == Features.Entertainment.Error.YouTube.None)
                {
                    RadioChannel radio = p.radios.Find(x => x.m_guildId == Context.Guild.Id);
                    if (radio.ContainMusic(result.answer.url))
                    {
                        await ReplyAsync(Sentences.RadioAlreadyInList(Context.Guild.Id));
                        return;
                    }
                    await ReplyAsync(Sentences.SongAdded(Context.Guild.Id, result.answer.name));
                    string fileName = "Saves/Radio/" + radio.m_guildId + "/" + Utilities.CleanWord(result.answer.name) + ".mp3";
                    radio.AddMusic(fileName, result.answer.name, result.answer.url, result.answer.imageUrl, Context.User.ToString());
                    ProcessStartInfo youtubeDownload = new ProcessStartInfo()
                    {
                        FileName = "youtube-dl",
                        Arguments = $"-x --audio-format mp3 -o " + fileName + " " + result.answer.url,
                        CreateNoWindow = true
                    };
                    youtubeDownload.WindowStyle = ProcessWindowStyle.Hidden;
                    Process.Start(youtubeDownload).WaitForExit();
                    radio.StopDownloading(result.answer.url);
                    await radio.Play();
                }
                else
                    await ReplyAsync("YouTube error: " + result.error);
            }
        }

        [Command("Launch radio", RunMode = RunMode.Async), Summary("Launch radio"), Alias("Radio launch", "Radio start", "Start radio")]
        public async Task LaunchRadio(params string[] words)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Radio);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            if (p.youtubeService == null)
                await ReplyAsync(Base.Sentences.NoApiKey(Context.Guild.Id));
            else
                await StartRadio(Context.Channel);
        }

        [Command("Playlist radio", RunMode = RunMode.Async), Summary("Display the current playlist"), Alias("Radio playlist", "Radio list", "List radio")]
        public async Task ListRadio(params string[] words)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Radio);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Radio);
            if (!p.radios.Any(x => x.m_guildId == Context.Guild.Id))
                await ReplyAsync(Sentences.RadioNotStarted(Context.Guild.Id));
            else
                await ReplyAsync(p.radios.Find(x => x.m_guildId == Context.Guild.Id).GetPlaylist(Context.Guild.Id));
        }

        [Command("Skip radio", RunMode = RunMode.Async), Summary("Skip the current song"), Alias("Radio skip")]
        public async Task SkipRadio(params string[] words)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Radio);
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
                return true;
            }
            IGuildUser guildUser = Context.User as IGuildUser;
            if (guildUser.VoiceChannel == null)
            {
                await chan.SendMessageAsync(Sentences.RadioNeedChannel(Context.Guild.Id));
                return false;
            }
            IAudioClient audioClient = await guildUser.VoiceChannel.ConnectAsync();
            p.radios.Add(new RadioChannel(guildUser.VoiceChannel, Context.Channel, audioClient)); // You need opus.dll and libsodium.dll
            return true;
        }

        [Command("Stop radio", RunMode = RunMode.Async), Summary("Stop radio"), Alias("Radio stop", "Radio quit", "Quit radio")]
        public async Task StopRadio(params string[] words)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Radio);
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
    }
}