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
using SanaraV2.Community;

namespace SanaraV2.Modules.NSFW
{
    public class Booru : ModuleBase
    {
        Program p = Program.p;

        [Command("Source", RunMode = RunMode.Async), Alias("Sauce", "AnimeSource", "SourceAnime")]
        public async Task Source(params string[] args)
        {
            Base.Utilities.CheckAvailability(Context.Guild, Program.Module.Booru);
            await p.DoAction(Context.User, Program.Module.Booru);
            if (Context.Message.Attachments.Count > 0)
                args = new[] { Context.Message.Attachments.ToArray()[0].Url };
            var result = await Features.NSFW.Booru.SearchSourceBooru(args);
            switch (result.error)
            {
                case Features.NSFW.Error.SourceBooru.None:
                    Color color;
                    float certitude = result.answer.compatibility;
                    if (certitude > 80f)
                        color = Color.Green;
                    else if (certitude > 50)
                        color = Color.Orange;
                    else
                        color = Color.Red;
                    if (Uri.IsWellFormedUriString(result.answer.url, UriKind.Absolute))
                    {
                        await ReplyAsync("", false, new EmbedBuilder
                        {
                            Description = result.answer.content,
                            ImageUrl = result.answer.url,
                            Color = color,
                            Footer = new EmbedFooterBuilder
                            {
                                Text = Entertainment.Sentences.Certitude(Context.Guild) + ": " + result.answer.compatibility + "%"
                            }
                        }.Build());
                    }
                    else
                    {
                        await ReplyAsync(result.answer.content + Environment.NewLine + result.answer.url + Environment.NewLine + Entertainment.Sentences.Certitude(Context.Guild) + ": " + result.answer.compatibility + "%");
                    }
                    break;

                case Features.NSFW.Error.SourceBooru.Help:
                    await ReplyAsync(Entertainment.Sentences.SourceHelp(Context.Guild));
                    break;

                case Features.NSFW.Error.SourceBooru.NotFound:
                    await ReplyAsync(Tools.Sentences.NotAnImage(Context.Guild));
                    break;

                case Features.NSFW.Error.SourceBooru.NotAnUrl:
                    await ReplyAsync(Entertainment.Sentences.NotAnUrl(Context.Guild));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [Command("Booru", RunMode = RunMode.Async)]
        public async Task BooruSearch(params string[] tags)
        {
            if (!(Context.Channel is ITextChannel) || ((ITextChannel)Context.Channel).IsNsfw)
                await GelbooruSearch(tags);
            else
                await SafebooruSearch(tags);
        }

        [Command("Safebooru", RunMode = RunMode.Async), Summary("Get an image from Safebooru")]
        public async Task SafebooruSearch(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild, Program.Module.Booru);
            await p.DoAction(Context.User, Program.Module.Booru);
            await PostImage(new Safebooru(), Context.Channel, tags, Context.Guild, Context.User.Id);
        }

        [Command("Gelbooru", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task GelbooruSearch(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild, Program.Module.Booru);
            await p.DoAction(Context.User, Program.Module.Booru);
            await PostImage(new Gelbooru(), Context.Channel, tags, Context.Guild, Context.User.Id);
        }

        [Command("Konachan", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task KonachanSearch(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild, Program.Module.Booru);
            await p.DoAction(Context.User, Program.Module.Booru);
            await PostImage(new Konachan(), Context.Channel, tags, Context.Guild, Context.User.Id);
        }

        [Command("Rule34", RunMode = RunMode.Async), Summary("Get an image from Rule34")]
        public async Task Rule34Search(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild, Program.Module.Booru);
            await p.DoAction(Context.User, Program.Module.Booru);
            await PostImage(new Rule34(), Context.Channel, tags, Context.Guild, Context.User.Id);
        }

        [Command("E621", RunMode = RunMode.Async), Summary("Get an image from E621")]
        public async Task E621Search(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild, Program.Module.Booru);
            await p.DoAction(Context.User, Program.Module.Booru);
            await PostImage(new E621(), Context.Channel, tags, Context.Guild, Context.User.Id);
        }

        [Command("E926", RunMode = RunMode.Async), Summary("Get an image from E926")]
        public async Task E926Search(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild, Program.Module.Booru);
            await p.DoAction(Context.User, Program.Module.Booru);
            await PostImage(new E926(), Context.Channel, tags, Context.Guild, Context.User.Id);
        }

        [Command("Tags", RunMode = RunMode.Async), Summary("Get informations about tags"), Alias("Tag")]
        public async Task TagsSearch(params string[] tags)
        {
            Base.Utilities.CheckAvailability(Context.Guild, Program.Module.Booru);
            await p.DoAction(Context.User, Program.Module.Booru);
            var result = await Features.NSFW.Booru.SearchTags(tags);
            switch (result.error)
            {
                case Features.NSFW.Error.BooruTags.NotFound:
                    await ReplyAsync(Sentences.InvalidId(Context.Guild));
                    break;

                case Features.NSFW.Error.BooruTags.Help:
                    await ReplyAsync(Sentences.HelpId(Context.Guild));
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
                    eb.AddField(((result.answer.sourceTags.Length > 1) ? (Sentences.Sources(Context.Guild)) : (Sentences.Source(Context.Guild))), "`" + string.Join(", ", result.answer.sourceTags) + "`");
                    eb.AddField(((result.answer.characTags.Length > 1) ? (Sentences.Characters(Context.Guild)) : (Sentences.Character(Context.Guild))), "`" + string.Join(", ", result.answer.characTags) + "`");
                    eb.AddField(((result.answer.artistTags.Length > 1) ? (Sentences.Artists(Context.Guild)) : (Sentences.Artist(Context.Guild))), "`" + string.Join(", ", result.answer.artistTags) + "`");
                    await ReplyAsync("", false, eb.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private static async Task PostImage(ABooru booru, IMessageChannel chan, string[] tags, IGuild guild, ulong userId)
        {
            
                var result = await Features.NSFW.Booru.SearchBooru(chan is ITextChannel ? !((ITextChannel)chan).IsNsfw : false, tags, booru, Program.p.rand);
            switch (result.error)
            {
                case Features.NSFW.Error.Booru.ChanNotNSFW:
                    await chan.SendMessageAsync(Base.Sentences.ChanIsNotNsfw(guild));
                    break;

                case Features.NSFW.Error.Booru.NotFound:
                    await chan.SendMessageAsync(Base.Sentences.TagsNotFound(guild, tags));
                    break;

                case Features.NSFW.Error.Booru.None:
                    IUserMessage msg;
                    if (!Utilities.IsImage(result.answer.url))
                    {
                        msg = await chan.SendMessageAsync(result.answer.url + Environment.NewLine + "*" + Sentences.ImageInfo(guild, result.answer.saveId) + "*");
                    }
                    else
                    {
                        msg = await chan.SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Color = result.answer.colorRating,
                            ImageUrl = result.answer.url,
                            Footer = new EmbedFooterBuilder()
                            {
                                Text = (result.answer.newTags != null ? "Some of your tags were invalid. The current search tags are: " + string.Join(", ", result.answer.newTags) + "\n\n" :
                                "") + (result.answer.saveId == null ? "" : Sentences.ImageInfo(guild, result.answer.saveId))
                            }
                        }.Build());
                    }
                    foreach (string t in tags)
                        await Program.p.cm.ProgressAchievementAsync(AchievementID.DoDifferentsBoorus, 1, t.GetHashCode().ToString(), msg, userId);
                    if (Program.p.sendStats)
                        await Program.p.UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("booru", booru.ToString().Split('.').Last().ToLower()) });
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}