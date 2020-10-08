using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Exception;
using SimpleCrypto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadScienceHelp()
        {
            _submoduleHelp.Add("Science", "Get information related to science");
            _help.Add(("Tool", new Help("Science", "Calc", new[] { new Argument(ArgumentType.MANDATORY, "operation") }, "Evaluate a basic math operation and return the result.", new string[0], Restriction.None, "Calc 72 * 32")));
            _help.Add(("Tool", new Help("Science", "Color", new[] { new Argument(ArgumentType.MANDATORY, "name/RGB/Hex") }, "Evaluate a basic math operation and return the result.", new string[0], Restriction.None, "Color 125 32 200")));
            _help.Add(("Tool", new Help("Science", "Qrcode", new[] { new Argument(ArgumentType.MANDATORY, "text") }, "Create a QR code with the text as a content", new[] { "qr" }, Restriction.None, "Qrcode https://nyanpass.com/")));
            _help.Add(("Tool", new Help("Science", "Encode", new[] { new Argument(ArgumentType.MANDATORY, "text") }, "Encode a text", new string[0], Restriction.None, "Encode https://github.com/Xwilarg/")));
            _help.Add(("Tool", new Help("Science", "Decode", new[] { new Argument(ArgumentType.MANDATORY, "text") }, "Decode a text", new string[0], Restriction.None, "Decode (%e2%95%af%c2%b0%e2%96%a1%c2%b0%ef%bc%89%e2%95%af%ef%b8%b5+%e2%94%bb%e2%94%81%e2%94%bb")));
            _help.Add(("Tool", new Help("Science", "Hash", new[] { new Argument(ArgumentType.MANDATORY, "text") }, "Hash a text", new string[0], Restriction.None, "Hash hello")));
        }
    }
}

namespace SanaraV3.Module.Tool
{
    public sealed class ScienceModule : ModuleBase
    {
        [Command("Hash")]
        public async Task HashAsync([Remainder]string content)
        {
            // SHA256
            var crypt256 = new SHA256Managed();
            var hash256 = new StringBuilder();
            byte[] compute = crypt256.ComputeHash(Encoding.UTF8.GetBytes(content));
            foreach (byte b in compute)
            {
                hash256.Append(b.ToString("x2"));
            }

            var crypt1 = new SHA1Managed();
            var hash1 = new StringBuilder();
            compute = crypt1.ComputeHash(Encoding.UTF8.GetBytes(content));
            foreach (byte b in compute)
            {
                hash1.Append(b.ToString("x2"));
            }

            StringBuilder crypt5 = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(content);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                for (int i = 0; i < hashBytes.Length; i++)
                {
                    crypt5.Append(hashBytes[i].ToString("X2"));
                }
            }

            await ReplyAsync(embed: new Discord.EmbedBuilder
            {
                Color = Discord.Color.Blue,
                Fields = new List<Discord.EmbedFieldBuilder>
                {
                    new Discord.EmbedFieldBuilder
                    {
                        Name = "SHA256",
                        Value = hash256.ToString()
                    },
                    new Discord.EmbedFieldBuilder
                    {
                        Name = "SHA1",
                        Value = hash1.ToString()
                    },
                    new Discord.EmbedFieldBuilder
                    {
                        Name = "MD5",
                        Value = crypt5.ToString()
                    },
                    new Discord.EmbedFieldBuilder
                    {
                        Name = "BCrypt",
                        Value =  BCrypt.Net.BCrypt.HashPassword(content)
                    },
                    new Discord.EmbedFieldBuilder
                    {
                        Name = "PBKDF2",
                        Value = new PBKDF2().Compute(content)
                    }
                },
                Footer = new Discord.EmbedFooterBuilder
                {
                    Text = "Never send your password or any sensitive information on Discord"
                }
            }.Build());
        }

        [Command("Encode")]
        public async Task EncodeAsync([Remainder]string content)
        {
            await ReplyAsync(embed: new Discord.EmbedBuilder
            {
                Color = Discord.Color.Blue,
                Fields = new List<Discord.EmbedFieldBuilder>
                {
                    new Discord.EmbedFieldBuilder
                    {
                        Name = "URL",
                        Value = HttpUtility.UrlEncode(content)
                    },
                    new Discord.EmbedFieldBuilder
                    {
                        Name = "HTML",
                        Value = HttpUtility.HtmlEncode(content)
                    },
                    new Discord.EmbedFieldBuilder
                    {
                        Name = "JavaScript",
                        Value = HttpUtility.JavaScriptStringEncode(content)
                    }
                }
            }.Build());
        }


        [Command("Decode")]
        public async Task DecodeAsync([Remainder]string content)
        {
            await ReplyAsync(embed: new Discord.EmbedBuilder
            {
                Color = Discord.Color.Blue,
                Fields = new List<Discord.EmbedFieldBuilder>
                {
                    new Discord.EmbedFieldBuilder
                    {
                        Name = "URL",
                        Value = HttpUtility.UrlDecode(content)
                    },
                    new Discord.EmbedFieldBuilder
                    {
                        Name = "HTML",
                        Value = HttpUtility.HtmlDecode(content)
                    }
                }
            }.Build());
        }

        [Command("Qrcode"), Alias("Qr")]
        public async Task QrcodeAsync([Remainder]string content)
        {
            await Context.Channel.SendFileAsync(await StaticObjects.HttpClient.GetStreamAsync("https://api.qrserver.com/v1/create-qr-code/?data=" + HttpUtility.UrlEncode(content)), "qrcode.png");
        }

        [Command("Color"), Priority(-1)]
        public async Task ColorAsync([Remainder]string color)
        {
            try // TODO: Can probably do a REGEX to check if the input is an hexadecimal string
            {
                await ParseColorAsync(ColorTranslator.FromHtml((color.StartsWith("#") ? "" : "#") + color)); // If the user didn't write the # we add it manually
            }
            catch (System.Exception e)
            {
                if (e is ArgumentException || e is FormatException) // Can be thrown by ColorTranslator.FromHtml is the input is invalid
                {
                    var cName = Color.FromName(color);
                    if (cName.IsKnownColor)
                        await ParseColorAsync(Color.FromName(color));
                    else
                        throw new CommandFailed("The given argument is not a color.");
                }
                else
                    throw;
            }
        }

        [Command("Color")]
        public async Task ColorFromRGB(int r, int g, int b)
        {
            if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
                throw new CommandFailed("RGB values must be between 0 and 255.");
            await ParseColorAsync(Color.FromArgb(r, g, b));
        }

        private async Task ParseColorAsync(Color color)
        {
            string hexValue = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            var json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.HttpClient.GetStringAsync("http://www.thecolorapi.com/id?hex=" + hexValue));
            await ReplyAsync(embed: new Discord.EmbedBuilder
            {
                Title = json["name"]["exact_match_name"].Value<bool>() ? json["name"]["value"].Value<string>() : null,
                Description = $"RGB: {color.R}, {color.G}, {color.B}\nHex: #{hexValue}",
                ImageUrl = "https://dummyimage.com/500x500/" + hexValue + "/000000.png&text=+",
                Color = new Discord.Color(color.R, color.G, color.B)
            }.Build());
        }

        [Command("Calc")]
        public async Task CalcAsync([Remainder]string operation)
        {
            DataTable table = new DataTable();
            try
            {
                await ReplyAsync(table.Compute(operation, "").ToString());
            }
            catch (EvaluateException)
            {
                await ReplyAsync("I can't calculate the expression you gave");
            }
            catch (SyntaxErrorException)
            {
                await ReplyAsync("I can't calculate the expression you gave");
            }
        }
    }
}
