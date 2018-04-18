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
using Discord.Commands;
using MediaToolkit;
using MediaToolkit.Model;
using System.Collections.Generic;
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
            public RadioChannel(IVoiceChannel chan)
            {
                m_chan = chan;
                m_musics = new List<string>();
                m_guildId = chan.GuildId;
            }

            private IVoiceChannel m_chan;
            private List<string> m_musics;
            public ulong m_guildId { private set; get; }
        }

        [Command("Launch radio", RunMode = RunMode.Async), Summary("Launch radio")]
        public async Task youtubeVideo(params string[] words)
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
            foreach (IVoiceChannel chan in await Context.Guild.GetVoiceChannelsAsync())
            {
                IGuildUser user = await chan.GetUserAsync(Context.User.Id);
                if (user != null)
                {
                    p.radios.Add(new RadioChannel(chan));
                    // await chan.ConnectAsync(); // TODO
                    return;
                }
            }
            await ReplyAsync(Sentences.radioNeedChannel);
        }

        public void downloadAudio(string url)
        {
            if (url != null)
            {
                YouTube youTube = YouTube.Default;
                YouTubeVideo video = youTube.GetVideo(url);
                File.WriteAllBytes("Saves/audio." + video.FileExtension, video.GetBytes());
                MediaFile inputFile = new MediaFile { Filename = "Saves/audio." + video.FileExtension };
                MediaFile outputFile = new MediaFile { Filename = "Saves/audio.mp3" };
                using (Engine engine = new Engine())
                {
                    engine.Convert(inputFile, outputFile);
                }
                File.Delete("Saves/audio." + video.FileExtension);
            }
        }
    }
}