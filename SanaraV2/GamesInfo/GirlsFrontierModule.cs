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
using SanaraV2.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SanaraV2.GamesInfo
{
    public class GirlsFrontierModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Girls Frontline", RunMode = RunMode.Async), Summary("Get informations about a Girls Frontline character"), Alias("GirlsFrontline")]
        public async Task Charac(params string[] shipNameArr) // TODO Refactor (duplicated of Kancolle command)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.GirlsFrontier);
            if (shipNameArr.Length == 0)
            {
                await ReplyAsync(Sentences.GirlsFrontlineHelp(Context.Guild.Id));
                return;
            }
            string shipName = Utilities.AddArgs(shipNameArr);
            IGuildUser me = await Context.Guild.GetUserAsync(Base.Sentences.myId);
            string id, thumbnail;
            if (!Wikia.GetCharacInfos(shipName, out id, out thumbnail, Wikia.WikiaType.GirlsFrontline))
            {
                await ReplyAsync(Sentences.TDollDontExist(Context.Guild.Id));
                return;
            }
            string thumbnailPath = null;
            List<string> finalStr = FillGirlsFrontlineInfos(id, Context.Guild.Id);
            if (me.GuildPermissions.AttachFiles)
            {
                thumbnailPath = Wikia.DownloadCharacThumbnail(thumbnail);
                await Context.Channel.SendFileAsync(thumbnailPath);
            }
            foreach (string s in finalStr)
                await ReplyAsync(s);
            if (me.GuildPermissions.AttachFiles)
                File.Delete(thumbnailPath);
        }
        
        public static List<string> FillGirlsFrontlineInfos(string shipId, ulong guildId)
        {
            return (Wikia.FillWikiaInfos(shipId, guildId, new Dictionary<string, Func<ulong, string>>
            {
                { "Model & Historical Information", Sentences.ModelAndHistoricalInformation },
                { "Basic", Sentences.Basic },
                { "Technical", Sentences.Technical }
            }, Wikia.WikiaType.GirlsFrontline));
        }
    }
}