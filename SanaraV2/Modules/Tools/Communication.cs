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
using Discord.Net;
using SanaraV2.Modules.Base;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Tools
{
    public class Communication : ModuleBase
    {
        Program p = Program.p;

        [Command("Inspire")]
        public async Task Inspire(params string[] _)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Communication);
            await p.DoAction(Context.User, Program.Module.Communication);
            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = Color.Blue,
                ImageUrl = (await Features.Tools.Communication.Inspire()).answer.url
            }.Build());
        }

        [Command("Complete", RunMode = RunMode.Async)]
        public async Task Complete(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Communication);
            await p.DoAction(Context.User, Program.Module.Communication);
            IUserMessage message = null;
            string content = string.Join(" ", args);
            string oldContent = content;
            var result = await Features.Tools.Communication.Complete(args,
                (e) =>
                {
                    content += " " + e;
                },
                (e) =>
                {
                    var embed = new EmbedBuilder
                    {
                        Color = Color.Red,
                        Description = e
                    }.Build();
                    if (message != null)
                    {
                        message.ModifyAsync(x => x.Embed = embed).GetAwaiter().GetResult();
                    }
                    else
                    {
                        ReplyAsync("", false, embed).GetAwaiter().GetResult();
                    }
                },
                () =>
                {
                    if (message != null)
                    {
                        message.ModifyAsync(x => x.Embed = new EmbedBuilder
                        {
                            Color = Color.Blue,
                            Description = content
                        }.Build()).GetAwaiter().GetResult();
                    }
                    content = null;
                });
            switch (result.error)
            {
                case Features.Tools.Error.Complete.Help:
                    await ReplyAsync(Sentences.CompleteHelp(Context.Guild));
                    break;

                case Features.Tools.Error.Complete.None:
                    message = await ReplyAsync("", false, new EmbedBuilder
                    {
                        Color = Color.Blue,
                        Description = content,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = Sentences.CompleteWait(Context.Guild)
                        }
                    }.Build());
                    await Task.Run(async () =>
                    {
                        while (content != null)
                        {
                            await Task.Delay(2000);
                            if (content != null && oldContent != content)
                            {
                                await message.ModifyAsync(x => x.Embed = new EmbedBuilder
                                {
                                    Color = Color.Blue,
                                    Description = content,
                                    Footer = new EmbedFooterBuilder
                                    {
                                        Text = Sentences.CompleteWait(Context.Guild)
                                    }
                                }.Build());
                            }
                            oldContent = content;
                        }
                    });
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [Command("Calc")]
        public async Task Calc(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Communication);
            await p.DoAction(Context.User, Program.Module.Communication);
            DataTable table = new DataTable();
            try
            {
                await ReplyAsync(table.Compute(string.Join("", args), "").ToString());
            }
            catch (EvaluateException)
            {
                await ReplyAsync(Sentences.InvalidCalc(Context.Guild));
            }
            catch (SyntaxErrorException)
            {
                await ReplyAsync(Sentences.InvalidCalc(Context.Guild));
            }
        }

        [Command("Poll")]
        public async Task Poll(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Communication);
            await p.DoAction(Context.User, Program.Module.Communication);
            if (args.Length < 2)
            {
                await ReplyAsync(Sentences.PollHelp(Context.Guild));
                return;
            }
            if (args.Length > 10)
            {
                await ReplyAsync(Sentences.PollTooManyChoices(Context.Guild));
                return;
            }
            var emotes = new[] { new Emoji("1️⃣"), new Emoji("2️⃣"), new Emoji("3️⃣"), new Emoji("4️⃣"), new Emoji("5️⃣"), new Emoji("6️⃣"), new Emoji("7️⃣"), new Emoji("8️⃣"), new Emoji("9️⃣") };
            string str = "";
            int i = 0;
            foreach (string s in args.Skip(1))
            {
                str += emotes[i] + ": " + s + Environment.NewLine;
                i++;
            }
            var msg = await ReplyAsync("", false, new EmbedBuilder
            {
                Title = args[0],
                Description = str,
                Color = Color.Blue
            }.Build());
            await msg.AddReactionsAsync(emotes.Take(i).ToArray());
        }

        [Command("Infos"), Summary("Give informations about an user"), Alias("Info")]
        public async Task Infos(params string[] command)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync(Base.Sentences.CommandDontPm(Context.Guild));
                return;
            }
            Utilities.CheckAvailability(Context.Guild, Program.Module.Communication);
            await p.DoAction(Context.User, Program.Module.Communication);
            IGuildUser user;
            if (command.Length == 0)
                user = Context.User as IGuildUser;
            else
            {
                user = await Utilities.GetUser(Utilities.AddArgs(command), Context.Guild);
                if (user == null)
                {
                    await ReplyAsync(Sentences.UserNotExist(Context.Guild));
                    return;
                }
            }
            await InfosUser(user);
        }

        [Command("BotInfos"), Summary("Give informations about the bot"), Alias("BotInfo", "InfosBot", "InfoBot")]
        public async Task BotInfos(params string[] command)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync(Base.Sentences.CommandDontPm(Context.Guild));
                return;
            }
            Utilities.CheckAvailability(Context.Guild, Program.Module.Communication);
            await p.DoAction(Context.User, Program.Module.Communication);
            await InfosUser(await Context.Channel.GetUserAsync(Program.p.client.CurrentUser.Id) as IGuildUser);
        }

        [Command("Quote", RunMode = RunMode.Async), Summary("Quote a message")]
        public async Task Quote(params string[] id)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Communication);
            await p.DoAction(Context.User, Program.Module.Communication);
            IUser author = (id.Length == 0) ? (null) : (await Utilities.GetUser(string.Join("", id), Context.Guild));
            if (id.Length == 0 || author != null)
            {
                if (author == null)
                    author = Context.User;
                IMessage msg = (await Context.Channel.GetMessagesAsync().FlattenAsync()).Skip(1).ToList().Find(x => x.Author.Id == author.Id);
                if (msg == null)
                    await ReplyAsync(Sentences.QuoteNoMessage(Context.Guild));
                else
                    await ReplyAsync("", false, new EmbedBuilder()
                    {
                        Description = msg.Content
                    }.WithAuthor(msg.Author.ToString(), msg.Author.GetAvatarUrl()).WithFooter("The " + msg.CreatedAt.ToString(Base.Sentences.DateHourFormat(Context.Guild)) + " in " + msg.Channel.Name).Build());
            }
            else
            {
                IMessage msg = null;
                Match url = Regex.Match(id[0], "https:\\/\\/discordapp.com\\/channels\\/" + Context.Guild.Id + "\\/([0-9]{18})\\/([0-9]{18})");
                if (url.Success)
                {
                    ulong idChan, idMsg;
                    if (ulong.TryParse(url.Groups[1].Value, out idChan) && ulong.TryParse(url.Groups[2].Value, out idMsg))
                        msg = await (await Context.Guild.GetTextChannelAsync(idChan))?.GetMessageAsync(idMsg);
                }
                else
                {
                    ulong uId;
                    try
                    {
                        uId = Convert.ToUInt64(id[0]);
                    }
                    catch (FormatException)
                    {
                        await ReplyAsync(Sentences.QuoteInvalidId(Context.Guild));
                        return;
                    }
                    catch (OverflowException)
                    {
                        await ReplyAsync(Sentences.QuoteInvalidId(Context.Guild));
                        return;
                    }
                    msg = await Context.Channel.GetMessageAsync(uId);
                    if (msg == null)
                    {
                        foreach (IGuildChannel chan in await Context.Guild.GetChannelsAsync())
                        {
                            try
                            {
                                ITextChannel textChan = chan as ITextChannel;
                                if (textChan == null)
                                    continue;
                                msg = await textChan.GetMessageAsync(uId);
                                if (msg != null)
                                    break;
                            }
                            catch (HttpException) { }
                        }
                    }
                }
                if (msg == null)
                    await ReplyAsync(Sentences.QuoteInvalidId(Context.Guild));
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder()
                    {
                        Description = msg.Content
                    }.WithAuthor(msg.Author.ToString(), msg.Author.GetAvatarUrl()).WithFooter("The " + msg.CreatedAt.ToString(Base.Sentences.DateHourFormat(Context.Guild)) + " in " + msg.Channel.Name).Build());
                }
            }
        }

        public async Task InfosUser(IGuildUser user)
        {
            string roles = "";
            foreach (ulong roleId in user.RoleIds)
            {
                IRole role = Context.Guild.GetRole(roleId);
                if (role.Name == "@everyone")
                    continue;
                roles += role.Name + ", ";
            }
            if (roles != "")
                roles = roles.Substring(0, roles.Length - 2);
            EmbedBuilder embed = new EmbedBuilder
            {
                ImageUrl = user.GetAvatarUrl(),
                Color = Color.Purple
            };
            embed.AddField(Sentences.Username(Context.Guild), user.ToString(), true);
            if (user.Nickname != null)
                embed.AddField(Sentences.Nickname(Context.Guild), user.Nickname, true);
            embed.AddField(Sentences.AccountCreation(Context.Guild), user.CreatedAt.ToString(Base.Sentences.DateHourFormat(Context.Guild)), true);
            embed.AddField(Sentences.GuildJoined(Context.Guild), user.JoinedAt.Value.ToString(Base.Sentences.DateHourFormat(Context.Guild)), true);
            if (user == (await Context.Channel.GetUserAsync(Program.p.client.CurrentUser.Id)))
            {
                embed.AddField(Sentences.Creator(Context.Guild), "Zirk#0001", true);
                embed.AddField(Sentences.LatestVersion(Context.Guild), new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTimeUtc.ToString(Base.Sentences.DateHourFormat(Context.Guild)), true);
                embed.AddField(Sentences.NumberGuilds(Context.Guild), p.client.Guilds.Count, true);
                embed.AddField(Sentences.Uptime(Context.Guild), Utilities.TimeSpanToString(DateTime.Now.Subtract(p.startTime), Context.Guild));
                embed.AddField("GitHub", "https://github.com/Xwilarg/Sanara");
                embed.AddField(Sentences.Website(Context.Guild), "https://sanara.zirk.eu");
                embed.AddField(Sentences.InvitationLink(Context.Guild), "https://discordapp.com/oauth2/authorize?client_id=329664361016721408&permissions=3196928&scope=bot");
                embed.AddField(Sentences.OfficialGuild(Context.Guild), "https://discordapp.com/invite/H6wMRYV");
                embed.AddField("Discord Bot List", "https://discordbots.org/bot/329664361016721408");
                embed.AddField(Sentences.ProfilePicture(Context.Guild), "BlankSensei");
            }
            embed.AddField(Sentences.Roles(Context.Guild), ((roles == "") ? (Sentences.NoRole(Context.Guild)) : (roles)));
            await ReplyAsync("", false, embed.Build());
        }
    }
}
