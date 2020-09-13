using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Exceptions;
using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Administration
{
    public sealed partial class HelpPreload
    {
        public void LoadScienceHelp()
        {
            _help.Add(new Help("Calc", new[] { new Argument(ArgumentType.MANDATORY, "operation") }, "Evaluate a basic math operation and return the result.", false));
        }
    }
}

namespace SanaraV3.Modules.Tool
{
    public class ScienceModule : ModuleBase
    {
        [Command("Color"), Priority(-1)]
        public async Task ColorAsync([Remainder]string color)
        {
            try
            {
                await ParseColorAsync(ColorTranslator.FromHtml((color.StartsWith("#") ? "" : "#") + color));
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is FormatException)
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
