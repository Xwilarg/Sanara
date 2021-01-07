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
            _isSendingStats = false;
        }

        private bool _isSendingStats;

        public void KeepSendStats()
        {
            if (_isSendingStats)
                return;
            _isSendingStats = true;
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(60000); // 1 minute

                    await UpdateElement(new Tuple<string, string>[] {
                        new Tuple<string, string>("serverCount", StaticObjects.Client.Guilds.Count.ToString())
                    });
                }
            });
        }

        // An user did a command
        public async Task AddNewMessageAsync()
        {
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("nbMsgs", "1") });
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("errors", "OK") });
        }

        public async Task AddNewCommandAsync(string name)
        {
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("commands", name) });
        }

        public async Task AddErrorAsync(System.Exception e)
        {
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("errors", e.GetType().ToString()) });
        }

        public async Task AddGameAsync(string name, string option)
        {
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("games", name + (option == null ? "" : "-" + option)) });
        }

        public async Task AddGamePlayerAsync(string name, string option, int playerCount)
        {
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("gamesPlayers", name + (option == null ? "" : "-" + option) + ";" + playerCount.ToString()) });
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
