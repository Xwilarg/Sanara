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
using Discord;
using System.Threading.Tasks;
using BooruSharp.Booru;
using System.Linq;
using SanaraV2.Modules.Base;

namespace SanaraV2.Modules.NSFW
{
    public class Booru : ModuleBase
    {
        Program p = Program.p;

        [Command("Safebooru", RunMode = RunMode.Async), Summary("Get an image from Safebooru")]
        public async Task SafebooruSearch(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Booru);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Safebooru(), Context.Channel as ITextChannel, tags, Context.Guild.Id);
        }

        [Command("Gelbooru", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task GelbooruSearch(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Booru);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Gelbooru(), Context.Channel as ITextChannel, tags, Context.Guild.Id);
        }

        [Command("Konachan", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task KonachanSearch(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Booru);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Konachan(), Context.Channel as ITextChannel, tags, Context.Guild.Id);
        }

        [Command("Rule34", RunMode = RunMode.Async), Summary("Get an image from Rule34")]
        public async Task Rule34Search(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Booru);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new Rule34(), Context.Channel as ITextChannel, tags, Context.Guild.Id);
        }

        [Command("E621", RunMode = RunMode.Async), Summary("Get an image from E621")]
        public async Task E621Search(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Booru);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new E621(), Context.Channel as ITextChannel, tags, Context.Guild.Id);
        }

        [Command("E926", RunMode = RunMode.Async), Summary("Get an image from E926")]
        public async Task E926Search(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Booru);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            await PostImage(new E926(), Context.Channel as ITextChannel, tags, Context.Guild.Id);
        }

        [Command("Tags", RunMode = RunMode.Async), Summary("Get informations about tags"), Alias("Tag")]
        public async Task TagsSearch(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Booru);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            var result = await Features.NSFW.Booru.SearchTags(tags);
            switch (result.error)
            {
                case Features.NSFW.Error.BooruTags.NotFound:
                    await ReplyAsync(Sentences.InvalidId(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.BooruTags.Help:
                    await ReplyAsync(Sentences.HelpId(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.BooruTags.None:
                    EmbedBuilder eb = new EmbedBuilder()
                    {
                        Color = result.answer.rating,
                        Title = result.answer.booruName,
                        ImageUrl = result.answer.imageUrl.AbsoluteUri,
                        Description = result.answer.width + " x " + result.answer.height +
                        (result.answer.width == result.answer.aspectRatio.Item1 ? "" : " (" + result.answer.aspectRatio.Item1 + ":" + result.answer.aspectRatio.Item2 + ")")
                    };
                    eb.AddField(((result.answer.sourceTags.Length > 1) ? (Sentences.Sources(Context.Guild.Id)) : (Sentences.Source(Context.Guild.Id))), "`" + string.Join(", ", result.answer.sourceTags) + "`");
                    eb.AddField(((result.answer.characTags.Length > 1) ? (Sentences.Characters(Context.Guild.Id)) : (Sentences.Character(Context.Guild.Id))), "`" + string.Join(", ", result.answer.characTags) + "`");
                    eb.AddField(((result.answer.artistTags.Length > 1) ? (Sentences.Artists(Context.Guild.Id)) : (Sentences.Artist(Context.Guild.Id))), "`" + string.Join(", ", result.answer.artistTags) + "`");
                    await ReplyAsync("", false, eb.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private static async Task PostImage(BooruSharp.Booru.Booru booru, ITextChannel chan, string[] tags, ulong guildId)
        {
            var result = await Features.NSFW.Booru.SearchBooru(!chan.IsNsfw, tags, booru, Program.p.rand);
            switch (result.error)
            {
                case Features.NSFW.Error.Booru.ChanNotNSFW:
                    await chan.SendMessageAsync(Base.Sentences.ChanIsNotNsfw(chan.GuildId));
                    break;

                case Features.NSFW.Error.Booru.NotFound:
                    await chan.SendMessageAsync(Base.Sentences.TagsNotFound(guildId, tags));
                    break;

                case Features.NSFW.Error.Booru.None:
                    if (!Utilities.IsImage(result.answer.url.Split('.').Last()))
                    {
                        await chan.SendMessageAsync(result.answer.url + Environment.NewLine + "*" + Sentences.ImageInfo(guildId, result.answer.saveId) + "*");
                    }
                    else
                    {
                        await chan.SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Color = result.answer.colorRating,
                            ImageUrl = result.answer.url,
                            Footer = new EmbedFooterBuilder()
                            {
                                Text = Sentences.ImageInfo(guildId, result.answer.saveId)
                            }
                        }.Build());
                    }
                    if (Program.p.sendStats)
                        await Program.p.UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("booru", booru.ToString().Split('.').Last().ToLower()) });
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}