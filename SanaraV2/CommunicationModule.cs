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
            string help = Sentences.help(Context.Channel.IsNsfw);
            EmbedBuilder embed = new EmbedBuilder()
            {
                Description = help,
                Color = Color.Purple,
            };
            await ReplyAsync("", false, embed);
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
    }
}