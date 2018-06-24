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
using Discord.Commands;
using SanaraV2.Base;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SanaraV2.Entertainment
{
    public class XKCDModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Xkcd", RunMode = RunMode.Async), Summary("Give XKCD commic")]
        public async Task RandomXkcd(params string[] command)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Xkcd);
            int? myNb = null;
            if (command.Length > 0)
            {
                try
                {
                    myNb = Convert.ToInt32(Utilities.AddArgs(command));
                }
                catch (FormatException)
                {
                    await ReplyAsync(Sentences.XkcdWrongArg(Context.Guild.Id));
                    return;
                }
                catch (OverflowException)
                {
                    await ReplyAsync(Sentences.XkcdWrongArg(Context.Guild.Id));
                    return;
                }
            }
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString("https://xkcd.com/info.0.json");
                int nbMax = Convert.ToInt32(Utilities.GetElementXml("\"num\":", json, ','));
                int nb;
                if (myNb == null)
                    nb = p.rand.Next(nbMax) + 1;
                else
                {
                    if (myNb < 1 || myNb > nbMax)
                    {
                        await ReplyAsync(Sentences.XkcdWrongId(Context.Guild.Id, nbMax));
                        return;
                    }
                    nb = (int)myNb;
                }
                json = wc.DownloadString("https://xkcd.com/" + nb.ToString() + "/info.0.json");
                string dlUrl = Utilities.GetElementXml("\"img\": \"", json, '"');
                string currName = "xkcd" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.ToString() + Context.User.Id.ToString() + "." + dlUrl.Split('.')[dlUrl.Split('.').Length - 1];
                wc.DownloadFile(dlUrl, currName);
                await Context.Channel.SendFileAsync(currName);
                File.Delete(currName);
            }
        }
    }
}