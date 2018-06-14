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
using System.Reflection;
using System.Threading.Tasks;

namespace SanaraV2
{
    public class CommunicationModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Help"), Summary("Give the help"), Alias("Commands")]
        public async Task help()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            await ReplyAsync("", false, Sentences.help(Context.Guild.Id, (Context.Channel as ITextChannel).IsNsfw));
        }

        [Command("Hi"), Summary("Answer with hi"), Alias("Hey", "Hello", "Hi!", "Hey!", "Hello!")]
        public async Task SayHi()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            await ReplyAsync(Sentences.hiStr(Context.Guild.Id));
        }

        [Command("Who are you"), Summary("Answer with who she is"), Alias("Who are you ?", "Who are you?")]
        public async Task WhoAreYou()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            await ReplyAsync(Sentences.whoIAmStr(Context.Guild.Id));
        }

        [Command("Infos"), Summary("Give informations about an user"), Alias("Info")]
        public async Task Infos(params string[] command)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            IGuildUser user;
            if (command.Length == 0)
                user = Context.User as IGuildUser;
            else
            {
                user = await Utilities.GetUser(Utilities.addArgs(command), Context.Guild);
                if (user == null)
                {
                    await ReplyAsync(Sentences.userNotExist(Context.Guild.Id));
                    return;
                }
            }
            await InfosUser(user);
        }

        [Command("BotInfos"), Summary("Give informations about the bot"), Alias("BotInfo", "InfosBot", "InfoBot")]
        public async Task BotInfos(params string[] command)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            await InfosUser(await Context.Channel.GetUserAsync(Sentences.myId) as IGuildUser);
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
            embed.AddField(Sentences.username(Context.Guild.Id), user.ToString(), true);
            if (user.Nickname != null)
                embed.AddField(Sentences.nickname(Context.Guild.Id), user.Nickname, true);
            embed.AddField(Sentences.accountCreation(Context.Guild.Id), user.CreatedAt.ToString(Sentences.dateHourFormat(Context.Guild.Id)), true);
            embed.AddField(Sentences.guildJoined(Context.Guild.Id), user.JoinedAt.Value.ToString(Sentences.dateHourFormat(Context.Guild.Id)), true);
            if (user == (await Context.Channel.GetUserAsync(Sentences.myId)))
            {
                embed.AddField(Sentences.creator(Context.Guild.Id), "Zirk#0001", true);
                embed.AddField(Sentences.latestVersion(Context.Guild.Id), new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTimeUtc.ToString(Sentences.dateHourFormat(Context.Guild.Id)), true);
                embed.AddField(Sentences.numberGuilds(Context.Guild.Id), p.client.Guilds.Count, true);
                embed.AddField(Sentences.uptime(Context.Guild.Id), Utilities.TimeSpanToString(DateTime.Now.Subtract(p.startTime), Context.Guild.Id));
                embed.AddField("GitHub", "https://github.com/Xwilarg/Sanara");
                embed.AddField(Sentences.website(Context.Guild.Id), "https://zirk.eu/sanara.html");
                embed.AddField(Sentences.officialGuild(Context.Guild.Id), "https://discordapp.com/invite/H6wMRYV");
            }
            embed.AddField(Sentences.roles(Context.Guild.Id), ((roles == "") ? (Sentences.noRole(Context.Guild.Id)) : (roles)));
            await ReplyAsync("", false, embed.Build());
        }
    }
}