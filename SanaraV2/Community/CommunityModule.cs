using Discord.Commands;
using SanaraV2.Modules.Base;
using System.Drawing;
using System.Threading.Tasks;

namespace SanaraV2.Community
{
    public class CommunityModule : ModuleBase
    {
        [Command("Profile")]
        public async Task Profile(params string[] _)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Community);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Community);
            var me = Program.p.cm.GetOrCreateProfile(Context.User);
            me.UpdateProfile(Context.User);
            using (var model = new Bitmap("Saves/Assets/Background.png"))
            {
                using (var bp = new Bitmap(model.Width, model.Height))
                {
                    using (var g = Graphics.FromImage(bp))
                    {
                        g.DrawImage(model, 0, 0);
                        g.DrawImage(me.GetProfilePicture(Context.User), 20, 20);
                        g.DrawString(me.GetUsername(), new Font("Arial", 23), Brushes.Black, 170f, 70f, StringFormat.GenericDefault);
                        g.DrawString(me.GetDescription(), new Font("Arial", 15), Brushes.Black, 20f, 200f, StringFormat.GenericDefault);
                        g.Flush();
                    }
                    bp.Save("Saves/Profiles/" + Context.User.Id + ".png");
                }
            }
            await Context.Channel.SendFileAsync("Saves/Profiles/" + Context.User.Id + ".png");
        }

        [Command("Description")]
        public async Task Description(params string[] args)
        {
            var me = Program.p.cm.GetProfile(Context.User.Id);
            if (me == null)
            {
                await ReplyAsync("You don't have a profile. You must at first generate it with the 'Profile' command.");
                return;
            }
            if (me.UpdateDescription(string.Join(" ", args)))
            {
                await ReplyAsync("Your description was updated.");
            }
            else
            {
                await ReplyAsync("Your description is too long.");
            }
        }
    }
}
