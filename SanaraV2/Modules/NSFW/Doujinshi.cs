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

        [Command("Download", RunMode = RunMode.Async)]
        public async Task GetDownload(params string[] args)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Doujinshi);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Doujinshi);
            IMessage msg = null;
            var result = await Features.NSFW.Doujinshi.SearchDownload(!(Context.Channel as ITextChannel).IsNsfw, args, async () =>
            {
                msg = await ReplyAsync("Preparing download, this might take some time...");
            });
            switch (result.error)
            {
                case Features.NSFW.Error.Download.ChanNotSafe:
                    await ReplyAsync(Base.Sentences.ChanIsNotNsfw(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Download.Help:
                    await ReplyAsync(Sentences.DownloadHelp(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Download.NotFound:
                    await ReplyAsync(Sentences.DownloadNotFound(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Download.None:
                    FileInfo fi = new FileInfo(result.answer.filePath);
                    if (fi.Length < 8000000)
                        await Context.Channel.SendFileAsync(result.answer.filePath);
                    else
                    {
                        if (Program.p.websiteUpload == null)
                            throw new NullReferenceException("File bigger than 8GB and websiteUpload key null");
                        else
                        {
                            string now = DateTime.Now.ToString("yyyyMMddHHmmss");
                            Directory.CreateDirectory(Program.p.websiteUpload + "/" + now);
                            File.Copy(result.answer.filePath, Program.p.websiteUpload + "/" + now + "/" + result.answer.id + ".zip");
                            await Context.Channel.SendFileAsync(Program.p.websiteUpload + "/" + now + "/" + result.answer.id + ".zip", Sentences.DeleteTime(Context.Guild.Id, "10"));
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(600000); // 10 minutes
                                File.Delete(Program.p.websiteUpload + "/" + now + "/" + result.answer.id + ".zip");
                                Directory.Delete(Program.p.websiteUpload + "/" + now);
                            });
                        }
                    }
                    await msg.DeleteAsync();
                    File.Delete(result.answer.filePath);
                    Directory.Delete(result.answer.directoryPath);
                    break;

                default:
                    throw new NotImplementedException();
            }
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
                    Text = Sentences.ClickFull(guildId) + (result.id == 0 ? "" : "\n\n" + Sentences.DownloadInfo(guildId, result.id.ToString()))
                }
            }.Build();
        }
    }
}