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
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Features.Tools
{
    public static class Code
    {
        public static async Task<FeatureRequest<Response.Shell, Error.Shell>> Shell(string[] args)
        {
            if (args.Length == 0)
                return (new FeatureRequest<Response.Shell, Error.Shell>(null, Error.Shell.Help));
            string html;
            using (HttpClient hc = new HttpClient())
                html = await hc.GetStringAsync("https://explainshell.com/explain?cmd=" + Uri.EscapeDataString(string.Join("%20", args)));
            if (html.Contains("No man page found for"))
                return (new FeatureRequest<Response.Shell, Error.Shell>(null, Error.Shell.NotFound));
            List<Tuple<string, string>> explanations = new List<Tuple<string, string>>();
            foreach (Match m in Regex.Matches(html, "<pre class=\"help-box\" id=\"help-[0-9]+\">"))
            {
                explanations.Add(new Tuple<string, string>("###",
                    Uri.UnescapeDataString(Regex.Replace(html.Split(new[] { m.Value }, StringSplitOptions.None)[1].Split(new[] { " </td>" }, StringSplitOptions.None)[0],
                    "<[^>]>([^<]+)<\\/[^<]+>", "$1"))));
            }
            return (new FeatureRequest<Response.Shell, Error.Shell>(new Response.Shell()
            {
                explanations = explanations
            }, Error.Shell.None));
        }
    }
}
