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

namespace SanaraV2.Modules.Tools
{
    public class Image : ModuleBase
    {
        Program p = Program.p;

        [Command("Color", RunMode = RunMode.Async), Summary("Display a RGB color")]
        public async Task SearchColor(params string[] args)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Image);
            var result = await Features.Tools.Image.SearchColor(args);
            switch (result.error)
            {
                case Features.Tools.Error.Image.InvalidArg:
                    await ReplyAsync(Sentences.HelpColor(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Image.InvalidColor:
                    await ReplyAsync(Sentences.InvalidColor(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Image.None:
                    await ReplyAsync("", false, new EmbedBuilder()
                    {
                        Title = result.answer.name,
                        Color = result.answer.discordColor,
                        ImageUrl = result.answer.colorUrl,
                        Description = "RGB: " + result.answer.discordColor.R + ", " + result.answer.discordColor.G + ", " + result.answer.discordColor.B + Environment.NewLine +
                        "Hex: #" + result.answer.colorHex
                    }.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}