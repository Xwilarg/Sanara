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
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Base
{
    public static class Utilities
    {
        public static void CheckAvailability(IGuild guild, Program.Module module)
        {
            if (guild != null && !Program.p.db.IsAvailable(guild.Id, module))
                throw new NotAvailable();
        }

        public static string AddZero(string nb)
        {
            if (nb.Length == 1)
                return ("0" + nb);
            return (nb);
        }

        /// <summary>
        /// Every commands take a string[] in parameter so they can be called with any number of arguments.
        /// This function transform it to a string adding spaces between each elements of the array
        /// </summary>
        /// <param name="args">The string[] to deal with</param>
        public static string AddArgs(string[] args)
        {
            if (args.Length == 0)
                return (null);
            return (string.Join(" ", args));
        }

        /// <summary>
        /// For comparaisons between 2 string it's sometimes useful that you remove everything except number and letters
        /// </summary>
        /// <param name="word">The string to deal with</param>
        public static string CleanWord(string word)
        {
            string finalStr = "";
            word = Regex.Replace(word, "%[0-9A-Z]{2}", ""); // Remove special characters encoded
            foreach (char c in word)
            {
                if (char.IsLetterOrDigit(c))
                    finalStr += char.ToUpper(c);
            }
            return (finalStr);
        }

        /// <summary>
        /// Get a user by his username/nickname/id
        /// </summary>
        /// <param name="name">The name/id of the user</param>
        /// <param name="guild">The guild the user is in</param>
        /// <returns></returns>
        public static async Task<IGuildUser> GetUser(string name, IGuild guild)
        {
            Match match = Regex.Match(name, "<@[!]?[0-9]{18}>");
            if (match.Success)
            {
                try
                {
                    string val = "";
                    foreach (char c in match.Value)
                    {
                        if (char.IsNumber(c))
                            val += c;
                    }
                    return await guild.GetUserAsync(Convert.ToUInt64(val));
                }
                catch (Exception) // Error while parsing user ID
                { }
            }
            try
            {
                return await guild.GetUserAsync(Convert.ToUInt64(name));
            }
            catch (Exception) // Error while parsing user ID
            { }
            foreach (IGuildUser user in await guild.GetUsersAsync())
            {
                if (user.Nickname == name || user.Username == name)
                    return user;
            }
            return null;
        }

        /// <summary>
        /// Return a string given a TimeSpan
        /// </summary>
        /// <param name="ts">The TimeSpan to transform</param>
        /// <returns>The string wanted</returns>
        public static string TimeSpanToString(TimeSpan ts, ulong guildId)
        {
            string finalStr = Sentences.TimeSeconds(guildId, ts.Seconds.ToString());
            if (ts.Days > 0)
                finalStr = Sentences.TimeDays(guildId, ts.Days.ToString(), ts.Hours.ToString(), ts.Minutes.ToString(), finalStr);
            else if (ts.Hours > 0)
                finalStr = Sentences.TimeHours(guildId, ts.Hours.ToString(), ts.Minutes.ToString(), finalStr);
            else if (ts.Minutes > 0)
                finalStr = Sentences.TimeMinutes(guildId, ts.Minutes.ToString(), finalStr);
            return finalStr;
        }

        /// <summary>
        /// Get a language in 2 letters (ex: fr for french)
        /// </summary>
        /// <param name="languageName">Language string</param>
        public static string GetLanguage(string languageName)
        {
            string lang = null;
            if (Program.p.allLanguages.ContainsKey(languageName))
                lang = languageName;
            foreach (var key in Program.p.allLanguages)
            {
                if (key.Value.Contains(languageName))
                {
                    lang = key.Key;
                    break;
                }
            }
            return lang;
        }

        /// <summary>
        /// Check if file extension is the one of an image
        /// </summary>
        public static bool IsImage(string link)
            => link.Contains(".gif") || link.Contains(".png") || link.Contains(".jpg") || link.Contains(".jpeg");
    }
}