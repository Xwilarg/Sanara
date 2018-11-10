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
using System.Drawing;
using System.Threading.Tasks;

namespace SanaraV2.Features.Tools
{
    public static class Image
    {
        public static async Task<FeatureRequest<Response.Image, Error.Image>> SearchColor(string[] args)
        {
            if (args.Length == 0)
                return (new FeatureRequest<Response.Image, Error.Image>(null, Error.Image.InvalidArg));
            string color = Utilities.AddArgs(args, "");
            if (color[0] == '#')
                color = color.Substring(1);
            Color finalColor;
            if (color.Length == 6)
            {
                try
                {
                    finalColor = ColorTranslator.FromHtml("#" + color);
                }
                catch (System.Exception)
                {
                    return (new FeatureRequest<Response.Image, Error.Image>(null, Error.Image.InvalidColor));
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
                    return (new FeatureRequest<Response.Image, Error.Image>(null, Error.Image.InvalidColor));
                }
            }
            else
            {
                finalColor = Color.FromName(color);
                if (finalColor.R == 0 && finalColor.B == 0 && finalColor.G == 0)
                    return (new FeatureRequest<Response.Image, Error.Image>(null, Error.Image.InvalidColor));
            }
            return (new FeatureRequest<Response.Image, Error.Image>(new Response.Image()
            {
                systemColor = finalColor,
                discordColor = new Discord.Color(finalColor.R, finalColor.G, finalColor.B)
            }, Error.Image.None));
        }
    }
}
