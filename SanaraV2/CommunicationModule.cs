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
           await ReplyAsync("", false, Sentences.help((Context.Channel as ITextChannel).IsNsfw));
        }

        [Command("Hi"), Summary("Answer with hi"), Alias("Hey", "Hello", "Hi!", "Hey!", "Hello!")]
        public async Task SayHi()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            await ReplyAsync(Sentences.hiStr);
        }

        [Command("Who are you"), Summary("Answer with who she is"), Alias("Who are you ?", "Who are you?")]
        public async Task WhoAreYou()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            await ReplyAsync(Sentences.whoIAmStr);
        }

        [Command("Infos"), Summary("Give informations about an user")]
        public async Task InfosUser(params string[] command)
        {
            System.Console.WriteLine(Program.addArgs(command));
            IGuildUser user;
            if (command.Length == 0)
                user = Context.User as IGuildUser;
            else
            {
                user = await Program.GetUser(Program.addArgs(command), Context.Guild);
                if (user == null)
                {
                    await ReplyAsync(Sentences.userNotExist);
                    return;
                }
            }
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
            embed.AddField("Username", user.ToString(), true);
            if (user.Nickname != null)
                embed.AddField("Nickname", user.Nickname, true);
            embed.AddField("Account creation", user.CreatedAt.ToString("dd/MM/yy HH:mm:ss"), true);
            embed.AddField("Server joined", user.JoinedAt.Value.ToString("dd/MM/yy HH:mm:ss"), true);
            if (user == (await Context.Channel.GetUserAsync(Sentences.myId)))
            {
                embed.AddField("Creator", "Zirk#0001", true);
                embed.AddField("GitHub", "https://github.com/Xwilarg/Sanara");
            }
            embed.AddField("Roles", ((roles == "") ? ("Vous n'avez aucun rôle") : (roles)));
            await ReplyAsync("", false, embed.Build());
        }
    }
}