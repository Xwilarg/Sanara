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
using SanaraV2.Modules.Base;
using System.Threading.Tasks;

namespace SanaraV2.Games
{
    public class GameModule : ModuleBase
    {
        [Command("Play")]
        public async Task Play(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Game);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Game);
            ITextChannel chan = (ITextChannel)Context.Channel;
            await Program.p.gm.Play(args, chan);
        }

        [Command("Cancel")]
        public async Task Cancel(params string[] _)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Game);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Game);
            if (Program.p.gm.Cancel(Context.Channel.Id))
                await ReplyAsync(Sentences.ResetDone(Context.Guild.Id));
            else
                await ReplyAsync(Sentences.ResetNone(Context.Guild.Id));
        }
    }
}
