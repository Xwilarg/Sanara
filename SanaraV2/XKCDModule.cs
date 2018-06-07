using Discord.Commands;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SanaraV2
{
    public class XKCDModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Xkcd", RunMode = RunMode.Async), Summary("Give XKCD commic")]
        public async Task randomXkcd(params string[] command)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Xkcd);
            int? myNb = null;
            if (command.Length > 0)
            {
                try
                {
                    myNb = Convert.ToInt32(Program.addArgs(command));
                }
                catch (FormatException)
                {
                    await ReplyAsync(Sentences.xkcdWrongArg(Context.Guild.Id));
                    return;
                }
                catch (OverflowException)
                {
                    await ReplyAsync(Sentences.xkcdWrongArg(Context.Guild.Id));
                    return;
                }
            }
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString("https://xkcd.com/info.0.json");
                int nbMax = Convert.ToInt32(Program.getElementXml("\"num\":", json, ','));
                int nb;
                if (myNb == null)
                    nb = p.rand.Next(nbMax) + 1;
                else
                {
                    if (myNb < 1 || myNb > nbMax)
                    {
                        await ReplyAsync(Sentences.xkcdWrongId(Context.Guild.Id, nbMax));
                        return;
                    }
                    nb = (int)myNb;
                }
                json = wc.DownloadString("https://xkcd.com/" + nb.ToString() + "/info.0.json");
                string dlUrl = Program.getElementXml("\"img\": \"", json, '"');
                string currName = "xkcd" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.ToString() + Context.User.Id.ToString() + "." + dlUrl.Split('.')[dlUrl.Split('.').Length - 1];
                wc.DownloadFile(dlUrl, currName);
                await Context.Channel.SendFileAsync(currName);
                File.Delete(currName);
            }
        }
    }
}