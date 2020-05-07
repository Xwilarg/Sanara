using Discord.Commands;
using SanaraV2.Modules.Base;
using System.Drawing;
using System.Threading.Tasks;

namespace SanaraV2.Community
{
    public class CommunityModule : ModuleBase
    {
        [Command("Profile")]
        public async Task Leave(params string[] _)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Community);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Community);
            using (var model = new Bitmap("Saves/Assets/Background.png"))
            {
                using (var bp = new Bitmap(model.Width, model.Height))
                {
                    using (var g = Graphics.FromImage(bp))
                    {
                        g.DrawImage(model, 0, 0);
                        g.DrawString(Context.User.ToString(), new Font("Arial", 18), Brushes.Black, 25f, 25f, StringFormat.GenericDefault);
                        g.Flush();
                    }
                    bp.Save("Saves/Profiles/" + Context.User.Id + ".png");
                }
            }
            await Context.Channel.SendFileAsync("Saves/Profiles/" + Context.User.Id + ".png");
        }
    }
}
