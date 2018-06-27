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
using SanaraV2.Base;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SanaraV2.Tools
{
    public class ImageModule : ModuleBase
    {
        Program p = Program.p;
        [Command("Transparency", RunMode = RunMode.Async), Summary("Add transparency to the image given in parameter")]
        public async Task Transparency(params string[] word)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Image);
            if (word.Length == 0 || !Utilities.IsLinkValid(word[0]))
                await ReplyAsync(Sentences.HelpTransparency(Context.Guild.Id));
            else
            {
                string extension = Utilities.GetExtensionImage(word[0]);
                if (extension == null)
                {
                    await ReplyAsync(Sentences.InvalidFormat(Context.Guild.Id));
                    return;
                }
                string currName = "transparency" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString() + ".png";
                using (WebClient wc = new WebClient())
                {
                    using (MemoryStream stream = new MemoryStream(wc.DownloadData(word[0])))
                    {
                        Bitmap bmp = new Bitmap(stream);
                        bmp.MakeTransparent(Color.White);
                        bmp.Save(currName, ImageFormat.Png);
                    }
                }
                await Context.Channel.SendFileAsync(currName);
                File.Delete(currName);
            }
        }

        [Command("Negate", RunMode = RunMode.Async), Summary("Negate the image color")]
        public async Task Negate(params string[] word)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Image);
            if (word.Length == 0 || !Utilities.IsLinkValid(word[0]))
                await ReplyAsync(Sentences.HelpTransparency(Context.Guild.Id));
            else
            {
                string extension = Utilities.GetExtensionImage(word[0]);
                if (extension == null)
                {
                    await ReplyAsync(Sentences.InvalidFormat(Context.Guild.Id));
                    return;
                }
                string currName = "negate" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString() + "." + extension;
                ChangeImage(currName, word[0], delegate (Bitmap bmp, int i, int y)
                {
                    Color color = bmp.GetPixel(i, y);
                    bmp.SetPixel(i, y, Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B));
                });
                await Context.Channel.SendFileAsync(currName);
                File.Delete(currName);
            }
        }

        [Command("Convert", RunMode = RunMode.Async), Summary("Convert an image to another format")]
        public async Task ConvertImage(params string[] word)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Image);
            if (word.Length <= 1 || !Utilities.IsLinkValid(word[0]))
                await ReplyAsync(Sentences.HelpConvert(Context.Guild.Id));
            else
            {
                string extension = Utilities.GetExtensionImage(word[0]);
                string url = word[0];
                word = Utilities.RemoveFirstArg(word);
                ImageFormat newExtension = Utilities.GetImage(Utilities.AddArgs(word));
                if (extension == null || newExtension == null)
                {
                    await ReplyAsync(Sentences.InvalidFormat(Context.Guild.Id));
                    return;
                }
                string currName = "convert" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString() + "." + Utilities.AddArgs(word);
                using (WebClient wc = new WebClient())
                {
                    using (MemoryStream stream = new MemoryStream(wc.DownloadData(url)))
                    {
                        Bitmap bmp = new Bitmap(stream);
                        bmp.Save(currName, newExtension);
                    }
                }
                await Context.Channel.SendFileAsync(currName);
                File.Delete(currName);
            }
        }

        [Command("Rgb", RunMode = RunMode.Async), Summary("Display a RGB color")]
        public async Task Rgb(params string[] word)
        {
            if (word.Length < 3)
                await ReplyAsync(Sentences.HelpRgb(Context.Guild.Id));
            else
            {
                Bitmap bmp = new Bitmap(100, 100, PixelFormat.Format24bppRgb);
                Color color;
                try
                {
                    color = Color.FromArgb(255, Convert.ToInt32(word[0]), Convert.ToInt32(word[1]), Convert.ToInt32(word[2]));
                }
                catch (ArgumentException)
                {
                    await ReplyAsync(Sentences.InvalidColor(Context.Guild.Id));
                    return;
                }
                catch (OverflowException)
                {
                    await ReplyAsync(Sentences.InvalidColor(Context.Guild.Id));
                    return;
                }
                catch (FormatException)
                {
                    await ReplyAsync(Sentences.InvalidColor(Context.Guild.Id));
                    return;
                }
                string currName = "rgb" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString() + ".png";
                LoopImage(bmp, delegate (Bitmap bitmap, int i, int y) {
                    bitmap.SetPixel(i, y, color);
                });
                bmp.Save(currName);
                await Context.Channel.SendFileAsync(currName);
                File.Delete(currName);
            }
        }

        [Command("Epure", RunMode = RunMode.Async), Summary("Epure an image")]
        public async Task Epure(params string[] word)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Image);
            if (word.Length == 0 || !Utilities.IsLinkValid(word[0]))
                await ReplyAsync(Sentences.HelpTransparency(Context.Guild.Id));
            else
            {
                string extension = Utilities.GetExtensionImage(word[0]);
                if (extension == null)
                {
                    await ReplyAsync(Sentences.InvalidFormat(Context.Guild.Id));
                    return;
                }
                int step = 25;
                if (word.Length > 1)
                {
                    try
                    {
                        step = Convert.ToInt32(word[1]);
                        if (step <= 0 || step > 255)
                        {
                            await ReplyAsync(Sentences.InvalidStep(Context.Guild.Id));
                            return;
                        }
                    }
                    catch (OverflowException)
                    {
                        await ReplyAsync(Sentences.InvalidColor(Context.Guild.Id));
                        return;
                    }
                    catch (FormatException)
                    {
                        await ReplyAsync(Sentences.InvalidColor(Context.Guild.Id));
                        return;
                    }
                }
                string currName = "epure" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString() + "." + extension;
                ChangeImage(currName, word[0], delegate (Bitmap bmp, int i, int y)
                {
                    Color color = bmp.GetPixel(i, y);
                    bmp.SetPixel(i, y, GetClosestColor(color, step));
                });
                await Context.Channel.SendFileAsync(currName);
                File.Delete(currName);
            }
        }

        private void LoopImage(Bitmap bmp, Action<Bitmap, int, int> action)
        {
            for (int i = 0; i < bmp.Size.Width; i++)
            {
                for (int y = 0; y < bmp.Size.Height; y++)
                {
                    action(bmp, i, y);
                }
            }
        }

        private void ChangeImage(string currName, string url, Action<Bitmap, int, int> action)
        {
            using (WebClient wc = new WebClient())
            {
                using (MemoryStream stream = new MemoryStream(wc.DownloadData(url)))
                {
                    Bitmap bmp = new Bitmap(stream);
                    LoopImage(bmp, action);
                    bmp.Save(currName);
                }
            }
        }

        private Color GetClosestColor(Color orrColor, int step)
        {
            return (Color.FromArgb(255, (orrColor.R * step / 255) * (255 / step), (orrColor.G * step / 255) * (255 / step), (orrColor.B * step / 255) * (255 / step)));
        }
    }
}