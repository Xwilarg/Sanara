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
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SanaraV2
{
    public class ImageModule : ModuleBase
    {
        Program p = Program.p;
        [Command("Transparency", RunMode = RunMode.Async), Summary("Add transparency to the image given in parameter")]
        public async Task transparency(params string[] word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Image);
            if (word.Length == 0 || !IsLinkValid(word[0]))
                await ReplyAsync(Sentences.helpTransparency(Context.Guild.Id));
            else
            {
                string extension = GetExtensionImage(word[0]);
                if (extension == null)
                {
                    await ReplyAsync(Sentences.invalidFormat(Context.Guild.Id));
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
        public async Task negate(params string[] word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Image);
            if (word.Length == 0 || !IsLinkValid(word[0]))
                await ReplyAsync(Sentences.helpTransparency(Context.Guild.Id));
            else
            {
                string extension = GetExtensionImage(word[0]);
                if (extension == null)
                {
                    await ReplyAsync(Sentences.invalidFormat(Context.Guild.Id));
                    return;
                }
                string currName = "negate" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString() + "." + extension;
                using (WebClient wc = new WebClient())
                {
                    using (MemoryStream stream = new MemoryStream(wc.DownloadData(word[0])))
                    {
                        Bitmap bmp = new Bitmap(stream);
                        for (int i = 0; i < bmp.Size.Width; i++)
                        {
                            for (int y = 0; y < bmp.Size.Height; y++)
                            {
                                Color color = bmp.GetPixel(i, y);
                                bmp.SetPixel(i, y, Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B));
                            }
                        }
                        bmp.Save(currName);
                    }
                }
                await Context.Channel.SendFileAsync(currName);
                File.Delete(currName);
            }
        }

        [Command("Convert", RunMode = RunMode.Async), Summary("Convert an image to another format")]
        public async Task convert(params string[] word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Image);
            if (word.Length <= 1 || !IsLinkValid(word[0]))
                await ReplyAsync(Sentences.helpConvert(Context.Guild.Id));
            else
            {
                string extension = GetExtensionImage(word[0]);
                string url = word[0];
                word = Program.RemoveFirstArg(word);
                ImageFormat newExtension = GetExtension(Program.addArgs(word));
                if (extension == null || newExtension == null)
                {
                    await ReplyAsync(Sentences.invalidFormat(Context.Guild.Id));
                    return;
                }
                string currName = "convert" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString() + "." + Program.addArgs(word);
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
        public async Task rgb(params string[] word)
        {
            if (word.Length < 3)
                await ReplyAsync(Sentences.helpRgb(Context.Guild.Id));
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
                    await ReplyAsync(Sentences.invalidColor(Context.Guild.Id));
                    return;
                }
                catch (OverflowException)
                {
                    await ReplyAsync(Sentences.invalidColor(Context.Guild.Id));
                    return;
                }
                catch (FormatException)
                {
                    await ReplyAsync(Sentences.invalidColor(Context.Guild.Id));
                    return;
                }
                string currName = "rgb" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString() + ".png";
                for (int i = 0; i < bmp.Size.Width; i++)
                {
                    for (int y = 0; y < bmp.Size.Height; y++)
                    {
                        bmp.SetPixel(i, y, color);
                    }
                }
                bmp.Save(currName);
                await Context.Channel.SendFileAsync(currName);
                File.Delete(currName);
            }
        }

        [Command("Epure", RunMode = RunMode.Async), Summary("Epure an image")]
        public async Task epure(params string[] word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Image);
            if (word.Length == 0 || !IsLinkValid(word[0]))
                await ReplyAsync(Sentences.helpTransparency(Context.Guild.Id));
            else
            {
                string extension = GetExtensionImage(word[0]);
                if (extension == null)
                {
                    await ReplyAsync(Sentences.invalidFormat(Context.Guild.Id));
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
                            await ReplyAsync(Sentences.invalidStep(Context.Guild.Id));
                            return;
                        }
                    }
                    catch (OverflowException)
                    {
                        await ReplyAsync(Sentences.invalidColor(Context.Guild.Id));
                        return;
                    }
                    catch (FormatException)
                    {
                        await ReplyAsync(Sentences.invalidColor(Context.Guild.Id));
                        return;
                    }
                }
                string currName = "epure" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString() + "." + extension;
                using (WebClient wc = new WebClient())
                {
                    using (MemoryStream stream = new MemoryStream(wc.DownloadData(word[0])))
                    {
                        Bitmap bmp = new Bitmap(stream);
                        for (int i = 0; i < bmp.Size.Width; i++)
                        {
                            for (int y = 0; y < bmp.Size.Height; y++)
                            {
                                Color color = bmp.GetPixel(i, y);
                                bmp.SetPixel(i, y, GetClosestColor(color, step));
                            }
                        }
                        bmp.Save(currName);
                    }
                }
                await Context.Channel.SendFileAsync(currName);
                File.Delete(currName);
            }
        }

        private Color GetClosestColor(Color orrColor, int step)
        {
            return (Color.FromArgb(255, (orrColor.R * step / 255) * (255 / step), (orrColor.G * step / 255) * (255 / step), (orrColor.B * step / 255) * (255 / step)));
        }

        private string GetExtensionImage(string fileName)
        {
            string[] file = fileName.Split('.');
            string extension = file[file.Length - 1];
            if (GetExtension(extension) != null)
                return (extension);
            else
                return (null);
        }

        private ImageFormat GetExtension(string extension)
        {
            switch (extension.ToLower())
            {
                case "jpg":
                case "jpeg":
                    return (ImageFormat.Jpeg);
                case "png":
                    return (ImageFormat.Png);
                case "bmp":
                    return (ImageFormat.Bmp);
                case "emf":
                    return (ImageFormat.Emf);
                case "exif":
                    return (ImageFormat.Exif);
                case "gif":
                    return (ImageFormat.Gif);
                case "icon":
                    return (ImageFormat.Icon);
                case "memorybmp":
                case "memory bmp":
                case "memory_bmp":
                    return (ImageFormat.MemoryBmp);
                case "tiff":
                    return (ImageFormat.Tiff);
                case "wmf":
                    return (ImageFormat.Wmf);
                default:
                    return (null);
            }
        }

        public bool IsLinkValid(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "HEAD";
                request.GetResponse();
                return (true);
            }
            catch (WebException)
            { }
            return (false);
        }
    }
}