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
using Discord.Commands;
using SanaraV2.Modules.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Tools
{
    public class Code : ModuleBase
    {
        Program p = Program.p;

        [Command("Indente"), Summary("Indente the code given in parameter")]
        public async Task Indent(params string[] arg)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Code);
            await Utilities.NotAvailable(Context.Channel as ITextChannel);
            return;
            if (arg.Length == 0)
            {
                await ReplyAsync(Sentences.IndenteHelp(Context.Guild.Id));
                return;
            }
            await ReplyAsync(IndenteCode(ParseCode(arg)));
        }

        public static string IndenteCode(List<string> code)
        {
            string finalStr = "";
            int currIndente = 0;
            bool tmpIdent = false;
            foreach (string s in code)
            {
                if (s == "")
                    continue;
                string line = s;
                if (line.StartsWith("for") || line.StartsWith("while") || line.StartsWith("if") || line.StartsWith("else") || s.StartsWith("catch"))
                {
                    tmpIdent = true;
                    for (int i = 0; i < currIndente; i++)
                        finalStr += '\t';
                    finalStr += line.Trim() + Environment.NewLine;
                    continue;
                }
                else if (line[0] == '{')
                {
                    for (int i = 0; i < currIndente; i++)
                        finalStr += '\t';
                    finalStr += line.Trim() + Environment.NewLine;
                    currIndente++;
                    tmpIdent = false;
                    continue;
                }
                else if (line[0] == '}')
                {
                    currIndente--;
                    tmpIdent = false;
                }
                for (int i = 0; i < currIndente; i++)
                    finalStr += '\t';
                if (tmpIdent)
                    finalStr += '\t';
                finalStr += line.Trim() + Environment.NewLine;
                tmpIdent = false;
            }
            return (finalStr);
        }

        public static List<string> ParseCode(string[] arg)
        {
            List<string> code = new List<string>();
            string curr = "";
            bool inCond = false;
            int parenthesis = 0;
            foreach (string s in arg)
            {
                if (s.StartsWith("```"))
                {
                    curr += s;
                    code.Add(curr);
                    curr = "";
                }
                else if (s[s.Length - 1] == ';' && parenthesis <= 0)
                {
                    curr += s;
                    code.Add(curr);
                    curr = "";
                }
                else if (s == "try" || s == "{" || s == "}")
                {
                    code.Add(curr);
                    code.Add(s);
                    curr = "";
                }
                else if (s.StartsWith("for") || s.StartsWith("while") || s.StartsWith("if") || s.StartsWith("else") || s.StartsWith("catch"))
                {
                    curr += s + ' ';
                    inCond = true;
                }
                else if (inCond && s[s.Length - 1] == ')')
                {
                    curr += s;
                    code.Add(curr);
                    curr = "";
                    inCond = false;
                }
                else
                    curr += s + ' ';
                if (s[0] == '(')
                    parenthesis++;
                else if (s[0] == ')')
                    parenthesis--;
            }
            code.Add(curr);
            return (code);
        }
    }
}