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
using System;
using System.Net;
using System.Threading.Tasks;

namespace SanaraV2
{
    public class MyAnimeListModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Anime", RunMode = RunMode.Async), Summary("Give informations about an anime using MyAnimeList API")]
        public async Task mal(params string[] animeNameArr)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.AnimeManga);
            if (p.malClient == null)
            {
                await ReplyAsync(Sentences.noApiKey(Context.Guild.Id));
                return;
            }
            string animeName = Utilities.addArgs(animeNameArr);
            if (animeName.Length == 0)
            {
                await ReplyAsync(Sentences.animeHelp(Context.Guild.Id));
                return;
            }
            try
            {
                string result = p.malClient.DownloadString("https://myanimelist.net/api/anime/search.xml?q=" + animeName.Replace(" ", "%20"));
                if (!result.Contains("<entry>"))
                    await ReplyAsync(Sentences.animeNotFound(Context.Guild.Id));
                else
                {
                    EmbedBuilder b = parseContent(result, animeName, (Context.Channel as ITextChannel).IsNsfw, Context.Guild.Id);
                    await ReplyAsync("", false, b.Build());
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse code = ex.Response as HttpWebResponse;
                if (code != null)
                {
                    if (code.StatusCode == HttpStatusCode.Forbidden)
                        await ReplyAsync(Sentences.tooManyRequests(Context.Guild.Id, "MyAnimeList"));
                }
                else
                    await ReplyAsync("An unexpected error occured: " + ex.Message);
            }
        }

        [Command("Manga", RunMode = RunMode.Async), Summary("Give informations about a manga using MyAnimeList API")]
        public async Task malManga(params string[] mangaNameArr) // Stuck in loop ?
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.AnimeManga);
            if (p.malClient == null)
            {
                await ReplyAsync(Sentences.noApiKey(Context.Guild.Id));
                return;
            }
            string mangaName = Utilities.addArgs(mangaNameArr);
            if (mangaName.Length == 0)
            {
                await ReplyAsync(Sentences.animeHelp(Context.Guild.Id));
                return;
            }
            try
            {
                string result = p.malClient.DownloadString("https://myanimelist.net/api/manga/search.xml?q=" + mangaName.Replace(" ", "%20"));
                if (!result.Contains("<entry>"))
                    await ReplyAsync(Sentences.mangaNotFound(Context.Guild.Id));
                else
                {
                    EmbedBuilder b = parseContent(result, mangaName, (Context.Channel as ITextChannel).IsNsfw, Context.Guild.Id);
                    if (b == null)
                        await ReplyAsync(Sentences.chanIsNotNsfw(Context.Guild.Id));
                    else
                        await ReplyAsync("", false, b.Build());
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse code = ex.Response as HttpWebResponse;
                if (code.StatusCode == HttpStatusCode.Forbidden)
                    await ReplyAsync(Sentences.tooManyRequests(Context.Guild.Id, "MyAnimeList"));
            }
        }

        private EmbedBuilder parseContent(string result, string animeName, bool isNsfw, ulong guildId) // TODO: Handle ratings
        {
            string[] entries = result.Split(new string[] { "<entry>" }, StringSplitOptions.None);
            int index = 1;
            for (int i = 1; i < entries.Length; i++)
            {
                if (Utilities.getElementXml("<title>", entries[i], '<').ToUpper() == animeName.ToUpper())
                {
                    index = i;
                    break;
                }
            }
            string id = Utilities.removeUnwantedSymboles(Utilities.getElementXml("<id>", entries[index], '<'));
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString("https://myanimelist.net/anime/" + id);
                if (!isNsfw && Utilities.getElementXml("<span class=\"dark_text\">Rating:</span>", json, '<').Contains("Rx"))
                    return (null);
            }
            string title = Utilities.removeUnwantedSymboles(Utilities.getElementXml("<title>", entries[index], '<'));
            string english = Utilities.removeUnwantedSymboles(Utilities.getElementXml("<english>", entries[index], '<'));
            string synonyms = Utilities.removeUnwantedSymboles(Utilities.getElementXml("<synonyms>", entries[index], '<'));
            string episodes = Utilities.getElementXml("<episodes>", entries[index], '<');
            if (episodes == "")
                episodes = Utilities.getElementXml("<volumes>", entries[index], '<');
            string type = Utilities.getElementXml("<type>", entries[index], '<');
            string status = Utilities.getElementXml("<status>", entries[index], '<');
            string score = Utilities.getElementXml("<score>", entries[index], '<');
            string synopsis = Utilities.removeUnwantedSymboles(Utilities.getElementXml("<synopsis>", entries[index], '<'));
            string currentTime = DateTime.Now.ToString("HHmmss") + Context.Guild.Id.ToString() + Context.User.Id.ToString();
            EmbedBuilder embed = new EmbedBuilder()
            {
                ImageUrl = Utilities.getElementXml("<image>", entries[index], '<'),
                Description = "**" + title + "** (" + english + ")" + Environment.NewLine
                + Sentences.orStr(guildId) + " " + synonyms + Environment.NewLine + Environment.NewLine
                + Sentences.animeInfos(guildId, type, status, episodes) + Environment.NewLine
                + Sentences.animeScore(guildId, score) + Environment.NewLine + Environment.NewLine
                + "**" + Sentences.synopsis(guildId) + "**" + Environment.NewLine + synopsis,
                Color = Color.Green,
            };
            return (embed);
        }
    }
}