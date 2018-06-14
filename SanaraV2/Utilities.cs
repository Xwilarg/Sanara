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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2
{
    public static class Utilities
    {
        /// <summary>
        /// Remove first argument of array
        /// </summary>
        /// <param name="args">The string[] to deal with</param>
        public static string[] RemoveFirstArg(string[] args)
        {
            List<string> newArgs = new List<string>();
            for (int i = 1; i < args.Length; i++)
                newArgs.Add(args[i]);
            return (newArgs.ToArray());
        }

        /// <summary>
        /// When receiving string from website, sometimes you have to replace some stuffs on them.
        /// </summary>
        /// <param name="text">The string to deal with</param>
        public static string removeUnwantedSymboles(string text)
        {
            text = text.Replace("[i]", "*");
            text = text.Replace("[/i]", "*");
            text = text.Replace("&lt;br /&gt;", Environment.NewLine);
            text = text.Replace("mdash;", "—");
            text = text.Replace("&quot;", "\"");
            text = text.Replace("&amp;", "&");
            text = text.Replace("&#039;", "'");
            return (text);
        }

        /// <summary>
        /// Every commands take a string[] in parameter so they can be called with any number of arguments.
        /// This function transform it to a string adding spaces between each elements of the array
        /// </summary>
        /// <param name="args">The string[] to deal with</param>
        public static string addArgs(string[] args)
        {
            if (args.Length == 0)
                return (null);
            string finalStr = args[0];
            for (int i = 1; i < args.Length; i++)
            {
                finalStr += " " + args[i];
            }
            return (finalStr);
        }

        /// <summary>
        /// For comparaisons between 2 string it's sometimes useful that you remove everything except number and letters
        /// </summary>
        /// <param name="word">The string to deal with</param>
        public static string cleanWord(string word)
        {
            string finalStr = "";
            foreach (char c in word)
            {
                if (char.IsLetterOrDigit(c))
                    finalStr += char.ToUpper(c);
            }
            return (finalStr);
        }

        /// <summary>
        /// Get an element in a string
        /// </summary>
        /// <param name="tag">The tag where we begin to take the element</param>
        /// <param name="file">The string to search in</param>
        /// <param name="stopCharac">The character after with we stop looking for</param>
        /// <returns></returns>
        public static string getElementXml(string tag, string file, char stopCharac)
        {
            string saveString = "";
            int prog = 0;
            char lastChar = ' ';
            foreach (char c in file)
            {
                if (prog == tag.Length)
                {
                    if (c == stopCharac
                        && ((stopCharac == '"' && lastChar != '\\') || stopCharac != '"'))
                        break;
                    saveString += c;
                }
                else
                {
                    if (c == tag[prog])
                        prog++;
                    else
                        prog = 0;
                }
                lastChar = c;
            }
            return (saveString);
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
                    return (await guild.GetUserAsync(Convert.ToUInt64(val)));
                }
                catch (Exception)
                { }
            }
            try
            {
                return (await guild.GetUserAsync(Convert.ToUInt64(name)));
            }
            catch (Exception)
            { }
            foreach (IGuildUser user in await guild.GetUsersAsync())
            {
                if (user.Nickname == name || user.Username == name)
                    return (user);
            }
            return (null);
        }

        /// <summary>
        /// Return a string given a TimeSpan
        /// </summary>
        /// <param name="ts">The TimeSpan to transform</param>
        /// <returns>The string wanted</returns>
        public static string TimeSpanToString(TimeSpan ts, ulong guildId)
        {
            string finalStr = Sentences.timeSeconds(guildId, ts.Seconds.ToString());
            if (ts.Days > 0)
                finalStr = Sentences.timeDays(guildId, ts.Days.ToString(), ts.Hours.ToString(), ts.Minutes.ToString(), finalStr);
            else if (ts.Hours > 0)
                finalStr = Sentences.timeHours(guildId, ts.Hours.ToString(), ts.Minutes.ToString(), finalStr);
            else if (ts.Minutes > 0)
                finalStr = Sentences.timeMinutes(guildId, ts.Minutes.ToString(), finalStr);
            return (finalStr);
        }
    }
}