using Discord;
using Discord.WebSocket;
using Google;
using Google.Cloud.Vision.V1;
using Sanara.Exception;
using System.Text;

namespace Sanara.Module.Utility
{
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

        public static async Task TranslateFromReactionAsync(Cacheable<IUserMessage, ulong> msg, Cacheable<IMessageChannel, ulong> chan, SocketReaction react)
        {
            string emote = react.Emote.ToString();
            bool allowFlags = await chan.GetOrDownloadAsync() is ITextChannel textChan && StaticObjects.Db.GetGuild(textChan.GuildId).TranslateUsingFlags;
            if (!allowFlags)
            {
                return;
            }
            if (StaticObjects.TranslationClient != null && StaticObjects.Flags.ContainsKey(emote))
            {
                _ = Task.Run(async () =>
                {
                    // If emote is not from the bot and is an arrow emote
                    if (react.User.IsSpecified && react.User.Value.Id != StaticObjects.ClientId)
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
                                await (await chan.GetOrDownloadAsync()).SendMessageAsync(embed: await GetTranslationEmbedAsync(gMsg, StaticObjects.Flags[emote]), messageReference: new(dMsg.Id));
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
                        await (await chan.GetOrDownloadAsync()).SendMessageAsync(embed: await Tool.GetSourceAsync(dMsg.Attachments.Any() ? dMsg.Attachments.First().Url : dMsg.Content), messageReference: new(dMsg.Id));
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

        public static async Task<Embed> GetTranslationEmbedAsync(string sentence, string language)
        {
            List<EmbedFieldBuilder> fields = new();
            if ((sentence.StartsWith("https://") || sentence.StartsWith("http://")) && sentence.Trim().Count(x => x == ' ') == 0)
            {
                if (StaticObjects.VisionClient == null)
                {
                    throw new CommandFailed("Vision client is not available");
                }
                try
                {
                    var image = await Google.Cloud.Vision.V1.Image.FetchFromUriAsync(sentence);
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
                    sentence = response.Text;
                    fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "Original Text",
                        Value = sentence
                    });
                }
                catch (GoogleApiException)
                {
                    throw new CommandFailed("The language you provided is invalid.");
                }
            }

            try
            {
                var translation = await StaticObjects.TranslationClient.TranslateTextAsync(sentence, language);
                return new EmbedBuilder
                {
                    Title = "From " + (StaticObjects.ISO639.ContainsKey(translation.DetectedSourceLanguage) ? StaticObjects.ISO639[translation.DetectedSourceLanguage] : translation.DetectedSourceLanguage),
                    Description = translation.TranslatedText,
                    Color = Color.Blue,
                    Fields = fields
                }.Build();
            }
            catch (GoogleApiException)
            {
                throw new CommandFailed("The language you provided is invalid.");
            }
        }

        public static string ToRomaji(string entry)
        {
            return ConvertLanguage(ConvertLanguage(entry, StaticObjects.KatakanaToRomaji, 'ッ'), StaticObjects.HiraganaToRomaji, 'っ');
        }

        public static string ToHiragana(string entry)
        {
            return ConvertLanguage(ConvertLanguage(entry, StaticObjects.KatakanaToRomaji, 'ッ'), StaticObjects.RomajiToHiragana, 'っ');
        }

        /// <summary>
        /// Convert an entry from a language to another
        /// </summary>
        /// <param name="entry">The entry to translate</param>
        /// <param name="dictionary">The dictionary that contains the from/to for each character</param>
        /// <param name="doubleChar">Character to use when a character is here twice, like remplace kko by っこ</param>
        public static string ConvertLanguage(string entry, Dictionary<string, string> dictionary, char doubleChar)
        {
            StringBuilder result = new();
            var biggest = dictionary.Keys.OrderByDescending(x => x.Length).First().Length;
            bool isEntryRomaji = char.IsAscii(dictionary.Keys.First()[0]) && char.IsAscii(entry[0]);
            bool doubleNext; // If we find a doubleChar, the next character need to be doubled (っこ -> kko)
            while (entry.Length > 0)
            {
                doubleNext = false;

                // SPECIAL CASES FOR KATAKANA
                if (entry[0] == 'ー') // We can't really convert this katakana so we just ignore it
                {
                    entry = entry[1..];
                    continue;
                }
                if (entry[0] == 'ァ' || entry[0] == 'ィ' || entry[0] == 'ゥ' || entry[0] == 'ェ' || entry[0] == 'ォ')
                {
                    result.Remove(result.Length - 1, 1);
                    var tmp = entry[0] switch
                    {
                        'ァ' => 'a',
                        'ィ' => 'i',
                        'ゥ' => 'u',
                        'ェ' => 'e',
                        'ォ' => 'o',
                        _ => throw new ArgumentException("Invalid katakana " + entry[0]),
                    };
                    result.Append(tmp);
                    entry = entry[1..];
                    continue;
                }

                if (entry.Length >= 2 && entry[0] == entry[1] && isEntryRomaji) // kko -> っこ
                {
                    result.Append(doubleChar);
                    entry = entry[1..];
                    continue;
                }
                if (entry[0] == doubleChar)
                {
                    doubleNext = true;
                    entry = entry[1..];
                    if (entry.Length == 0)
                        continue;
                }
                // Iterate on biggest to 1 to max size
                // We then test for each entry if we can convert
                // We begin with the biggest, if we don't do so, we would find ん (n) before な (na)
                for (int i = biggest; i > 0; i--)
                {
                    if (entry.Length >= i)
                    {
                        var value = entry[..i];
                        if (dictionary.ContainsKey(value))
                        {
                            if (doubleNext)
                                result.Append(dictionary[value][0]);
                            result.Append(dictionary[value]);
                            entry = entry[i..];
                            goto found;
                        }
                    }
                }
                result.Append(entry[0]);
                entry = entry[1..];
            found:;
            }
            return result.ToString();
        }
    }
}
