using Discord;
using Discord.Audio;
using Discord.Commands;
using DiscordUtils;
using SanaraV3.Attribute;
using SanaraV3.Exception;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadRadioHelp()
        {
            /*_submoduleHelp.Add("Radio", "Play musics in a vocal channel");
            _help.Add(("Radio", new Help("Radio", "Radio start", new Argument[0], "Make Sanara join your vocal channel.", new[] { "Skip radio" }, Restriction.PremiumOnly, null)));
            _help.Add(("Radio", new Help("Radio", "Radio add", new[] { new Argument(ArgumentType.MANDATORY, "keywords/id/url") }, "Add a music to the radio.", new[] { "Add radio" }, Restriction.PremiumOnly, "Radio add GgF9zH3Yv1I")));
            _help.Add(("Radio", new Help("Radio", "Radio remove", new[] { new Argument(ArgumentType.MANDATORY, "name/index") }, "Remove a radio from the playlist.", new[] { "Remove radio" }, Restriction.None, "Radio remove 1")));
            _help.Add(("Radio", new Help("Radio", "Radio skip", new Argument[0], "Skip the song that is currently being played.", new[] { "Skip radio" }, Restriction.None, null)));
            _help.Add(("Radio", new Help("Radio", "Radio playlist", new Argument[0], "Display the current playlist.", new[] { "Playlist radio" }, Restriction.None, null)));*/
        }
    }
}

namespace SanaraV3.Module.Radio
{
    public sealed class RadioModule : ModuleBase
    {
        [Command("Skip radio", RunMode = RunMode.Async), Alias("Radio skip")]
        public async Task SkipAsync()
        {
            if (!StaticObjects.Radios.ContainsKey(Context.Guild.Id))
                await ReplyAsync("There is no radio running.");
            else
                StaticObjects.Radios[Context.Guild.Id].Skip();
        }

        [Command("Playlist radio"), Alias("Radio playlist")]
        public async Task PlaylistAsync()
        {
            if (!StaticObjects.Radios.ContainsKey(Context.Guild.Id))
                await ReplyAsync("There is no radio running.");
            else
                await ReplyAsync(StaticObjects.Radios[Context.Guild.Id].GetPlaylist());
        }

        [Command("Stop radio"), Alias("Radio stop")]
        public async Task StopAsync()
        {
            if (!StaticObjects.Radios.ContainsKey(Context.Guild.Id))
                await ReplyAsync("There is no radio running.");
            else
            {
                await StaticObjects.Radios[Context.Guild.Id].StopAsync();
                StaticObjects.Radios.Remove(Context.Guild.Id);
            }
        }

        [Command("Remove radio", RunMode = RunMode.Async), Alias("Radio remove")]
        public async Task RemoveAsync([Remainder]string search)
        {
            if (!StaticObjects.Radios.ContainsKey(Context.Guild.Id))
                await ReplyAsync("There is no radio running.");
            else
            {
                StaticObjects.Radios[Context.Guild.Id].RemoveMusic(search);
                await ReplyAsync("Done~");
            }
        }

        [Command("Remove radio", RunMode = RunMode.Async), Alias("Radio remove"), Priority(1)]
        public async Task RemoveAsync(int id)
        {
            if (!StaticObjects.Radios.ContainsKey(Context.Guild.Id))
                await ReplyAsync("There is no radio running.");
            else
            {
                StaticObjects.Radios[Context.Guild.Id].RemoveMusicWithId(id);
                await ReplyAsync("Done~");
            }
        }

        [Command("Add radio", RunMode = RunMode.Async), Alias("Radio add"), RequirePremium]
        public async Task AddAsync([Remainder]string search)
        {
            var radio = StaticObjects.Radios.ContainsKey(Context.Guild.Id) ? StaticObjects.Radios[Context.Guild.Id] : null;
            if (radio != null && !radio.CanAddMusic())
                throw new CommandFailed("You can't add more musics to the playlist.");

            radio ??= await StartInternalAsync();
            var video = await Entertainment.MediaModule.GetYoutubeVideoAsync(search);
            var url = new Uri("https://youtu.be/" + video.Id);
            if (radio.HaveMusic(url))
                throw new CommandFailed("This music is already in the playlist.");
            await ReplyAsync(video.Snippet.Title + " was added to the playlist.");
            if (!Directory.Exists($"Saves/Radio/{Context.Guild.Id}"))
                Directory.CreateDirectory($"Saves/Radio/{Context.Guild.Id}");
            if (radio.IsLastMusicSuggestion())
            {
                File.Delete($"Saves/Radio/{Context.Guild.Id}/{Utils.CleanWord(video.Snippet.Title)}suggestion.mp3");
                radio.RemoveLastMusic();
            }
            radio.DownloadMusic(Context.Guild.Id, video, Context.User.ToString(), false);
            await radio.PlayAsync();
        }

        [Command("Start radio"), Alias("Radio start", "Launch radio", "Radio launch"), RequirePremium]
        public async Task StartAsync()
        {
            await StartInternalAsync();
        }

        private async Task<RadioChannel> StartInternalAsync()
        {
            if (StaticObjects.Radios.ContainsKey(Context.Guild.Id))
                throw new CommandFailed("You radio is already on.");
            IGuildUser guildUser = Context.User as IGuildUser;
            if (guildUser.VoiceChannel == null)
                throw new CommandFailed("You must be in a vocal channel to use this command.");
            IAudioClient audioClient = await guildUser.VoiceChannel.ConnectAsync();
            RadioChannel radio = new RadioChannel(guildUser.VoiceChannel, Context.Channel, audioClient);
            StaticObjects.Radios.Add(guildUser.GuildId, radio);
            return radio;
        }
    }
}
