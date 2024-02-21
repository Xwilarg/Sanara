using Discord;
using Google.Cloud.Vision.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quickenshtein;
using Sanara.Exception;
using Sanara.Help;
using Sanara.Module.Utility;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.VisualNovel;

namespace Sanara.Module.Command.Impl
{
    public class Tool : ISubmodule
    {
        public SubmoduleInfo GetInfo()
        {
            return new("Tool", "Utility commands");
        }

        public CommandInfo[] GetCommands()
        {
            return new[]
            {
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "photo",
                        Description = "Find a photo given an optional query",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "query",
                                Description = "Filter the serach given a term",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = false
                            }
                        },
                        IsNsfw = false
                    }.Build(),
                    callback: PhotoAsync,
                    precondition: Precondition.None,
                    aliases: Array.Empty<string>(),
                    needDefer: true
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "source",
                        Description = "Get the source of an image",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "image",
                                Description = "URL to the image",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            }
                        },
                        IsNsfw = false
                    }.Build(),
                    callback: SourceAsync,
                    precondition: Precondition.None,
                    aliases: Array.Empty<string>(),
                    needDefer: true
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "anime",
                        Description = "Get information about an anime/manga/light novel",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "name",
                                Description = "Name",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            },
                            new SlashCommandOptionBuilder()
                            {
                                Name = "type",
                                Description = "What kind of media you are looking for",
                                Type = ApplicationCommandOptionType.Integer,
                                IsRequired = true,
                                Choices = new()
                                {
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Anime",
                                        Value = (int)JapaneseMedia.Anime
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Manga",
                                        Value = (int)JapaneseMedia.Manga
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Light Novel",
                                        Value = (int)JapaneseMedia.LightNovel
                                    }
                                }
                            }
                        },
                        IsNsfw = false
                    }.Build(),
                    callback: AnimeAsync,
                    precondition: Precondition.None,
                    aliases: Array.Empty<string>(),
                    needDefer: true
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "drama",
                        Description = "Get information about a drama",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "name",
                                Description = "Name",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            }
                        },
                        IsNsfw = false
                    }.Build(),
                    callback: DramaAsync,
                    precondition: Precondition.None,
                    aliases: Array.Empty<string>(),
                    needDefer: true
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "visualnovel",
                        Description = "Get information about a visual novel",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "name",
                                Description = "Name",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            }
                        },
                        IsNsfw = false
                    }.Build(),
                    callback: VisualNovelAsync,
                    precondition: Precondition.None,
                    aliases: new[] { "vn" },
                    needDefer: true
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "qrcode",
                        Description = "Generate a QR code",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "input",
                                Description = "Text hidden behind the QR Code",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            }
                        },
                        IsNsfw = false
                    }.Build(),
                    callback: QrcodeAsync,
                    precondition: Precondition.None,
                    aliases: Array.Empty<string>(),
                    needDefer: true
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "ocr",
                        Description = "Detect text on an image",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "image",
                                Description = "Image",
                                Type = ApplicationCommandOptionType.Attachment,
                                IsRequired = true
                            }
                        },
                        IsNsfw = false
                    }.Build(),
                    callback: OCRAsync,
                    precondition: Precondition.None,
                    aliases: Array.Empty<string>(),
                    needDefer: true
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "get",
                        Description = "Do a GET request to an URL and return the result",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "url",
                                Description = "URL",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            }
                        },
                        IsNsfw = false
                    }.Build(),
                    callback: GetAsync,
                    precondition: Precondition.OwnerOnly,
                    aliases: Array.Empty<string>(),
                    needDefer: true
                )
            };
        }

        public async Task GetAsync(ICommandContext ctx)
        {
            var url = ctx.GetArgument<string>("url");
            var resp = await StaticObjects.HttpClient.SendAsync(new(HttpMethod.Get, url));

            var text = await resp.Content.ReadAsStringAsync();
            if (text.Length > 4088)
            {
                using var mStream = new MemoryStream();
                using var fStream = new StreamWriter(mStream);
                fStream.Write(text);
                fStream.Flush();
                mStream.Position = 0;
                await ctx.ReplyAsync(mStream, $"output-{resp.StatusCode}.txt");
            }
            else
            {
                await ctx.ReplyAsync(embed: new EmbedBuilder
                {
                    Color = resp.StatusCode == HttpStatusCode.OK ? Color.Green : Color.Red,
                    Title = resp.StatusCode.ToString(),
                    Description = $"```\n{await resp.Content.ReadAsStringAsync()}\n```"
                }.Build(), ephemeral: true);
            }
        }

        public async Task OCRAsync(ICommandContext ctx)
        {
            var input = ctx.GetArgument<IAttachment>("image");
            var image = await Google.Cloud.Vision.V1.Image.FetchFromUriAsync(input.Url);
            TextAnnotation response;
            try
            {
                response = await StaticObjects.VisionClient.DetectDocumentTextAsync(image);
            }
            catch (AnnotateImageException)
            {
                throw new CommandFailed("The file given isn't a valid image.");
            }
            if (response == null)
                throw new CommandFailed("There is no text on the image.");

            var embed = new EmbedBuilder();
            var img = SixLabors.ImageSharp.Image.Load(await StaticObjects.HttpClient.GetStreamAsync(input.Url));
            var pen = new Pen(SixLabors.ImageSharp.Color.Red, 2f);

            foreach (var page in response.Pages)
            {
                foreach (var block in page.Blocks)
                {
                    foreach (var paragraph in block.Paragraphs)
                    {
                        embed.AddField($"Confidence: {(paragraph.Confidence * 100):0.0}%", string.Join(" ", paragraph.Words.Select(x => string.Join("", x.Symbols.Select(s => s.Text)))));

                        // Draw all lines
                        var path = new PathBuilder();
                        path.AddLines(paragraph.BoundingBox.Vertices.Select(v => new SixLabors.ImageSharp.PointF(v.X, v.Y)).ToArray());
                        path.CloseFigure();

                        img.Mutate(x => x.Draw(pen, path.Build()));
                    }
                }
            }

            await ctx.ReplyAsync(embed: embed.Build());
            using var mStream = new MemoryStream();
            img.Save(mStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
            mStream.Position = 0;
            await ctx.ReplyAsync(mStream, "ocr.jpg");
        }

        public async Task QrcodeAsync(ICommandContext ctx)
        {
            var input = ctx.GetArgument<string>("input");
            await ctx.ReplyAsync(file: await StaticObjects.HttpClient.GetStreamAsync("https://api.qrserver.com/v1/create-qr-code/?data=" + HttpUtility.UrlEncode(input)),
                fileName: "qrcode.png");
        }


        public async Task SourceAsync(ICommandContext ctx)
        {
            var image = ctx.GetArgument<string>("image");

            await ctx.ReplyAsync(embed: await Utility.Tool.GetSourceAsync(image));
        }

        public async Task PhotoAsync(ICommandContext ctx)
        {
            if (StaticObjects.UnsplashToken == null)
            {
                throw new CommandFailed("Photo token is not available");
            }

            string? query = ctx.GetArgument<string>("query");

            JObject json;
            if (query == null)
            {
                json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.HttpClient.GetStringAsync("https://api.unsplash.com/photos/random?client_id=" + StaticObjects.UnsplashToken));
            }
            else
            {
                var resp = await StaticObjects.HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://api.unsplash.com/photos/random?query=" + HttpUtility.UrlEncode(query) + "&client_id=" + StaticObjects.UnsplashToken));
                if (resp.StatusCode == HttpStatusCode.NotFound)
                    throw new CommandFailed("There is no result with these search terms.");
                json = JsonConvert.DeserializeObject<JObject>(await resp.Content.ReadAsStringAsync());
            }
            await ctx.ReplyAsync(embed: new EmbedBuilder
            {
                Title = "By " + json["user"]["name"].Value<string>(),
                Url = json["links"]["html"].Value<string>(),
                ImageUrl = json["urls"]["full"].Value<string>(),
                Footer = new EmbedFooterBuilder
                {
                    Text = json["description"].Value<string>()
                },
                Color = Color.Blue
            }.Build());
        }

        public async Task VisualNovelAsync(ICommandContext ctx)
        {
            var name = ctx.GetArgument<string>("name");
            string originalName = name;
            name = Utils.CleanWord(name);
            HttpWebRequest http = (HttpWebRequest)WebRequest.Create("https://vndb.org/v/all?sq=" + HttpUtility.UrlEncode(originalName).Replace("%20", "+"));
            http.AllowAutoRedirect = false;

            string html;
            HttpWebResponse response;
            // HttpClient doesn't really look likes to handle redirection properly
            try
            {
                response = (HttpWebResponse)http.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError)
                    throw;

                response = (HttpWebResponse)ex.Response;
            }
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


            VisualNovel? vn;

            try
            {
                vn = (await StaticObjects.VnClient.GetVisualNovelAsync(VndbFilters.Id.Equals(id), VndbFlags.FullVisualNovel)).ToArray()[0];
            }
            catch (UnexpectedResponseException ure)
            {
                Console.WriteLine($"An error occurred searching for a VN: {ure.Message}");
                throw;
            }

            var embed = new EmbedBuilder()
            {
                Title = vn.OriginalName == null ? vn.Name : vn.OriginalName + " (" + vn.Name + ")",
                Url = "https://vndb.org/v" + vn.Id,
                ImageUrl =
#if NSFW_BUILD
                ctx.Channel is ITextChannel channel && !channel.IsNsfw && (vn.ImageRating.SexualAvg >= 1 || vn.ImageRating.ViolenceAvg >= 1) ? null : vn.Image,
#else
                (vn.ImageRating.SexualAvg >= 1 || vn.ImageRating.ViolenceAvg >= 1) ? null : vn.Image,
#endif
                Description = vn.Description == null ? null : Regex.Replace(vn.Description.Length > 1000 ? vn.Description[0..1000] + " [...]" : vn.Description, "\\[url=([^\\]]+)\\]([^\\[]+)\\[\\/url\\]", "[$2]($1)"),
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
                    releaseDate = $"{vn.Released.Month.Value:D2}/{releaseDate}";
                if (!vn.Released.Day.HasValue)
                    releaseDate = $"{vn.Released.Day.Value:D2}/{releaseDate}";
            }
            embed.AddField("Release Date", releaseDate, true);
            response.Dispose();
            await ctx.ReplyAsync(embed: embed.Build());
        }

        public async Task DramaAsync(ICommandContext ctx)
        {
            if (StaticObjects.TranslationClient == null)
            {
                throw new CommandFailed("Drama token is not available");
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://api.mydramalist.com/v1/search/titles?q=" + HttpUtility.UrlEncode(ctx.GetArgument<string>("name"))),
                Method = HttpMethod.Post
            };
            request.Headers.Add("mdl-api-key", StaticObjects.MyDramaListApiKey);

            var response = await StaticObjects.HttpClient.SendAsync(request);
            var searchResults = JsonConvert.DeserializeObject<JArray>(await response.Content.ReadAsStringAsync());

            if (!searchResults.Any())
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

            embed.AddField("English Title", drama.Value<string>("title"), true);
            embed.AddField("Country", drama.Value<string>("country"), true);
            embed.AddField("Episode Count", drama.Value<int>("episodes"), true);

            if (drama["released"] != null)
            {
                embed.AddField("Released", drama.Value<string>("released"), true);
            }
            else
            {
                embed.AddField("Air date", drama.Value<string>("aired_start") + ((drama["aired_end"] != null) ? " - " + drama.Value<string>("aired_end") : ""), true);
            }
            embed.AddField("Audiance Warning", drama.Value<string>("certification"), true);

            if (drama["votes"] != null)
            {
                embed.AddField("MyDramaList User Rating", drama.Value<double>("rating") + "/10", true);
            }

            await ctx.ReplyAsync(embed: embed.Build());
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

        public async Task AnimeAsync(ICommandContext ctx)
        {
            var name = ctx.GetArgument<string>("name");
            var media = (JapaneseMedia)ctx.GetArgument<long>("type");
            var answer = await AniList.SearchMediaAsync(media, name);

            if (answer == null)
                throw new CommandFailed("Nothing was found with this name.");

            if (ctx.Channel is ITextChannel channel && !channel.IsNsfw && answer.isAdult)
#if NSFW_BUILD
                throw new CommandFailed("The result of your search was NSFW and thus, can only be shown in a NSFW channel.");
#else
                throw new CommandFailed("Nothing was found with this name.");
#endif

            var description = "";
            if (!string.IsNullOrEmpty(answer.description))
                description = answer.description.Length > 1000 ? answer.description[..1000] + " [...]" : answer.description; // Description that fill the whole screen are a pain
            description = Utils.CleanHtml(description);

            var embed = new EmbedBuilder
            {
                Title = answer.title.romaji,
                Color = answer.isAdult ? new Color(255, 20, 147) : Color.Green,
                Url = $"https://anilist.co/{(media == JapaneseMedia.Anime ? "anime" : "manga")}/{answer.id}",
                Description = description,
                ImageUrl = answer.coverImage.large
            };

            if (answer.tags.Any())
                embed.AddField("Tags", string.Join(", ", answer.tags.Where(x => x.rank > 50).Select(x => x.name)));
            if (!string.IsNullOrEmpty(answer.title.english)) // No use displaying this if it's the same as the embed title
                embed.AddField("English Title", answer.title.english, true);
            if (answer.averageScore != null)
                embed.AddField("User Average Rating", answer.averageScore, true);
            if (media == JapaneseMedia.Anime && answer.episodes != null)
                embed.AddField("Episode Count", answer.episodes + (answer.duration != null ? $" ({answer.duration} min per episode)" : ""), true);
            if (answer.startDate.year == null)
                embed.AddField("Release Date", "To Be Announced", true);
            else if (answer.endDate == answer.startDate)
                embed.AddField("Release Date", $"{answer.startDate.year}-{Utils.PadNumber(answer.startDate.month.Value)}-{Utils.PadNumber(answer.startDate.day.Value)}", true);
            else
                embed.AddField("Release Date", $"{answer.startDate.year.Value}-{Utils.PadNumber(answer.startDate.month.Value)}-{Utils.PadNumber(answer.startDate.day.Value)}" + " - " + (answer.endDate.year == null ? "???" : $"{answer.endDate.year}-{Utils.PadNumber(answer.endDate.month.Value)}-{Utils.PadNumber(answer.endDate.day.Value)}"), true);
            if (!string.IsNullOrEmpty(answer.source))
                embed.AddField("Source", Utils.ToWordCase(answer.source.Replace('_', ' ')), true);
            if (!string.IsNullOrEmpty(answer.type))
                embed.AddField("Type", Utils.ToWordCase(answer.type), true);
            if (!string.IsNullOrEmpty(answer.format))
                embed.AddField("Format", answer.format switch
                {
                    "TV" => answer.format,
                    "OVA" => answer.format,
                    "ONA" => answer.format,
                    "TV_SHORT" => "TV short",
                    _ => Utils.ToWordCase(answer.format.Replace('_', ' '))
                }, true);
            if (answer.genres.Any())
                embed.AddField("Genres", string.Join(", ", answer.genres), true);

            await ctx.ReplyAsync(embed: embed.Build());
        }
    }
}
