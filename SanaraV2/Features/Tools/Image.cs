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
using Newtonsoft.Json;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV2.Features.Tools
{
    public static class Image
    {
        public static async Task<FeatureRequest<Response.Image, Error.Image>> SearchColor(string[] args)
        {
            if (args.Length == 0)
                return (new FeatureRequest<Response.Image, Error.Image>(null, Error.Image.InvalidArg));
            string color = Utilities.AddArgs(args);
            if (color[0] == '#')
                color = color.Substring(1);
            Color? finalColor;
            if (color.Length == 6 || color.Length == 3)
            {
                try
                {
                    finalColor = ColorTranslator.FromHtml("#" + color);
                }
                catch (System.Exception)
                {
                    finalColor = null;
                }
            }
            else if (args.Length == 3)
            {
                try
                {
                    finalColor = Color.FromArgb(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]));
                }
                catch (System.Exception)
                {
                    finalColor = null;
                }
            }
            else
                finalColor = null;
            if (finalColor == null)
            {
                finalColor = Color.FromName(color);
                if (finalColor.Value.R == 0 && finalColor.Value.B == 0 && finalColor.Value.G == 0)
                    return (new FeatureRequest<Response.Image, Error.Image>(null, Error.Image.InvalidColor));
            }
            string hexValue = finalColor.Value.R.ToString("X2") + finalColor.Value.G.ToString("X2") + finalColor.Value.B.ToString("X2");
            dynamic json;
            using (HttpClient hc = new HttpClient())
                json = JsonConvert.DeserializeObject(await (await hc.GetAsync("http://www.thecolorapi.com/id?hex=" + hexValue)).Content.ReadAsStringAsync());
            return (new FeatureRequest<Response.Image, Error.Image>(new Response.Image()
            {
                discordColor = new Discord.Color(finalColor.Value.R, finalColor.Value.G, finalColor.Value.B),
                colorUrl = string.Format("https://dummyimage.com/500x500/{0}/000000.png&text=+", hexValue),
                colorHex = hexValue,
                name = ((bool)json.name.exact_match_name) ? (json.name.value) : (null)
            }, Error.Image.None));
        }
    }
}
