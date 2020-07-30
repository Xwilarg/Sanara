using Discord;
using Discord.Audio;
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
            _playlist = new List<Music>();
            _guildId = voiceChan.GuildId;
            _process = null;
            _audioClient = audioClient;
        }

        /// <summary>
        /// Check if the bot is the only user left in the channel
        /// </summary>
        public async Task<bool> IsChannelEmptyAsync()
            => (await _voiceChan.GetUsersAsync().FlattenAsync()).Count() <= 1; // "<=" if somehow the bot isn't in the channel anymore

        /// <summary>
        /// Called when a music is done downloading
        /// </summary>
        public void DownloadDone(string url)
            => _playlist.Find(x => x.Url == url).Downloading = false;

        /// <summary>
        /// Check if a song is already in the playlist
        /// </summary>
        public bool HaveMusic(string url)
            => _playlist.Any(x => x.Url == url);

        /// <summary>
        /// Add a music to the playlist
        /// </summary>
        public void AddMusic(Music m)
            => _playlist.Add(m);

        /// <summary>
        /// Remove a music from the playlist
        /// </summary>
        public string RemoveMusic(string title)
        {
            var music = _playlist.Find(x => x.Title == title);
            if (music == null)
                throw new CommandFailed("There is no music with this name in the playlist.");

            int index = _playlist.IndexOf(music);
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
            if (index < 0 || index >= _playlist.Count)
                throw new CommandFailed("There is no music on the given index.");
            if (index == 0) // The music is the one currently being played
                return Skip();
            string musicTitle = _playlist[index].Title;
            _playlist.RemoveAt(index);
            return musicTitle;
        }

        /// <summary>
        /// Skip the music currently being played
        /// </summary>
        public string Skip()
        {
            if (_process == null)
                throw new CommandFailed("There is no music currently playing");
            _process.Kill();
            return _playlist[0].Title;
        }

        /// <summary>
        /// Stop the radio
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            for (int i = 1; i < _playlist.Count; i++) // We skip the first one because it'll be handled by the radio
                File.Delete(_playlist[i].Path);
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
            string finalStr = $"🎵 Current: {_playlist[0].Title} requested by {_playlist[0].Requester}\n";
            for (int i = 1; i < _playlist.Count; i++)
                finalStr += i + $". {_playlist[i].Title} " + (_playlist[i].Downloading ? "(Download...)" : "") + $" requested by {_playlist[i].Requester}\n";
            return finalStr;
        }

        /// <summary>
        /// Does the playlist have the maximum number of songs in it
        /// </summary>
        /// <returns></returns>
        public bool CanAddMusic()
            => _playlist.Count < MUSIC_COUNT_LIMIT;

        /// <summary>
        /// Play a music
        /// </summary>
        public async Task Play()
        {
            // If the playlist is empty, the current song is not finished yet or the song that need to be played is still downloading
            if (_playlist.Count == 0 || (_process != null && !_process.HasExited) || _playlist[0].Downloading)
                return;
            await _textChan.SendMessageAsync("", false, new EmbedBuilder()
            {
                Title = _playlist[0].Title,
                Url = _playlist[0].Url,
                ImageUrl = _playlist[0].ImageUrl,
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Requested by " + _playlist[0].Requester
                }
            }.Build());
            if (!File.Exists("ffmpeg.exe"))
                throw new FileNotFoundException("ffmpeg.exe was not found near the bot executable.");
            _process = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                // -af volume=0.2 reduce the volume of the song since by default it's really loud
                Arguments = $"-hide_banner -loglevel panic -i \"{_playlist[0].Path}\" -af volume=0.2 -ac 2 -f s16le -ar 48000 pipe:1",
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
                    File.Delete(_playlist[0].Path);
                }
                await discord.FlushAsync();
            }
            File.Delete(_playlist[0].Path);
            _playlist.RemoveAt(0);
            if (_playlist.Count == 0)
            {
                await Stop();
                StaticObjects.Radios.Remove(this);
            }
            else
                await Play(); // If there are others songs in the playlist, we play the next one
        }

        private IVoiceChannel _voiceChan; // Voice channel where the bot is streaming music
        private IMessageChannel _textChan; // Text channel where the bot was asked to join, and where she will send the next music to be played
        private List<Music> _playlist; // Next musics to be played
        private ulong _guildId; // ID of this guild
        private Process _process; // Process of FFMPEG playing the song
        private IAudioClient _audioClient; // Client streaming the song to Discord

        private const int MUSIC_COUNT_LIMIT = 11; // Maximum number of song that can be in a playlist
    }
}
