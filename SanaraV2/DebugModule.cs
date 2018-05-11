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
            infosDebug = "**General**:" + Environment.NewLine;
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
            infosDebug += "Creator: Zirk#1001." + Environment.NewLine +
            "Creation date: " + (await Context.Channel.GetUserAsync(Sentences.myId)).CreatedAt.ToString("dd/MM/yy HH:mm:ss") + Environment.NewLine +
            "Messages received: " + totalMessages + "." + Environment.NewLine +
            "(The user who sent me more messages is " + mostSpeakUser + " with " + nbMessages + " messages)." + Environment.NewLine +
            "Users know: " + userMet + " (I already spoke with " + userKnow + " of them)." + Environment.NewLine +
            "Guilds available: "+ p.client.Guilds.Count + "." + Environment.NewLine;

            EmbedBuilder embed = new EmbedBuilder()
            {
                Description = infosDebug,
                Color = Color.Purple,
            };

            await ReplyAsync("", false, embed.Build());

            infosDebug = "**Linguistic Module:**" + Environment.NewLine + "Translation: ";
            try
            {
                if (LinguistModule.getTranslation("cat", "fr") == "chat") infosDebug += "OK";
                else infosDebug += "Error";
            } catch (Exception e) {
                infosDebug += e.ToString().Split(':')[0];
            }
            infosDebug += Environment.NewLine + "Definition: ";
            try
            {
                if (LinguistModule.getAllKanjis("cat")[0].Contains("猫")) infosDebug += "OK";
                else infosDebug += "Error";
            } catch (Exception e) {
                infosDebug += e.ToString().Split(':')[0];
            }

            infosDebug += Environment.NewLine + Environment.NewLine + "**Booru Module:**" + Environment.NewLine + "Safebooru: ";
            try
            {
                if (BooruModule.getBooruUrl(new BooruModule.Safebooru(), new string[] { "hibiki_(kantai_collection)" }).StartsWith("https://")) infosDebug += "OK";
                else infosDebug += "Error";
            } catch (Exception e) {
                infosDebug += e.ToString().Split(':')[0];
            }
            infosDebug += Environment.NewLine + "Gelbooru: ";
            try
            {
                if (BooruModule.getBooruUrl(new BooruModule.Gelbooru(), new string[] { "hibiki_(kantai_collection)" }).StartsWith("https://")) infosDebug += "OK";
                else infosDebug += "Error";
            } catch (Exception e) {
                infosDebug += e.ToString().Split(':')[0];
            }
            infosDebug += Environment.NewLine + "Konachan: ";
            try
            {
                if (BooruModule.getBooruUrl(new BooruModule.Konachan(), new string[] { "hibiki_(kancolle)" }).StartsWith("https://")) infosDebug += "OK";
                else infosDebug += "Error";
            } catch (Exception e) {
                infosDebug += e.ToString().Split(':')[0];
            }
            infosDebug += Environment.NewLine + "Rule 34: ";
            try
            {
                if (BooruModule.getBooruUrl(new BooruModule.Rule34(), new string[] { "hibiki_(kancolle)" }).StartsWith("https://")) infosDebug += "OK";
                else infosDebug += "Error";
            } catch (Exception e) {
                infosDebug += e.ToString().Split(':')[0];
            }

            infosDebug += Environment.NewLine + Environment.NewLine + "**Vndb Module:**" + Environment.NewLine + "Vn: ";
            try
            {
                if ((await VndbModule.getVn("Sunrider: Mask of Arcadius")).Name == "Sunrider: Mask of Arcadius") infosDebug += "OK";
                else infosDebug += "Error";
            }
            catch (Exception e)
            {
                infosDebug += e.ToString().Split(':')[0];
            }

            infosDebug += Environment.NewLine + Environment.NewLine + "**Google Shortener Module:**" + Environment.NewLine + "Vn: ";
            try
            {
                Tuple<string, string> result = await GoogleShortenerModule.getUrl();
                if (result == null || result.Item1.StartsWith("https://goo.gl/") || result.Item1.StartsWith("http://goo.gl/")) infosDebug += "OK";
                else infosDebug += "Error";
            }
            catch (Exception e)
            {
                infosDebug += e.ToString().Split(':')[0];
            }

            embed = new EmbedBuilder()
            {
                Title = "Unit tests:",
                Description = infosDebug,
                Color = Color.Purple,
            };
            await ReplyAsync("", false, embed.Build());
        }
    }
}