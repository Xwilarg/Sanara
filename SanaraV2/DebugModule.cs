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
using System.Threading.Tasks;

namespace SanaraV2
{
    public class DebugModule : ModuleBase
    {
        Program p = Program.p;
        [Command("Print debug", RunMode = RunMode.Async), Summary("Get informations about the server and about each modules")]
        public async Task debugInfos()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Debug);
            string infosDebug;

            // GENERAL
            int userMet = 0;
            int userKnow = 0;
            string mostSpeakUser = "0";
            int nbMessages = 0;
            int totalMessages = 0;
            foreach (string s in Directory.GetFiles("Saves/Users/"))
            {
                userMet++;
                string[] details = File.ReadAllLines(s);
                if (details[2] != "No")
                {
                    userKnow++;
                    totalMessages += Convert.ToInt32(details[3]);
                    if (mostSpeakUser == "0" || Convert.ToInt32(details[3]) > nbMessages)
                    {
                        mostSpeakUser = details[0];
                        nbMessages = Convert.ToInt32(details[3]);
                    }
                }
            }

            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = Sentences.general(Context.Guild.Id),
                Color = Color.Purple,
            };

            embed.AddField(Sentences.creator(Context.Guild.Id), "Zirk#0001");
            embed.AddField(Sentences.creationDate(Context.Guild.Id), (await Context.Channel.GetUserAsync(Sentences.myId)).CreatedAt.ToString(Sentences.dateHourFormat(Context.Guild.Id)));
            embed.AddField(Sentences.messagesReceived(Context.Guild.Id), totalMessages + "\n" + Sentences.userMoreMessages(Context.Guild.Id, mostSpeakUser, nbMessages.ToString()));
            embed.AddField(Sentences.userKnown(Context.Guild.Id), userMet + "\n" + Sentences.alreadySpoke(Context.Guild.Id, userKnow.ToString()));
            embed.AddField(Sentences.guildsAvailable(Context.Guild.Id), p.client.Guilds.Count);

            await ReplyAsync("", false, embed.Build());

            infosDebug = "**Linguistic Module:**" + Environment.NewLine + Sentences.translation(Context.Guild.Id) + ": ";
            try
            {
                if (LinguistModule.getTranslation("cat", "fr", out _).Contains("chat")) infosDebug += Sentences.okStr(Context.Guild.Id);
                else infosDebug += "Error";
            } catch (Exception e) {
                infosDebug += e.ToString().Split(':')[0];
            }
            infosDebug += Environment.NewLine + Sentences.definition(Context.Guild.Id) + ": ";
            try
            {
                if (LinguistModule.getAllKanjis("cat", 0)[0].Contains("猫")) infosDebug += Sentences.okStr(Context.Guild.Id);
                else infosDebug += "Error";
            } catch (Exception e) {
                infosDebug += e.ToString().Split(':')[0];
            }

            infosDebug += Environment.NewLine + Environment.NewLine + "**Booru Module:**" + Environment.NewLine + "Safebooru: ";
            try
            {
                if (BooruModule.getBooruUrl(new BooruModule.Safebooru(), new string[] { "hibiki_(kantai_collection)" }).StartsWith("https://")) infosDebug += Sentences.okStr(Context.Guild.Id);
                else infosDebug += Sentences.errorStr(Context.Guild.Id);
            } catch (Exception e) {
                infosDebug += e.ToString().Split(':')[0];
            }
            infosDebug += Environment.NewLine + "Gelbooru: ";
            try
            {
                if (BooruModule.getBooruUrl(new BooruModule.Gelbooru(), new string[] { "hibiki_(kantai_collection)" }).StartsWith("https://")) infosDebug += Sentences.okStr(Context.Guild.Id);
                else infosDebug += Sentences.errorStr(Context.Guild.Id);
            } catch (Exception e) {
                infosDebug += e.ToString().Split(':')[0];
            }
            infosDebug += Environment.NewLine + "Konachan: ";
            try
            {
                if (BooruModule.getBooruUrl(new BooruModule.Konachan(), new string[] { "hibiki_(kancolle)" }).StartsWith("https://")) infosDebug += Sentences.okStr(Context.Guild.Id);
                else infosDebug += Sentences.errorStr(Context.Guild.Id);
            } catch (Exception e) {
                infosDebug += e.ToString().Split(':')[0];
            }
            infosDebug += Environment.NewLine + "Rule 34: ";
            try
            {
                if (BooruModule.getBooruUrl(new BooruModule.Rule34(), new string[] { "hibiki_(kancolle)" }).StartsWith("https://")) infosDebug += Sentences.okStr(Context.Guild.Id);
                else infosDebug += Sentences.errorStr(Context.Guild.Id);
            } catch (Exception e) {
                infosDebug += e.ToString().Split(':')[0];
            }

            infosDebug += Environment.NewLine + Environment.NewLine + "**Vn Module:**" + Environment.NewLine + "Vn: ";
            try
            {
                if ((await VndbModule.getVn("Sunrider: Mask of Arcadius")).Name == "Sunrider: Mask of Arcadius") infosDebug += Sentences.okStr(Context.Guild.Id);
                else infosDebug += Sentences.errorStr(Context.Guild.Id);
            }
            catch (Exception e)
            {
                infosDebug += e.ToString().Split(':')[0];
            }

            infosDebug += Environment.NewLine + Environment.NewLine + "**Google Shortener Module:**" + Environment.NewLine + Sentences.randomURL(Context.Guild.Id) + ": ";
            try
            {
                Tuple<string, string> result = await GoogleShortenerModule.getUrl();
                if (result == null || result.Item1.StartsWith("https://goo.gl/") || result.Item1.StartsWith("http://goo.gl/")) infosDebug += Sentences.okStr(Context.Guild.Id);
                else infosDebug += Sentences.errorStr(Context.Guild.Id);
            }
            catch (Exception e)
            {
                infosDebug += e.ToString().Split(':')[0];
            }

            embed = new EmbedBuilder()
            {
                Title = Sentences.unitTests(Context.Guild.Id),
                Description = infosDebug,
                Color = Color.Purple,
            };
            await ReplyAsync("", false, embed.Build());
        }
    }
}