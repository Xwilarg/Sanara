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

        [Command("Safebooru", RunMode = RunMode.Async), Summary("Get an image from Safebooru")]
        public async Task SafebooruSearch(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Safebooru(), Context.Channel as ITextChannel, tags);
        }

        [Command("Gelbooru", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task GelbooruSearch(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Gelbooru(), Context.Channel as ITextChannel, tags);
        }

        [Command("Konachan", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task KonachanSearch(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Konachan(), Context.Channel as ITextChannel, tags);
        }

        [Command("Rule34", RunMode = RunMode.Async), Summary("Get an image from Rule34")]
        public async Task Rule34Search(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Rule34(), Context.Channel as ITextChannel, tags);
        }

        [Command("E621", RunMode = RunMode.Async), Summary("Get an image from E621")]
        public async Task E621Search(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new E621(), Context.Channel as ITextChannel, tags);
        }

        [Command("E926", RunMode = RunMode.Async), Summary("Get an image from E926")]
        public async Task E926Search(params string[] tags)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new E926(), Context.Channel as ITextChannel, tags);
        }

        [Command("Tags", RunMode = RunMode.Async), Summary("Get informations about tags"), Alias("Tag")]
        public async Task TagsSearch(params string[] tags)
        {
            var result = await Features.NSFW.Booru.SearchTags(tags[0]);
            switch (result.error)
            {
                case Features.NSFW.Error.BooruTags.NotFound:
                    await ReplyAsync("This id does not exist");
                    break;

                case Features.NSFW.Error.BooruTags.None:
                    EmbedBuilder eb = new EmbedBuilder()
                    {
                        Color = Color.Blue
                    };
                    eb.AddField("Source" + ((result.answer.sourceTags.Length > 1) ? ("s") : ("")), "`" + string.Join(", ", result.answer.sourceTags) + "`");
                    eb.AddField("Character" + ((result.answer.characTags.Length > 1) ? ("s") : ("")), "`" + string.Join(", ", result.answer.characTags) + "`");
                    eb.AddField("Artist" + ((result.answer.artistTags.Length > 1) ? ("s") : ("")), "`" + string.Join(", ", result.answer.artistTags) + "`");
                    await ReplyAsync("", false, eb.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private static async Task PostImage(BooruSharp.Booru.Booru booru, ITextChannel chan, string[] tags)
        {
            var result = await Features.NSFW.Booru.SearchBooru(!chan.IsNsfw, tags, booru, Program.p.rand);
            switch (result.error)
            {
                case Features.NSFW.Error.Booru.ChanNotNSFW:
                    await chan.SendMessageAsync(Base.Sentences.ChanIsNotNsfw(chan.GuildId));
                    break;

                case Features.NSFW.Error.Booru.InvalidFile:
                    await chan.SendMessageAsync(Sentences.InvalidExtension(chan.GuildId));
                    break;

                case Features.NSFW.Error.Booru.NotFound:
                    await chan.SendMessageAsync(Base.Sentences.TagsNotFound(tags));
                    break;

                case Features.NSFW.Error.Booru.None:
                    await chan.SendMessageAsync("", false, new EmbedBuilder() {
                        Color = result.answer.colorRating,
                        ImageUrl = result.answer.url,
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = "Do the 'Tags' command with the id '" + result.answer.saveId + "' to have more informations about the image"
                        }
                    }.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
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
    }
}