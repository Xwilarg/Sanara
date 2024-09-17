using Discord;
using Discord.WebSocket;
using Google.Cloud.Translate.V3;
using Google.Cloud.Vision.V1;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Database;
using Sanara.Exception;
using Sanara.Service;
using System.Globalization;
using System.Web;

namespace Sanara.Module.Utility;

public class Language
{
    private static List<string> _alreadyRequests = new();

    private static void AddToRequestList(string id)
    {
        if (_alreadyRequests.Count == 100)
        {
            _alreadyRequests.RemoveAt(0);
        }
        _alreadyRequests.Add(id);
    }

    public static async Task TranslateFromReactionAsync(IServiceProvider provider, Cacheable<IUserMessage, ulong> msg, Cacheable<IMessageChannel, ulong> chan, SocketReaction react)
    {
        string emote = react.Emote.ToString();
        bool allowFlags = await chan.GetOrDownloadAsync() is ITextChannel textChan && provider.GetRequiredService<Db>().GetGuild(textChan.GuildId).TranslateUsingFlags;
        if (!allowFlags)
        {
            return;
        }
        if (provider.GetRequiredService<TranslationServiceClient>() != null && provider.GetRequiredService<TranslatorService>().Flags.ContainsKey(emote))
        {
            _ = Task.Run(async () =>
            {
                // If emote is not from the bot and is an arrow emote
                if (react.User.IsSpecified && react.User.Value.Id != Program.ClientId)
                {
                    var dMsg = await msg.GetOrDownloadAsync();
                    if (_alreadyRequests.Contains("TR_" + dMsg.Id))
                    {
                        return;
                    }
                    var gMsg = dMsg.Content;
                    if (string.IsNullOrEmpty(gMsg) && dMsg.Attachments.Any())
                    {
                        gMsg = dMsg.Attachments.ElementAt(0).Url;
                    }
                    else if (string.IsNullOrEmpty(gMsg) && dMsg.Embeds.Any() && dMsg.Embeds.ElementAt(0).Image.HasValue)
                    {
                        gMsg = dMsg.Embeds.ElementAt(0).Image.Value.Url;
                    }
                    else if (string.IsNullOrEmpty(gMsg) && dMsg.Embeds.Any())
                    {
                        gMsg = dMsg.Embeds.ElementAt(0).Description;
                    }
                    if (!string.IsNullOrEmpty(gMsg))
                    {
                        try
                        {
                            var tr = await GetTranslationEmbedAsync(provider, gMsg, provider.GetRequiredService<TranslatorService>().Flags[emote]);
                            await (await chan.GetOrDownloadAsync()).SendMessageAsync(embed: tr.embed, components: tr.component.Build(), messageReference: new MessageReference(dMsg.Id));
                            AddToRequestList("TR_" + dMsg.Id);
                        }
                        catch (CommandFailed ex)
                        {
                            await (await chan.GetOrDownloadAsync()).SendMessageAsync(embed: new EmbedBuilder
                            {
                                Color = Color.Red,
                                Description = ex.Message
                            }.Build(), messageReference: new(dMsg.Id));
                        }
                        catch (System.Exception e)
                        {
                            await Log.LogErrorAsync(e, null);
                        }
                    }
                }
            });
        }
        else if (emote == "🛸" || emote == "ℹ️")
        {
            _ = Task.Run(async () =>
            {
                var dMsg = await msg.GetOrDownloadAsync();
                if (_alreadyRequests.Contains("SR_" + dMsg.Id))
                {
                    return;
                }
                try
                {
                    await (await chan.GetOrDownloadAsync()).SendMessageAsync(embed: await Tool.GetSourceAsync(provider.GetRequiredService<HttpClient>(), dMsg.Attachments.Any() ? dMsg.Attachments.First().Url : dMsg.Content), messageReference: new(dMsg.Id));
                    AddToRequestList("SR_" + dMsg.Id);
                }
                catch (CommandFailed cf)
                {
                    await (await chan.GetOrDownloadAsync()).SendMessageAsync(cf.Message, messageReference: new(dMsg.Id));
                }
                catch (System.Exception e)
                {
                    await Log.LogErrorAsync(e, null);
                }
            });
        }
    }

    public static async Task<(Embed embed, ComponentBuilder component)> GetTranslationEmbedAsync(IServiceProvider provider, string sentence, string language)
    {
        ComponentBuilder buttons = new();
        if ((sentence.StartsWith("https://") || sentence.StartsWith("http://")) && !sentence.Trim().Any(x => x == ' '))
        {
            var image = await Google.Cloud.Vision.V1.Image.FetchFromUriAsync(sentence);
            TextAnnotation response;
            try
            {
                response = await provider.GetRequiredService<ImageAnnotatorClient>().DetectDocumentTextAsync(image);
            }
            catch (AnnotateImageException)
            {
                throw new CommandFailed("The file given isn't a valid image.");
            }
            if (response == null)
                throw new CommandFailed("There is no text on the image.");
            sentence = response.Text;
            var key = Guid.NewGuid().ToString();
            provider.GetRequiredService<TranslatorService>().TranslationOriginalText.Add(key, sentence);
            buttons.WithButton("Original Text", $"tr-{key}");
        }

        var req = new TranslateTextRequest
        {
            Contents = { sentence },
            TargetLanguageCode = CultureInfo.GetCultures(CultureTypes.NeutralCultures)
                .FirstOrDefault(x => x.EnglishName.Equals(language, StringComparison.InvariantCultureIgnoreCase) || x.NativeName.Equals(language, StringComparison.InvariantCultureIgnoreCase))?.TwoLetterISOLanguageName ?? language,
            Parent = $"projects/{provider.GetRequiredService<Credentials>().GoogleProjectId}/locations/global"
        };

        TranslateTextResponse translation;
        try
        {
            translation = await provider.GetRequiredService<TranslationServiceClient>().TranslateTextAsync(req);
        }
        catch (RpcException e)
        {
            if (e.StatusCode == StatusCode.InvalidArgument)
            {
                throw new CommandFailed("The language given is invalid", ephemeral: true);
            }
            throw;
        }

        var answer = translation.Translations[0];

        return (new EmbedBuilder
        {
            Title = $"From {new CultureInfo(answer.DetectedLanguageCode)}",
            Description = HttpUtility.HtmlDecode(answer.TranslatedText),
            Color = Color.Blue
        }.Build(), buttons);
    }
}
