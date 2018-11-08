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
using Discord.Commands;
using System;
using System.Net;
using Discord;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Discord.Net;
using System.Linq;
using SanaraV2.Modules.Base;
using BooruSharp.Booru;

namespace SanaraV2.Modules.NSFW
{
    public class Booru : ModuleBase
    {
        Program p = Program.p;

        private enum TagId
        {
            Safebooru,
            Gelbooru,
            Konachan,
            Rule34,
            E621,
            E926,
            Sakugabooru
        }

        [Command("Safebooru", RunMode = RunMode.Async), Summary("Get an image from Safebooru")]
        public async Task SafebooruSearch(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Safebooru(), Context.Channel as ITextChannel, tags, TagId.Safebooru);
        }

        [Command("Gelbooru", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task GelbooruSearch(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Gelbooru(), Context.Channel as ITextChannel, tags, TagId.Gelbooru);
        }

        [Command("Konachan", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task KonachanSearch(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Konachan(), Context.Channel as ITextChannel, tags, TagId.Konachan);
        }

        [Command("Rule34", RunMode = RunMode.Async), Summary("Get an image from Rule34")]
        public async Task Rule34Search(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Rule34(), Context.Channel as ITextChannel, tags, TagId.Rule34);
        }

        [Command("E621", RunMode = RunMode.Async), Summary("Get an image from E621")]
        public async Task E621Search(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new E621(), Context.Channel as ITextChannel, tags, TagId.E621);
        }

        [Command("E926", RunMode = RunMode.Async), Summary("Get an image from E926")]
        public async Task E926Search(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new E926(), Context.Channel as ITextChannel, tags, TagId.E926);
        }

        /*[Command("Sakugabooru", RunMode = RunMode.Async), Summary("Get an image from Sakugabooru")]
        public async Task SakugabooruSearch(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Sakugabooru(), Context.Channel as ITextChannel, tags, TagId.Sakugabooru);
        }*/

        private static async Task PostImage(BooruSharp.Booru.Booru booru, ITextChannel chan, string[] tags, TagId id)
        {
            var result = await Features.NSFW.Booru.SearchBooru(!chan.IsNsfw, tags, booru);
            switch (result.error)
            {
                case Features.NSFW.Error.Booru.ChanNotNSFW:
                    await chan.SendMessageAsync(Base.Sentences.ChanIsNotNsfw(chan.GuildId));
                    break;

                case Features.NSFW.Error.Booru.InvalidFile:
                    await chan.SendMessageAsync(Sentences.InvalidExtension(chan.GuildId));
                    break;

                case Features.NSFW.Error.Booru.None:
                    await chan.SendMessageAsync("", false, new EmbedBuilder() { Color = Color.Blue, ImageUrl = result.answer.url }.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private static async Task<string[]> CorrectName(string query, BooruSharp.Booru.Booru booru)
        {
             char[] split = new char[] {
                '(', ')', '_', ',', '.', '[', ']', '#', '{', '}', '|'
            };
            BooruSharp.Search.Tag.SearchResult[] res = (await booru.GetTags(query)).ToArray();
            if (res.Length > 0)
                return (res.Select(delegate (BooruSharp.Search.Tag.SearchResult result) { return (result.name); }).ToArray());
            List<string> othersRes = new List<string>();
            string[] splitQuery = query.Split(split, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);
            foreach (string q in splitQuery)
                othersRes.AddRange((await booru.GetTags(q)).Select(delegate (BooruSharp.Search.Tag.SearchResult result) { return (result.name); }).ToList());
            othersRes = othersRes.Distinct().ToList();
            List<string> bestRes = new List<string>();
            foreach (string s in othersRes)
            {
                bool isEverywhere = true;
                foreach (string s2 in s.Split(split, int.MaxValue, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!splitQuery.Contains(s2))
                    {
                        isEverywhere = false;
                        break;
                    }
                }
                if (isEverywhere)
                    bestRes.Add(s);
            }
            if (bestRes.Count > 0)
                return (bestRes.ToArray());
            return (othersRes.ToArray());
        }

        public static async Task<Tuple<string, long, string[]>> GetImage(BooruSharp.Booru.Booru booru, string[] tags)
        {
            string fileName = booru.ToString() + DateTime.Now.ToString("HHmmssfff") + Program.p.rand.Next(int.MaxValue);
            long fileSize;
            var result = await booru.GetRandomImage(tags);
            DownloadFile(ref fileName, out fileSize, result.fileUrl.AbsoluteUri);
            return (new Tuple<string, long, string[]>(fileName, fileSize, result.tags));
        }

        public static void DownloadFile(ref string fileName, out long fileSize, string url)
        {
            using (WebClient wc = new WebClient())
            {
                fileName += "." + url.Split('.')[url.Split('.').Length - 1];
                wc.Headers.Add("User-Agent: Sanara");
                wc.DownloadFile(url, fileName);
                FileInfo file = new FileInfo(fileName);
                fileSize = file.Length;
            }
        }

        public static async Task<string> GetTagsInfos(BooruSharp.Booru.Booru booru, ulong guildId, string[] tags)
        {
            List<string> animeFrom = new List<string>();
            List<string> characs = new List<string>();
            List<string> artists = new List<string>();
            foreach (string t in tags)
            {
                try
                {
                    switch ((await booru.GetTag(t)).type)
                    {
                        case BooruSharp.Search.Tag.TagType.Artist:
                            artists.Add(t);
                            break;

                        case BooruSharp.Search.Tag.TagType.Copyright:
                            animeFrom.Add(t);
                            break;

                        case BooruSharp.Search.Tag.TagType.Character:
                            characs.Add(t);
                            break;
                    }
                }
                catch (BooruSharp.Search.InvalidTags)
                { }
            }
            return (WriteTagsInfos(animeFrom, characs, artists, guildId));
        }

        private static string FixName(string original)
        {
            original = Regex.Replace(original, @"\(([^\)]+)\)", "");
            string newName = "";
            bool isPreviousSpace = true;
            foreach (char c in original)
            {
                if (isPreviousSpace)
                    newName += char.ToUpper(c);
                else if (c == '_')
                    newName += ' ';
                else
                    newName += c;
                isPreviousSpace = (c == '_');
            }
            newName = newName.Trim();
            return newName;
        }

        private static string WriteTagsInfos(List<string> animeFrom, List<string> characs, List<string> artists, ulong guildId)
        {
            artists = artists.Select(x => FixName(x)).ToList();
            characs = characs.Select(x => FixName(x)).ToList();
            animeFrom = animeFrom.Select(x => FixName(x)).ToList();

            artists.RemoveAll(x => artists.Count(delegate (string s) { return (Utilities.CleanWord(s) == Utilities.CleanWord(x)); }) > 1);
            characs.RemoveAll(x => characs.Count(delegate (string s) { return (Utilities.CleanWord(s) == Utilities.CleanWord(x)); }) > 1);
            animeFrom.RemoveAll(x => animeFrom.Count(delegate (string s) { return (Utilities.CleanWord(s) == Utilities.CleanWord(x)); }) > 1);

            string finalStr = GetAnimes(animeFrom, guildId) + Environment.NewLine;
            finalStr += GetCharacs(characs, guildId) + Environment.NewLine;
            finalStr += GetArtists(artists, guildId);
            return (finalStr);
        }

        private static string GetCharacs(List<string> characs, ulong guildId)
        {
            string finalCharacs = "";
            if (characs.Count == 1)
                finalCharacs = characs[0];
            else if (characs.Count > 1)
            {
                bool doesContainTagMe = characs.Any(x => x == "Tagme" || x == "Character Request");
                for (int i = 0; i < characs.Count - 1; i++)
                {
                    if (finalCharacs.Length > 850)
                    {
                        finalCharacs += Sentences.AndSomeOthers(guildId);
                        break;
                    }
                    if (characs[i] != "Tagme" && characs[i] != "Character Request")
                        finalCharacs += characs[i] + ", ";
                }
                if (!doesContainTagMe)
                {
                    finalCharacs = finalCharacs.Substring(0, finalCharacs.Length - 2);
                    finalCharacs += " " + Base.Sentences.AndStr(guildId) + " " + characs[characs.Count - 1];
                }
                else
                {
                    if (characs[characs.Count - 1] == "Tagme" || characs[characs.Count - 1] == "Source Request"
                        || characs[characs.Count - 1] == "Copyright Request")
                    {
                        finalCharacs = finalCharacs.Substring(0, finalCharacs.Length - 2);
                        finalCharacs += Sentences.CharacterNotTagged(guildId);
                    }
                    else
                        finalCharacs += ", " + characs[characs.Count - 1] + Sentences.MoreNotTagged(guildId);
                }
            }
            if (finalCharacs == "")
                return (Sentences.CharacterTagUnknowed(guildId));
            if (characs.Count == 1 && (characs[0] == "Tagme" || characs[0] == "Character Request"))
                return (Sentences.CharacterNotTagged(guildId));
            if (characs.Count == 1)
                return (Sentences.CharacterIs(guildId) + finalCharacs + ".");
            return (Sentences.CharacterAre(guildId) + finalCharacs + ".");
        }

        private static string GetAnimes(List<string> animeFrom, ulong guildId)
        {
            string finalStr = "";
            if (animeFrom.Count == 1)
                finalStr = animeFrom[0];
            else if (animeFrom.Count > 1)
            {
                for (int i = 0; i < animeFrom.Count - 1; i++)
                {
                    if (finalStr.Length > 850)
                    {
                        finalStr += Sentences.AndSomeOthers(guildId);
                        break;
                    }
                    finalStr += animeFrom[i] + ", ";
                }
            }
            else
                return (Sentences.AnimeTagUnknowed(guildId));
            finalStr = finalStr.Substring(0, finalStr.Length - 2);
            finalStr += " " + Base.Sentences.AndStr(guildId) + " " + animeFrom[animeFrom.Count - 1];
            if (animeFrom.Count == 1 && animeFrom[0] == "Original")
                return (Sentences.AnimeFromOriginal(guildId));
            if (animeFrom.Count == 1 && (animeFrom[0] == "Tagme" || animeFrom[0] == "Source Request" || animeFrom[0] == "Copyright Request"))
                return (Sentences.AnimeNotTagged(guildId));
            if (finalStr != "")
                return (Sentences.AnimeFrom(guildId) + finalStr);
            return (Sentences.AnimeTagUnknowed(guildId));
        }

        private static string GetArtists(List<string> artists, ulong guildId)
        {
            if (artists.Count > 0)
            {
                string finalStr = Sentences.ArtistFrom(guildId);
                if (artists.Count > 1)
                {
                    finalStr += String.Join(", ", artists.Take(artists.Count - 1));
                    finalStr += " " + Base.Sentences.AndStr(guildId) + " " + artists[artists.Count - 1];
                }
                else
                    finalStr += artists[0];
                return (finalStr);
            }
            else
                return (Sentences.ArtistNotTagged(guildId));
        }
    }
}