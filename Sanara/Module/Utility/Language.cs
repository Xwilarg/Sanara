﻿using Discord;
using Discord.WebSocket;
using System.Text;

namespace Sanara.Module.Utility
{
    public class Language
    {
        public static async Task TranslateFromReactionAsync(Cacheable<IUserMessage, ulong> msg, Cacheable<IMessageChannel, ulong> chan, SocketReaction react)
        {
            if (StaticObjects.TranslationClient == null)
            {
                return;
            }
            string emote = react.Emote.ToString();
            bool allowFlags = await chan.GetOrDownloadAsync() is ITextChannel textChan && StaticObjects.Db.GetGuild(textChan.GuildId).TranslateUsingFlags;
            // If emote is not from the bot and is an arrow emote
            if (allowFlags && react.User.IsSpecified && react.User.Value.Id != StaticObjects.ClientId && StaticObjects.Flags.ContainsKey(emote))
            {
                var gMsg = (await msg.GetOrDownloadAsync()).Content;
                if (!string.IsNullOrEmpty(gMsg))
                {
                    var translation = await StaticObjects.TranslationClient.TranslateTextAsync(gMsg, StaticObjects.Flags[emote]);
                    await (await chan.GetOrDownloadAsync()).SendMessageAsync(embed: new EmbedBuilder
                    {
                        Title = "From " + (StaticObjects.ISO639.ContainsKey(translation.DetectedSourceLanguage) ? StaticObjects.ISO639[translation.DetectedSourceLanguage] : translation.DetectedSourceLanguage),
                        Description = translation.TranslatedText,
                        Color = Color.Blue
                    }.Build());
                }
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
