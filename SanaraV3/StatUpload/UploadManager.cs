using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV3.StatUpload
{
    public class UploadManager
    {
        public UploadManager(string url, string token)
        {
            _url = url;
            _token = token;
        }

        public void KeepSendStats()
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(60000); // 1 minute

                // Biggest servers
                List<Tuple<string, int>> guilds = new List<Tuple<string, int>>();
                foreach (IGuild g in StaticObjects.Client.Guilds)
                {
                    guilds.Add(new Tuple<string, int>(g.Name, ((SocketGuild)g).MemberCount));
                }

                Tuple<string, int> biggest = null;
                string finalStr = "";
                for (int i = 0; i < 10; i++)
                {
                    foreach (var tuple in guilds)
                    {
                        if (biggest == null || tuple.Item2 > biggest.Item2)
                            biggest = tuple;
                    }
                    if (biggest == null)
                        break;
                    finalStr += GetName(biggest.Item1) + "|" + biggest.Item2 + "$";
                    guilds.Remove(biggest);
                    biggest = null;
                }

                await UpdateElement(new Tuple<string, string>[] {
                    new Tuple<string, string>("serverCount", StaticObjects.Client.Guilds.Count.ToString()),
                    new Tuple<string, string>("serversBiggest", finalStr)
                });
            });
        }

        // An user did a command
        public async Task AddNewMessage()
        {
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("nbMsgs", "1") });
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("errors", "OK") });
        }

        public async Task AddError(System.Exception e)
        {
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("errors", e.GetType().ToString()) });
        }

        // Clean name before sending it to the website for stats (| and $ are delimitators so we remove them)
        private string GetName(string name)
            => name.Replace("|", "").Replace("$", "");

        private async Task UpdateElement(Tuple<string, string>[] elems)
        {
            var values = new Dictionary<string, string> {
                           { "token", _token },
                           { "action", "add" },
                           { "name", "Sanara" }
                        };
            foreach (var elem in elems)
            {
                values.Add(elem.Item1, elem.Item2);
            }
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, _url);
            msg.Content = new FormUrlEncodedContent(values);

            try
            {
                await StaticObjects.HttpClient.SendAsync(msg);
            }
            catch (HttpRequestException) // TODO: We should probably retry
            { }
            catch (TaskCanceledException)
            { }
        }

        private string _url;
        private string _token;
    }
}
