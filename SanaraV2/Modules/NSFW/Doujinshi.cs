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
using System.Threading.Tasks;

namespace SanaraV2.Modules.NSFW
{
    public class Doujinshi : ModuleBase
    {
        Program p = Program.p;

        [Command("AdultVideo", RunMode = RunMode.Async), Alias("AV")]
        public async Task GetAdultVideo(params string[] args)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Doujinshi);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Doujinshi);
            var result = await Features.NSFW.Doujinshi.SearchAdultVideo(!(Context.Channel as ITextChannel).IsNsfw, args, Program.p.rand, Program.p.categories);
            switch (result.error)
            {
                case Features.NSFW.Error.Doujinshi.ChanNotNSFW:
                    await ReplyAsync(Base.Sentences.ChanIsNotNsfw(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Doujinshi.NotFound:
                    await ReplyAsync(Base.Sentences.TagsNotFound(Context.Guild.Id, args));
                    break;

                case Features.NSFW.Error.Doujinshi.None:
                    await ReplyAsync("", false, CreateFinalEmbed(result.answer, Context.Guild.Id));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [Command("Cosplay", RunMode = RunMode.Async)]
        public async Task GetCosplay(params string[] args)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Doujinshi);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Doujinshi);
            var result = await Features.NSFW.Doujinshi.SearchCosplay(!(Context.Channel as ITextChannel).IsNsfw, args, Program.p.rand);
            switch (result.error)
            {
                case Features.NSFW.Error.Doujinshi.ChanNotNSFW:
                    await ReplyAsync(Base.Sentences.ChanIsNotNsfw(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Doujinshi.NotFound:
                    await ReplyAsync(Base.Sentences.TagsNotFound(Context.Guild.Id, args));
                    break;

                case Features.NSFW.Error.Doujinshi.None:
                    await ReplyAsync("", false, CreateFinalEmbed(result.answer, Context.Guild.Id));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [Command("Doujinshi", RunMode = RunMode.Async), Summary("Give a random doujinshi using nhentai API")]
        public async Task GetNhentai(params string[] keywords)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Doujinshi);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Doujinshi);
            var result = await Features.NSFW.Doujinshi.SearchDoujinshi(!(Context.Channel as ITextChannel).IsNsfw, keywords, Program.p.rand);
            switch (result.error)
            {
                case Features.NSFW.Error.Doujinshi.ChanNotNSFW:
                    await ReplyAsync(Base.Sentences.ChanIsNotNsfw(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Doujinshi.NotFound:
                    await ReplyAsync(Base.Sentences.TagsNotFound(Context.Guild.Id, keywords));
                    break;

                case Features.NSFW.Error.Doujinshi.None:
                    await ReplyAsync("", false, CreateFinalEmbed(result.answer, Context.Guild.Id));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public Embed CreateFinalEmbed(Features.NSFW.Response.Doujinshi result, ulong guildId)
        {
            return new EmbedBuilder()
            {
                Color = new Color(255, 20, 147),
                Description = string.Join(", ", result.tags),
                Title = result.title,
                Url = result.url,
                ImageUrl = result.imageUrl,
                Footer = new EmbedFooterBuilder()
                {
                    Text = Sentences.ClickFull(guildId)
                }
            }.Build();
        }
    }
}