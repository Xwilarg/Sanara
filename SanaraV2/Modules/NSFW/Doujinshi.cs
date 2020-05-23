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
using System.IO;
using System.Threading.Tasks;

namespace SanaraV2.Modules.NSFW
{
    public class Doujinshi : ModuleBase
    {
        Program p = Program.p;


        [Command("Subscribe Doujinshi")]
        public async Task Subscribe(params string[] args)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.AnimeManga);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.AnimeManga);
            if (!Tools.Settings.CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync(Base.Sentences.OnlyOwnerStr(Context.Guild.Id, Context.Guild.OwnerId));
            }
            else
            {
                var result = await Features.Entertainment.AnimeManga.Subscribe(Context.Guild, Program.p.db, args);
                switch (result.error)
                {
                    case Features.Entertainment.Error.Subscribe.Help:
                        await ReplyAsync(Entertainment.Sentences.SubscribeHelp(Context.Guild.Id));
                        break;

                    case Features.Entertainment.Error.Subscribe.InvalidChannel:
                        await ReplyAsync(Entertainment.Sentences.InvalidChannel(Context.Guild.Id));
                        break;

                    case Features.Entertainment.Error.Subscribe.None:
                        await ReplyAsync(Entertainment.Sentences.SubscribeDone(Context.Guild.Id, "doujinshi", result.answer.chan));
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        [Command("Unsubscribe Doujinshi")]
        public async Task Unsubcribe(params string[] args)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.AnimeManga);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.AnimeManga);

            if (!Tools.Settings.CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync(Base.Sentences.OnlyOwnerStr(Context.Guild.Id, Context.Guild.OwnerId));
            }
            else
            {
                var result = await Features.Entertainment.AnimeManga.Unsubscribe(Context.Guild, Program.p.db);
                switch (result.error)
                {
                    case Features.Entertainment.Error.Unsubscribe.NoSubscription:
                        await ReplyAsync(Entertainment.Sentences.NoSubscription(Context.Guild.Id));
                        break;

                    case Features.Entertainment.Error.Unsubscribe.None:
                        await ReplyAsync(Entertainment.Sentences.UnsubscribeDone(Context.Guild.Id, "doujinshi"));
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }


        [Command("Download doujinshi", RunMode = RunMode.Async)]
        public async Task GetDownloadDoujinshi(params string[] args)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Doujinshi);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Doujinshi);
            IMessage msg = null;
            var result = await Features.NSFW.Doujinshi.SearchDownloadDoujinshi(!(Context.Channel as ITextChannel).IsNsfw, args, async () =>
            {
                msg = await ReplyAsync("Preparing download, this might take some time...");
            });
            switch (result.error)
            {
                case Features.NSFW.Error.Download.ChanNotSafe:
                    await ReplyAsync(Base.Sentences.ChanIsNotNsfw(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Download.Help:
                    await ReplyAsync(Sentences.DownloadDoujinshiHelp(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Download.NotFound:
                    await ReplyAsync(Sentences.DownloadDoujinshiNotFound(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Download.None:
                    await GetDownloadResult(msg, result.answer);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [Command("Download cosplay", RunMode = RunMode.Async)]
        public async Task GetDownloadCosplay(params string[] args)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Doujinshi);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Doujinshi);
            IMessage msg = null;
            var result = await Features.NSFW.Doujinshi.SearchDownloadCosplay(!(Context.Channel as ITextChannel).IsNsfw, args, async () =>
            {
                msg = await ReplyAsync("Preparing download, this might take some time...");
            });
            switch (result.error)
            {
                case Features.NSFW.Error.Download.ChanNotSafe:
                    await ReplyAsync(Base.Sentences.ChanIsNotNsfw(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Download.Help:
                    await ReplyAsync(Sentences.DownloadCosplayHelp(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Download.NotFound:
                    await ReplyAsync(Sentences.DownloadCosplayNotFound(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Download.None:
                    await GetDownloadResult(msg, result.answer);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [Command("Download"), Priority(-1)]
        public async Task GetDownloadDefault(params string[] _)
        {
            await ReplyAsync(Sentences.DownloadHelp(Context.Guild.Id));
        }

        public async Task GetDownloadResult(IMessage msg, Features.NSFW.Response.Download answer)
        {
            FileInfo fi = new FileInfo(answer.filePath);
            if (fi.Length < 8000000)
                await Context.Channel.SendFileAsync(answer.filePath);
            else
            {
                if (Program.p.websiteUpload == null)
                    throw new NullReferenceException("File bigger than 8MB and websiteUpload key null");
                else
                {
                    string now = DateTime.Now.ToString("yyyyMMddHHmmss");
                    Directory.CreateDirectory(Program.p.websiteUpload + "/" + now);
                    File.Copy(answer.filePath, Program.p.websiteUpload + "/" + now + "/" + answer.id + ".zip");
                    await ReplyAsync(Program.p.websiteUrl + "/" + now + "/" + answer.id + ".zip" + Environment.NewLine + Sentences.DeleteTime(Context.Guild.Id, "10"));
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(600000); // 10 minutes
                        File.Delete(Program.p.websiteUpload + "/" + now + "/" + answer.id + ".zip");
                        Directory.Delete(Program.p.websiteUpload + "/" + now);
                    });
                }
            }
            await msg.DeleteAsync();
            File.Delete(answer.filePath);
            Directory.Delete(answer.directoryPath);
        }

        [Command("AdultVideo", RunMode = RunMode.Async), Alias("AV")]
        public async Task GetAdultVideo(params string[] args)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Doujinshi);
            if (Program.p.categories.Count == 2) // Tags not loaded
            {
                throw new NullReferenceException("An error occurred while loading tags, the command is therefore temporarily unavailable.");
            }
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
                    await ReplyAsync("", false, CreateFinalEmbed(result.answer, Context.Guild.Id, null));
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
                    await ReplyAsync("", false, CreateFinalEmbed(result.answer, Context.Guild.Id, Sentences.DownloadCosplayInfo));
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
                    await ReplyAsync("", false, CreateFinalEmbed(result.answer, Context.Guild.Id, Sentences.DownloadDoujinshiInfo));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public Embed CreateFinalEmbed(Features.NSFW.Response.Doujinshi result, ulong guildId, Func<ulong, string, string> downloadInfo)
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
                    Text = Sentences.ClickFull(guildId) + (downloadInfo == null ? "" : "\n\n" + downloadInfo(guildId, result.id.ToString()))
                }
            }.Build();
        }
    }
}