
using Discord;
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
using Google;
using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Urlshortener.v1.Data;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV2
{
    public class GoogleShortenerModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Random url", RunMode = RunMode.Async), Summary("Give a random url from goo.gl")]
        public async Task randomPastebin()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.GoogleShortener);
            if (!(Context.Channel as ITextChannel).IsNsfw)
            {
                await ReplyAsync(Sentences.chanIsNotNsfw);
            }
            else
            {
                string result = "";
                string shortResult = "";
                int iteration = 1;
                string finalStr;
                UrlshortenerService service = new UrlshortenerService(new BaseClientService.Initializer
                {
                    ApiKey = File.ReadAllText("Keys/URLShortenerAPIKey.dat"),
                });
                using (WebClient wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    while (true)
                    {
                        finalStr = "";
                        for (int i = 0; i < 6; i++)
                        {
                            int nb = p.rand.Next(0, 62);
                            if (nb < 26)
                                finalStr += (char)(nb + 'a');
                            else if (nb < 52)
                                finalStr += (char)(nb + 'A' - 26);
                            else
                                finalStr += (char)(nb + '0' - 52);
                        }
                        try
                        {
                            Url response = await service.Url.Get("http://goo.gl/" + finalStr).ExecuteAsync();
                            result = response.LongUrl;
                            shortResult = response.Id;
                            break;
                        }
                        catch (GoogleApiException ex)
                        {
                            if (ex.HttpStatusCode == HttpStatusCode.NotFound) iteration++;
                            else if (ex.HttpStatusCode == HttpStatusCode.Forbidden)
                            {
                                await ReplyAsync(Sentences.tooManyRequests("goo.gl"));
                                return;
                            }
                        }
                        if (iteration == 500) break;
                    }
                }
                if (iteration == 500)
                    await ReplyAsync(Sentences.nothingAfterXIterations(500));
                else
                {
                    await ReplyAsync("I found something, here is the short URL: " + shortResult + Environment.NewLine
                        + ((result != null) ? ("It'll lead you here: " + result) : ("It will lead you nowhere since the URL was disabled...")));
                }
            }
        }
    }
}