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
using SanaraV2.Base;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SanaraV2.Entertainment
{
    public class AnimeMangaModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Anime", RunMode = RunMode.Async), Summary("Give informations about an anime using MyAnimeList API")]
        public async Task Mal(params string[] animeNameArr)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.AnimeManga);
            if (p.malClient == null)
            {
                await ReplyAsync(Base.Sentences.NoApiKey(Context.Guild.Id));
                return;
            }
            string animeName = Utilities.AddArgs(animeNameArr);
            if (animeName.Length == 0)
            {
                await ReplyAsync(Sentences.AnimeHelp(Context.Guild.Id));
                return;
            }
            try
            {
                string result = p.malClient.DownloadString("https://myanimelist.net/api/anime/search.xml?q=" + animeName.Replace(" ", "%20"));
                if (!result.Contains("<entry>"))
                    await ReplyAsync(Sentences.AnimeNotFound(Context.Guild.Id));
                else
                {
                    EmbedBuilder b = ParseContent(result, animeName, (Context.Channel as ITextChannel).IsNsfw, Context.Guild.Id);
                    await ReplyAsync("", false, b.Build());
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse code)
                {
                    if (code.StatusCode == HttpStatusCode.Forbidden)
                        await ReplyAsync(Base.Sentences.TooManyRequests(Context.Guild.Id, "MyAnimeList"));
                    else
                        await ReplyAsync(Base.Sentences.HttpError(Context.Guild.Id, "MyAnimeList"));
                }
                else
                    await ReplyAsync(Base.Sentences.HttpError(Context.Guild.Id, "MyAnimeList"));
            }
        }

        [Command("Manga", RunMode = RunMode.Async), Summary("Give informations about a manga using MyAnimeList API")]
        public async Task MalManga(params string[] mangaNameArr) // Stuck in loop ?
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.AnimeManga);
            if (p.malClient == null)
            {
                await ReplyAsync(Base.Sentences.NoApiKey(Context.Guild.Id));
                return;
            }
            string mangaName = Utilities.AddArgs(mangaNameArr);
            if (mangaName.Length == 0)
            {
                await ReplyAsync(Sentences.AnimeHelp(Context.Guild.Id));
                return;
            }
            try
            {
                string result = p.malClient.DownloadString("https://myanimelist.net/api/manga/search.xml?q=" + mangaName.Replace(" ", "%20"));
                if (!result.Contains("<entry>"))
                    await ReplyAsync(Sentences.MangaNotFound(Context.Guild.Id));
                else
                {
                    EmbedBuilder b = ParseContent(result, mangaName, (Context.Channel as ITextChannel).IsNsfw, Context.Guild.Id);
                    if (b == null)
                        await ReplyAsync(Base.Sentences.ChanIsNotNsfw(Context.Guild.Id));
                    else
                        await ReplyAsync("", false, b.Build());
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse code)
                {
                    if (code.StatusCode == HttpStatusCode.Forbidden)
                        await ReplyAsync(Base.Sentences.TooManyRequests(Context.Guild.Id, "MyAnimeList"));
                    else
                        await ReplyAsync(Base.Sentences.HttpError(Context.Guild.Id, "MyAnimeList"));
                }
                else
                    await ReplyAsync(Base.Sentences.HttpError(Context.Guild.Id, "MyAnimeList"));
            }
        }

        private EmbedBuilder ParseContent(string result, string animeName, bool isNsfw, ulong guildId) // TODO: Handle ratings
        {
            string[] entries = result.Split(new string[] { "<entry>" }, StringSplitOptions.None);
            int index = 1;
            for (int i = 1; i < entries.Length; i++)
            {
                if (Utilities.GetElementXml("<title>", entries[i], '<').ToUpper() == animeName.ToUpper())
                {
                    index = i;
                    break;
                }
            }
            string id = Utilities.RemoveUnwantedSymboles(Utilities.GetElementXml("<id>", entries[index], '<'));
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString("https://myanimelist.net/anime/" + id);
                if (!isNsfw && Utilities.GetElementXml("<span class=\"dark_text\">Rating:</span>", json, '<').Contains("Rx"))
                    return (null);
            }
            string title = Utilities.RemoveUnwantedSymboles(Utilities.GetElementXml("<title>", entries[index], '<'));
            string english = Utilities.RemoveUnwantedSymboles(Utilities.GetElementXml("<english>", entries[index], '<'));
            string synonyms = Utilities.RemoveUnwantedSymboles(Utilities.GetElementXml("<synonyms>", entries[index], '<'));
            string episodes = Utilities.GetElementXml("<episodes>", entries[index], '<');
            if (episodes == "")
                episodes = Utilities.GetElementXml("<volumes>", entries[index], '<');
            string type = Utilities.GetElementXml("<type>", entries[index], '<');
            string status = Utilities.GetElementXml("<status>", entries[index], '<');
            string score = Utilities.GetElementXml("<score>", entries[index], '<');
            string synopsis = Utilities.RemoveUnwantedSymboles(Utilities.GetElementXml("<synopsis>", entries[index], '<'));
            string currentTime = DateTime.Now.ToString("HHmmss") + Context.Guild.Id.ToString() + Context.User.Id.ToString();
            EmbedBuilder embed = new EmbedBuilder()
            {
                ImageUrl = Utilities.GetElementXml("<image>", entries[index], '<'),
                Description = "**" + title + "** (" + english + ")" + Environment.NewLine
                + Base.Sentences.OrStr(guildId) + " " + synonyms + Environment.NewLine + Environment.NewLine
                + Sentences.AnimeInfos(guildId, type, status, episodes) + Environment.NewLine
                + Sentences.AnimeScore(guildId, score) + Environment.NewLine + Environment.NewLine
                + "**" + Sentences.Synopsis(guildId) + "**" + Environment.NewLine + synopsis,
                Color = Color.Green,
            };
            return (embed);
        }
    }
}