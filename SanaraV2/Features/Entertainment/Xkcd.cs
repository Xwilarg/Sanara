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
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV2.Features.Entertainment
{
    public static class Xkcd
    {
        public static async Task<FeatureRequest<Response.Xkcd, Error.Xkcd>> SearchXkcd(string[] args, Random r)
        {
            int? myNb = null;
            bool isLast = false;
            if (args.Length > 0)
            {
                if (args[0].ToLower() == "last")
                    isLast = true;
                else
                {
                    int tmp;
                    if (int.TryParse(Utilities.AddArgs(args), out tmp))
                        myNb = tmp;
                    else
                        return new FeatureRequest<Response.Xkcd, Error.Xkcd>(null, Error.Xkcd.InvalidNumber);
                }
            }
            dynamic json;
            int max;
            using (HttpClient hc = new HttpClient())
            {
                json = JsonConvert.DeserializeObject(await (await hc.GetAsync("https://xkcd.com/info.0.json")).Content.ReadAsStringAsync());
                max = json.num;
                if (isLast)
                    myNb = max;
                if (myNb > max || myNb <= 0)
                    return (new FeatureRequest<Response.Xkcd, Error.Xkcd>(new Response.Xkcd() { maxNb = max }, Error.Xkcd.NotFound));
                if (myNb == null)
                    myNb = r.Next(max) + 1;
                json = JsonConvert.DeserializeObject(await (await hc.GetAsync("https://xkcd.com/" + myNb + "/info.0.json")).Content.ReadAsStringAsync());
            }
            return new FeatureRequest<Response.Xkcd, Error.Xkcd>(new Response.Xkcd()
            {
                imageUrl = json.img,
                maxNb = max,
                title = json.title,
                alt = json.alt
            }, Error.Xkcd.None);
        }
    }
}
