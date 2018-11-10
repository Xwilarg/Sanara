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
using Discord.Commands;
using SanaraV2.Modules.Base;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Tools
{
    public class Image : ModuleBase
    {
        Program p = Program.p;

        [Command("Color", RunMode = RunMode.Async), Summary("Display a RGB color")]
        public async Task SearchColor(params string[] args)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Image);
            var result = await Features.Tools.Image.SearchColor(args);
            switch (result.error)
            {
                case Features.Tools.Error.Image.InvalidArg:
                    await ReplyAsync(Sentences.HelpRgb(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Image.InvalidColor:
                    await ReplyAsync(Sentences.InvalidColor(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Image.None:
                    Bitmap bmp = new Bitmap(100, 100, PixelFormat.Format24bppRgb);
                    string currName = "rgb" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString() + ".png";
                    for (int i = 0; i < bmp.Size.Width; i++)
                        for (int y = 0; y < bmp.Size.Height; y++)
                            bmp.SetPixel(i, y, result.answer.systemColor);
                    bmp.Save(currName);
                    await Context.Channel.SendFileAsync(currName);
                    File.Delete(currName);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}