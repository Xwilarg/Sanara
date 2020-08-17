using Discord;
using Discord.Audio;
using DiscordUtils;
using Google.Apis.YouTube.v3.Data;
using SanaraV3.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Radio
{
    public sealed class RadioChannel
    {
        public RadioChannel(IVoiceChannel voiceChan, IMessageChannel textChan, IAudioClient audioClient)
        {
            _voiceChan = voiceChan;
            _textChan = textChan;
            Playlist = new List<Music>();
            _guildId = voiceChan.GuildId;
            _process = null;
            _audioClient = audioClient;
            _recentlyPlayed = new List<string>();
        }

        /// <summary>
        /// Check if the bot is the only user left in the channel
        /// </summary>
        public async Task<bool> IsChannelEmptyAsync()
            => (await _voiceChan.GetUsersAsync().FlattenAsync()).Count() <= 1; // "<=" if somehow the bot isn't in the channel anymore

        /// <summary>
        /// Called when a music is done downloading
        /// </summary>
        private void DownloadDone(Uri url)
        {
            var music = Playlist.Find(x => x.Url == url);
            if (music != null)
                music.Downloading = false;
        }

        /// <summary>
        /// Check if a song is already in the playlist
        /// </summary>
        public bool HaveMusic(Uri url)
            => Playlist.Any(x => x.Url == url);

        /// <summary>
        /// Add a music to the playlist
        /// </summary>
        private void AddMusic(Music m)
            => Playlist.Add(m);

        /// <summary>
        /// Remove a music from the playlist
        /// </summary>
        public string RemoveMusic(string title)
        {
            var music = Playlist.Find(x => x.Title == title);
            if (music == null)
                throw new CommandFailed("There is no music with this name in the playlist.");

            int index = Playlist.IndexOf(music);
            return RemoveMusicWithId(index);
        }

        /// <summary>
        /// Remove a music from the playlist given its ID
        /// </summary>
        /// <param name="chan"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public string RemoveMusicWithId(int index)
        {
            if (index < 0 || index >= Playlist.Count)
                throw new CommandFailed("There is no music on the given index.");
            if (index == 0) // The music is the one currently being played
                return Skip();
            var music = Playlist[index];
            Playlist.RemoveAt(index);
            if (Playlist.Count == 1 && !music.IsAutoSuggestion)
                _ = Task.Run(AddAutosuggestionAsync);
            return music.Title;
        }

        /// <summary>
        /// Remove the last music of the playlist
        /// </summary>
        public void RemoveLastMusic()
        {
            Playlist.RemoveAt(Playlist.Count - 1);
        }

        /// <summary>
        /// Skip the music currently being played
        /// </summary>
        public string Skip()
        {
            if (_process == null)
                throw new CommandFailed("There is no music currently playing");
            _process.Kill();
            return Playlist[0].Title;
        }

        /// <summary>
        /// Stop the radio
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            for (int i = 1; i < Playlist.Count; i++) // We skip the first one because it'll be handled by the radio
                File.Delete(Playlist[i].Path);
            if (_process != null && !_process.HasExited)
                _process.Kill();
            await _audioClient.StopAsync();
        }

        /// <summary>
        /// Display the current playlist
        /// </summary>
        public string GetPlaylist()
        {
            if (_process == null || _process.HasExited)
                return "There is no song currently being played.";
            string finalStr = $"🎵 Current: {Playlist[0].Title} ({Playlist[0].Duration}) requested by {Playlist[0].Requester}\n";
            for (int i = 1; i < Playlist.Count; i++)
                finalStr += i + $". {Playlist[i].Title} ({ Playlist[0].Duration}) " + (Playlist[i].Downloading ? "(Download...)" : "") + $" requested by {Playlist[i].Requester}\n";
            return finalStr;
        }

        /// <summary>
        /// Does the playlist have the maximum number of songs in it
        /// </summary>
        /// <returns></returns>
        public bool CanAddMusic()
            => Playlist.Count < MUSIC_COUNT_LIMIT;

        private async Task AddAutosuggestionAsync()
        {
            var r = StaticObjects.YouTube.Search.List("snippet");
            r.RelatedToVideoId = Playlist.Last().Id;
            r.Type = "video";
            r.MaxResults = 10;
            string id = null;
            foreach (var res in (await r.ExecuteAsync()).Items)
            {
                if (!_recentlyPlayed.Contains(res.Id.VideoId))
                {
                    id = res.Id.VideoId;
                    break;
                }
            }
            if (id != null)
            {
                var rr = StaticObjects.YouTube.Videos.List("snippet,statistics,contentDetails");
                rr.Id = id;
                Video video = (await rr.ExecuteAsync()).Items[0];
                DownloadMusic(_guildId, video, "Sanara#1537 (Autosuggestion)", true);
            }
        }

        /// <summary>
        /// Play a music
        /// </summary>
        public async Task PlayAsync()
        {
            // If the playlist is empty, the current song is not finished yet or the song that need to be played is still downloading
            if (Playlist.Count == 0 || (_process != null && !_process.HasExited) || Playlist[0].Downloading)
                return;
            if (Playlist.Count == 1) // There is only one music remaining in the playlist so we add a custom suggestion based on the last one
                _ = Task.Run(AddAutosuggestionAsync);
            await _textChan.SendMessageAsync(embed: Playlist[0].Embed);
            if (!File.Exists("ffmpeg.exe"))
                throw new FileNotFoundException("ffmpeg.exe was not found near the bot executable.");
            _process = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                // -af volume=0.2 reduce the volume of the song since by default it's really loud
                Arguments = $"-hide_banner -loglevel panic -i \"{Playlist[0].Path}\" -af volume=0.2 -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
            using (Stream output = _process.StandardOutput.BaseStream)
            using (AudioOutStream discord = _audioClient.CreatePCMStream(AudioApplication.Music))
            {
                try
                {
                    await output.CopyToAsync(discord);
                }
                catch (OperationCanceledException) // If we stopped the music midway (stop/skip command)
                {
                    // If force the GC to do his job so we can safely delete the music file
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    File.Delete(Playlist[0].Path);
                }
                await discord.FlushAsync();
            }
            File.Delete(Playlist[0].Path);
            Playlist.RemoveAt(0);
            if (Playlist.Count == 0)
            {
                await StopAsync();
                StaticObjects.Radios.Remove(_guildId);
            }
            else
                await PlayAsync(); // If there are others songs in the playlist, we play the next one
        }

        /// <summary>
        /// Is the last video of the playlist a suggestion made by the bot
        /// </summary>
        public bool IsLastMusicSuggestion()
            => Playlist.Count != 0 && Playlist.Last().IsAutoSuggestion;

        public void DownloadMusic(ulong guildId, Video video, string requester, bool isAutoSuggestion)
        {
            var url = new Uri("https://youtu.be/" + video.Id);
            string fileName = $"Saves/Radio/{guildId}/{Utils.CleanWord(video.Snippet.Title)}{DateTime.Now:HHmmssff}.mp3";
            string duration;
            var embed = Entertainment.MediaModule.GetEmbedFromVideo(video, out duration);
            embed.Description = "Requested by " + requester;
            AddMusic(new Music(video.Id, fileName, video.Snippet.Title, url, embed.Build(), requester, isAutoSuggestion, duration));
            if (!File.Exists("youtube-dl.exe"))
                throw new FileNotFoundException("youtube-dl.exe was not found near the bot executable.");
            ProcessStartInfo youtubeDownload = new ProcessStartInfo
            {
                FileName = "youtube-dl",
                Arguments = $"-x --audio-format mp3 -o " + fileName + " " + url,
                CreateNoWindow = true
            };
            youtubeDownload.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(youtubeDownload).WaitForExit();
            _recentlyPlayed.Add(video.Id);
            if (_recentlyPlayed.Count > MUSIC_COUNT_KEEP_ID)
                _recentlyPlayed.RemoveAt(0);
            DownloadDone(url);
        }

        private readonly IVoiceChannel _voiceChan; // Voice channel where the bot is streaming music
        private readonly IMessageChannel _textChan; // Text channel where the bot was asked to join, and where she will send the next music to be played
        public List<Music> Playlist { private set; get; } // Next musics to be played
        private readonly ulong _guildId; // ID of this guild, used to remove the radio from the dictionary
        private Process _process; // Process of FFMPEG playing the song
        private readonly IAudioClient _audioClient; // Client streaming the song to Discord
        private readonly List<string> _recentlyPlayed; // IDs of recently played musics so we don't autosuggest things that were played not so long ago

        private const int MUSIC_COUNT_LIMIT = 11; // Maximum number of musics that can be in a playlist
        private const int MUSIC_COUNT_KEEP_ID = 10; // Maximum of we IDs we keep in _recentlyPlayed
    }
}
