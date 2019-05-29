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
    public class Code : ModuleBase
    {
        [Command("Shell")]
        public async Task Shell(params string[] args)
        {

            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Code);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Code);
            var result = await Features.Tools.Code.Shell(args);
            switch (result.error)
            {
                case Features.Tools.Error.Shell.Help:
                    await ReplyAsync(Sentences.ShellHelp(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Shell.NotFound:
                    await ReplyAsync(Sentences.ShellNotFound(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Shell.None:
                    EmbedBuilder em = new EmbedBuilder()
                    {
                        Color = Color.Green,
                        Title = result.answer.title,
                        Url = result.answer.url
                    };
                    foreach (var ex in result.answer.explanations)
                        em.AddField(ex.Item1, ex.Item2);
                    await ReplyAsync("", false, em.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [Command("Color", RunMode = RunMode.Async), Summary("Display a RGB color")]
        public async Task SearchColor(params string[] args)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Code);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Code);
            var result = await Features.Tools.Code.SearchColor(args);
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
                        Description = Sentences.Rgb(Context.Guild.Id) + ": " + result.answer.discordColor.R + ", " + result.answer.discordColor.G + ", " + result.answer.discordColor.B + Environment.NewLine +
                        Sentences.Hex(Context.Guild.Id) + ": #" + result.answer.colorHex
                    }.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
