using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Attribute;
using Sanara.CustomClass;
using Sanara.Exception;
using Sanara.Subscription.Tags;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.VisualNovel;

namespace Sanara.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadJapaneseHelp()
        {
            _help.Add(("Entertainment", new Help("Japanese", "Manga", new[] { new Argument(ArgumentType.Mandatory, "name") }, "Get information about a manga.", Array.Empty<string>(), Restriction.None, "Manga made in abyss")));
            _help.Add(("Entertainment", new Help("Japanese", "Anime", new[] { new Argument(ArgumentType.Mandatory, "name") }, "Get information about an anime.", Array.Empty<string>(), Restriction.None, "Anime nichijou")));
            _help.Add(("Entertainment", new Help("Japanese", "Light Novel", new[] { new Argument(ArgumentType.Mandatory, "name") }, "Get information about a light novel.", new string[]{ "LN" }, Restriction.None, "Light novel so I'm a spider so what")));
            _help.Add(("Entertainment", new Help("Japanese", "Visual Novel", new[] { new Argument(ArgumentType.Mandatory, "name") }, "Get information about a visual novel.", new string[] { "VN" }, Restriction.None, "Visual novel katawa shoujo")));
            _help.Add(("Entertainment", new Help("Japanese", "Subscribe anime", new[] { new Argument(ArgumentType.Mandatory, "text channel") }, "Get information on all new anime in to a channel.", Array.Empty<string>(), Restriction.AdminOnly, null)));
            _help.Add(("Entertainment", new Help("Japanese", "Unsubscribe anime", Array.Empty<Argument>(), "Remove an anime subscription.", Array.Empty<string>(), Restriction.AdminOnly, null)));
            _help.Add(("Entertainment", new Help("Japanese", "Source", new[] { new Argument(ArgumentType.Mandatory, "image") }, "Get the source of an image.", Array.Empty<string>(), Restriction.None, "Source https://sanara.zirk.eu/img/Gallery/001_01.jpg")));
            _help.Add(("Entertainment", new Help("Japanese", "Drama", new[] { new Argument(ArgumentType.Mandatory, "name") }, "Get information about an Asian drama.", Array.Empty<string>(), Restriction.None, "Drama my mister")));
        }
    }
}

namespace Sanara.Module.Entertainment
{
    public sealed class JapaneseModule : ISubmodule
    {
        public Help.SubmoduleInfo GetInfo()
        {
            return new("Japanese", "Commands related to Japanese culture");
        }

        public CommandInfo[] GetCommands()
        {
            throw new NotImplementedException();
        }
        public static async Task<JToken> SearchMediaAsync(JapaneseMedia media, string query, bool onlyExactMatch = false)
        {
            // Authentification is required to see NSFW content
            if (StaticObjects.KitsuAuth != null)
            {
                if (StaticObjects.KitsuAccessToken == null) // Get access token
                {
                    var answer = await StaticObjects.HttpClient.SendAsync(StaticObjects.KitsuAuth);
                    var authJson = JsonConvert.DeserializeObject<JObject>(await answer.Content.ReadAsStringAsync());
                    StaticObjects.KitsuAccessToken = authJson["access_token"].Value<string>();
                    StaticObjects.KitsuRefreshDate = DateTime.Now.AddSeconds(authJson["expires_in"].Value<int>());
                    StaticObjects.KitsuRefreshToken = authJson["refresh_token"].Value<string>();
                }
                else if (DateTime.Now > StaticObjects.KitsuRefreshDate) // Access token expired, need to refresh it
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://kitsu.io/api/oauth/token")
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "grant_type", "refresh_token" },
                            { "refresh_token", StaticObjects.KitsuRefreshToken }
                        })
                    };
                    var answer = await StaticObjects.HttpClient.SendAsync(request);
                    var authJson = JsonConvert.DeserializeObject<JObject>(await answer.Content.ReadAsStringAsync());
                    StaticObjects.KitsuAccessToken = authJson["access_token"].Value<string>();
                    StaticObjects.KitsuRefreshDate = DateTime.Now.AddSeconds(authJson["expires_in"].Value<int>());
                    StaticObjects.KitsuRefreshToken = authJson["refresh_token"].Value<string>();
                }
            }

            StaticObjects.KitsuHttpClient.DefaultRequestHeaders.Authorization = new("Bearer", StaticObjects.KitsuAccessToken);
            // For anime we need to contact the anime endpoint, manga and light novels are on the manga endpoint
            // The limit=5 is because we only take the 5 firsts results to not end up with things that are totally unrelated
            var json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.KitsuHttpClient.GetStringAsync("https://kitsu.io/api/edge/" + (media == JapaneseMedia.Anime? "anime" : "manga") + "?page[limit]=5&filter[text]=" + HttpUtility.UrlEncode(query)));

            StaticObjects.KitsuHttpClient.DefaultRequestHeaders.Authorization = null;

            var data = json["data"].Value<JArray>();

            // Filter data depending of wanted media
            JToken[] allData;
            if (media == JapaneseMedia.Manga)
                allData = data.Where(x => x["attributes"]["subtype"].Value<string>() != "novel").ToArray();
            else if (media == JapaneseMedia.LightNovel)
                allData = data.Where(x => x["attributes"]["subtype"].Value<string>() == "novel").ToArray();
            else
                allData = data.ToArray();

            if (allData.Length == 0)
                throw new CommandFailed("Nothing was found with this name.");

            string cleanName = Utils.CleanWord(query); // Cleaned word for comparaisons

            // If we can find an exact match, we go with that
            string upperName = query.ToUpper();
            foreach (var elem in allData)
            {
                if (elem["attributes"]["canonicalTitle"].Value<string>().ToUpper() == upperName ||
                    elem["attributes"]["titles"]["en"] != null && elem["attributes"]["titles"]["en"].Value<string>()?.ToUpper() == upperName ||
                    elem["attributes"]["titles"]["en_jp"] != null && elem["attributes"]["titles"]["en_jp"].Value<string>()?.ToUpper() == upperName ||
                    elem["attributes"]["titles"]["en_us"] != null && elem["attributes"]["titles"]["en_us"].Value<string>()?.ToUpper() == upperName)
                {
                    return elem;
                }
            }

            if (onlyExactMatch) // Used by subscriptions (because we want to be 100% sure to not match the wrong anime)
                throw new CommandFailed("Nothing was found with this name");

            // Else we try to find something that somehow match
            foreach (var elem in allData)
            {
                if (Utils.CleanWord(elem["attributes"]["canonicalTitle"].Value<string>()).Contains(cleanName) ||
                    elem["attributes"]["titles"]["en"] != null && Utils.CleanWord(elem["attributes"]["titles"]["en"].Value<string>() ?? "").Contains(cleanName) ||
                    elem["attributes"]["titles"]["en_jp"] != null && Utils.CleanWord(elem["attributes"]["titles"]["en_jp"].Value<string>() ?? "").Contains(cleanName) ||
                    elem["attributes"]["titles"]["en_us"] != null && Utils.CleanWord(elem["attributes"]["titles"]["en_us"].Value<string>() ?? "").Contains(cleanName))
                {
                    // We would rather find the episodes and not some OVA/ONA
                    // We don't filter them before so we can fallback on them if we find nothing else
                    if (media == JapaneseMedia.Anime && elem["attributes"]["subtype"].Value<string>() != "TV")
                        continue;

                    return elem;
                }
            }

            // Otherwise, we just fall back on the first result available
            return allData[0];
        }
        /*
[Command("Source", RunMode = RunMode.Async)]
public async Task SourceAsync(ImageLink img)
{
   await ParseSourceAsync(img.Link);
}

[Command("Source", RunMode = RunMode.Async)]
public async Task SourceAsync()
{
   if (Context.Message.Attachments.Count > 0 && Utils.IsImage(Path.GetExtension(Context.Message.Attachments.First().Url)))
       await ParseSourceAsync(Context.Message.Attachments.First().Url);
   else
       throw new CommandFailed("This command have some invalid parameters.");
}

private async Task ParseSourceAsync(string link)
{
   var html = await StaticObjects.HttpClient.GetStringAsync("https://saucenao.com/search.php?db=999&url=" + Uri.EscapeDataString(link));
   if (!html.Contains("<div id=\"middle\">"))
       throw new CommandFailed("I didn't find the source of this image.");
   var subHtml = html.Split(new[] { "<td class=\"resulttablecontent\">" }, StringSplitOptions.None)[1];

   var compatibility = float.Parse(Regex.Match(subHtml, "<div class=\"resultsimilarityinfo\">([0-9]{2,3}\\.[0-9]{1,2})%<\\/div>").Groups[1].Value, CultureInfo.InvariantCulture);
   var content = Utils.CleanHtml(subHtml.Split(new[] { "<div class=\"resultcontentcolumn\">" }, StringSplitOptions.None)[1].Split(new[] { "</div>" }, StringSplitOptions.None)[0]);
   var url = Regex.Match(html, "<img title=\"Index #[^\"]+\"( raw-rating=\"[^\"]+\") src=\"(https:\\/\\/img[0-9]+.saucenao.com\\/[^\"]+)\"").Groups[2].Value;

   await ReplyAsync(embed: new EmbedBuilder
   {
       ImageUrl = url,
       Description = content,
       Color = Color.Green,
       Footer = new EmbedFooterBuilder
       {
           Text = $"Certitude: {compatibility}%"
       }
   }.Build());
}

[Command("Subscribe anime"), RequireAdmin]
public async Task SubscribeAnimeAsync(ITextChannel chan, params string[] tags)
{
   await StaticObjects.Db.SetSubscriptionAsync(Context.Guild.Id, "anime", chan, new AnimeTags(tags, true));
   await ReplyAsync($"You subscribed for anime to {chan.Mention}.");
}

[Command("Unsubscribe anime"), RequireAdmin]
public async Task UnsubscribeAnimeAsync()
{
   if (!await StaticObjects.Db.HasSubscriptionExistAsync(Context.Guild.Id, "anime"))
       await ReplyAsync("There is no active anime subscription.");
   else
       await StaticObjects.Db.RemoveSubscriptionAsync(Context.Guild.Id, "anime");
}

private HttpWebResponse GetHttpResponse(HttpWebRequest request)
{
   try
   {
       return (HttpWebResponse)request.GetResponse();
   }
   catch (WebException ex)
   {
       if (ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError)
           throw;

       return (HttpWebResponse)ex.Response;
   }
}

[Command("Visual Novel"), Alias("VN")]
public async Task VisualNovel([Remainder] string name)
{
   string originalName = name;
   name = Utils.CleanWord(name);
   HttpWebRequest http = (HttpWebRequest)WebRequest.Create("https://vndb.org/v/all?sq=" + HttpUtility.UrlEncode(originalName).Replace("%20", "+"));
   http.AllowAutoRedirect = false;

   string html;
   // HttpClient doesn't really look likes to handle redirection properly
   using HttpWebResponse response = GetHttpResponse(http);
   using Stream stream = response.GetResponseStream();
   using StreamReader reader = new(stream);
   html = reader.ReadToEnd();

   uint id = 0;
   if (response.StatusCode == HttpStatusCode.OK) // Search succeed
   {
       // Parse HTML and go though every VN, check the original name and translated name to get the VN id
       // TODO: Use string length for comparison
       MatchCollection matches = Regex.Matches(html, "<a href=\"\\/v([0-9]+)\" title=\"([^\"]+)\">([^<]+)<\\/a>");
       foreach (Match match in matches)
       {
           string titleName = Utils.CleanWord(match.Groups[3].Value);
           string titleNameBase = match.Groups[2].Value;
           if (id == 0 && (titleName.Contains(name) || titleNameBase.Contains(originalName)))
               id = uint.Parse(match.Groups[1].Value);
           if (titleName == name || titleNameBase == name)
           {
               id = uint.Parse(match.Groups[1].Value);
               break;
           }
       }
       // If no matching name, we take the first one in the search list, if none these NotFound
       if (id == 0)
       {
           if (matches.Count == 0)
               throw new CommandFailed("Nothing was found with this name.");
           id = uint.Parse(matches[0].Groups[1].Value);
       }
   }
   else // Only one VN found, search is trying to redirect us
   {
       id = uint.Parse(response.Headers["Location"][2..]); // VN ID is the location which is in format /VXXXX, XXXX being our numbers
   }


   var vn = (await StaticObjects.VnClient.GetVisualNovelAsync(VndbFilters.Id.Equals(id), VndbFlags.FullVisualNovel)).ToArray()[0];

   var embed = new EmbedBuilder()
   {
       Title = vn.OriginalName == null ? vn.Name : vn.OriginalName + " (" + vn.Name + ")",
       Url = "https://vndb.org/v" + vn.Id,
       ImageUrl = Context.Channel is ITextChannel channel && !channel.IsNsfw && (vn.ImageRating.SexualAvg > 1 || vn.ImageRating.ViolenceAvg > 1) ? null : vn.Image,
       Description = vn.Description == null ? null : Regex.Replace(vn.Description.Length > 1000 ? vn.Description[0..1000]  + " [...]" : vn.Description, "\\[url=([^\\]]+)\\]([^\\[]+)\\[\\/url\\]", "[$2]($1)"),
       Color = Color.Blue
   };
   embed.AddField("Available in english?", vn.Languages.Contains("en") ? "Yes" : "No", true);
   embed.AddField("Available on Windows?", vn.Platforms.Contains("win") ? "Yes" : "No", true);
   string length = "???";
   switch (vn.Length)
   {
       case VisualNovelLength.VeryShort: length = "<2 Hours"; break;
       case VisualNovelLength.Short: length = "2 - 10  Hours"; break;
       case VisualNovelLength.Medium: length = "10 - 30 Hours"; break;
       case VisualNovelLength.Long: length = "30 - 50 Hours"; break;
       case VisualNovelLength.VeryLong: length = "\\> 50 Hours"; break;
   }
   embed.AddField("Length", length, true);
   embed.AddField("Vndb Rating", vn.Rating + " / 10", true);
   string releaseDate;
   if (vn.Released?.Year == null)
       releaseDate = "TBA";
   else
   {
       releaseDate = vn.Released.Year.Value.ToString();
       if (!vn.Released.Month.HasValue)
           releaseDate = AddZero(vn.Released.Month.Value) + "/" + releaseDate;
       if (!vn.Released.Day.HasValue)
           releaseDate = AddZero(vn.Released.Day.Value) + "/" + releaseDate;
   }
   embed.AddField("Release Date", releaseDate, true);
   await ReplyAsync(embed: embed.Build());
}

private string AddZero(uint val)
   => val < 10 ? "0" + val : val.ToString();

[Command("Manga", RunMode = RunMode.Async)]
public async Task MangaAsync([Remainder] string name)
{
   await PostAnimeEmbedAsync(JapaneseMedia.MANGA, await SearchMediaAsync(JapaneseMedia.MANGA, name));
}

[Command("Anime", RunMode = RunMode.Async)]
public async Task AnimeAsync([Remainder] string name)
{
   await PostAnimeEmbedAsync(JapaneseMedia.ANIME, await SearchMediaAsync(JapaneseMedia.ANIME, name));
}

[Command("Light novel", RunMode = RunMode.Async), Alias("LN")]
public async Task LightNovelAsync([Remainder] string name)
{
   await PostAnimeEmbedAsync(JapaneseMedia.LIGHT_NOVEL, await SearchMediaAsync(JapaneseMedia.LIGHT_NOVEL, name));
}

[Command("Drama", RunMode = RunMode.Async)]
public async Task DramaAsync([Remainder] string name)
{
   var request = new HttpRequestMessage()
   {
       RequestUri = new Uri("https://api.mydramalist.com/v1/search/titles?q=" + HttpUtility.UrlEncode(name)),
       Method = HttpMethod.Post
   };
   request.Headers.Add("mdl-api-key", StaticObjects.MyDramaListApiKey);

   var response = await StaticObjects.HttpClient.SendAsync(request);
   var searchResults = JsonConvert.DeserializeObject<JArray>(await response.Content.ReadAsStringAsync());

   if (searchResults.Count == 0)
       throw new CommandFailed("Nothing was found with this name.");

   var id = searchResults.First().Value<int>("id");
   var drama = await GetDramaAsync(id);

   var embed = new EmbedBuilder()
   {
       Title = drama.Value<string>("original_title"),
       Description = drama.Value<string>("synopsis"),
       Color = new Color(0, 97, 157),
       Url = drama.Value<string>("permalink"),
       ImageUrl = drama["images"].Value<string>("poster")
   };

   embed.AddField("English Title", drama.Value<string>("title"));
   embed.AddField("Episode Count", drama.Value<int>("episodes"), true);

   if(drama["released"] != null)
   {
       embed.AddField("Released", drama.Value<string>("released"), true);
   }
   else
   {
       embed.AddField("Air date", drama.Value<string>("aired_start") + ((drama["aired_end"] != null) ? " - " + drama.Value<string>("aired_end") : ""), true);
   }

   embed.AddField("Country", drama.Value<string>("country"), true);
   embed.AddField("Audiance Warning", drama.Value<string>("certification"), true);

   if (drama["votes"] != null)
   {
       embed.AddField("MyDramaList User Rating", drama.Value<double>("rating") + "/10", true);
   }

   await ReplyAsync(embed: embed.Build());
}

public static async Task<JObject> GetDramaAsync(int id)
{
   var request = new HttpRequestMessage()
   {
       RequestUri = new Uri("https://api.mydramalist.com/v1/titles/" + id),
       Method = HttpMethod.Get
   };
   request.Headers.Add("mdl-api-key", StaticObjects.MyDramaListApiKey);

   var response = await StaticObjects.HttpClient.SendAsync(request);
   return JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
}

private async Task PostAnimeEmbedAsync(JapaneseMedia media, JToken token)
{
   token = token["attributes"];

   if (Context.Channel is ITextChannel channel && !channel.IsNsfw && token["nsfw"] != null && token["nsfw"].Value<bool>())
       throw new CommandFailed("The result of your search was NSFW and thus, can only be shown in a NSFW channel.");

   var description = "";
   if (token["synopsis"].Value<string>() != null)
       description = token["synopsis"].Value<string>().Length > 1000 ? token["synopsis"].Value<string>().Substring(0, 1000) + " [...]" : token["synopsis"].Value<string>(); // Description that fill the whole screen are a pain
   var embed = new EmbedBuilder
   {
       Title = token["canonicalTitle"].Value<string>(),
       Color = token["nsfw"] == null ? Color.Green : (token["nsfw"].Value<bool>() ? new Color(255, 20, 147) : Color.Green),
       Url = "https://kitsu.io/" + (media == JapaneseMedia.ANIME ? "anime" : "manga") + "/" + token["slug"].Value<string>(),
       Description = description,
       ImageUrl = token["posterImage"]["original"].Value<string>()
   };
   if (token["titles"] != null && token["titles"]["en"] != null && !string.IsNullOrEmpty(token["titles"]["en"].Value<string>()) && token["titles"]["en"].Value<string>() != token["canonicalTitle"].Value<string>()) // No use displaying this if it's the same as the embed title
       embed.AddField("English Title", token["titles"]["en"].Value<string>());
   if (media == JapaneseMedia.ANIME && token["episodeCount"] != null)
       embed.AddField("Episode Count", token["episodeCount"].Value<string>() + (token["episodeLength"] != null ? $" ({token["episodeLength"].Value<string>()} minutes per episode)" : ""), true);
   if (token["startDate"] == null)
       embed.AddField("Release Date", "To Be Announced", true);
   else
       embed.AddField("Release Date", token["startDate"] + " - " + (token["endDate"] != null ? "???" : token["endDate"]), true);
   if (!string.IsNullOrEmpty(token["ageRatingGuide"].Value<string>()))
       embed.AddField("Audiance Warning", token["ageRatingGuide"].Value<string>(), true);
   if (!string.IsNullOrEmpty(token["averageRating"].Value<string>()))
       embed.AddField("Kitsu User Rating", token["averageRating"].Value<string>(), true);
   await ReplyAsync(embed: embed.Build());
}
*/
    }
}
