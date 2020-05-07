using Discord.Commands;
using SanaraV2.Modules.Base;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace SanaraV2.Community
{
    [Group("Profile"), Alias("P")]
    public class CommunityModule : ModuleBase
    {
        [Command(""), Priority(-1)]
        public async Task ProfileDefault(params string[] args)
        {
            if (args.Length == 0) await Profile(args);
        }

        [Command("Show"), Alias("Get")]
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
                        Color color = me.GetBackgroundColor();
                        Brush backgroundBrush = new SolidBrush(Color.FromArgb(50, color.R, color.G, color.B));
                        g.FillRectangle(backgroundBrush, 0, 0, model.Width, model.Height);
                        var pfpImage = me.GetProfilePicture(Context.User);
                        // Round the image, from: https://stackoverflow.com/questions/1758762/how-to-create-image-with-rounded-corners-in-c
                        int radius = 20;
                        using (var pfp = new Bitmap(pfpImage.Width, pfpImage.Height))
                        {
                            using (Graphics gPfp = Graphics.FromImage(pfp))
                            {
                                Brush brush = new TextureBrush(pfpImage);
                                GraphicsPath gp = new GraphicsPath();
                                gp.AddArc(0, 0, radius, radius, 180, 90);
                                gp.AddArc(0 + pfpImage.Width - radius, 0, radius, radius, 270, 90);
                                gp.AddArc(0 + pfp.Width - radius, 0 + pfp.Height - radius, radius, radius, 0, 90);
                                gp.AddArc(0, 0 + pfp.Height - radius, radius, radius, 90, 90);
                                gPfp.FillPath(brush, gp);
                            }
                            g.DrawImage(pfp, 20, 20);
                        }

                        SizeF usernameSize = g.MeasureString(me.GetUsername(), new Font("Arial", 23));
                        g.DrawString(me.GetUsername(), new Font("Arial", 23), Brushes.Black, 170f, 70f, StringFormat.GenericDefault);
                        g.DrawString("#" + me.GetDiscriminator(), new Font("Arial", 17), Brushes.Black, 170f + usernameSize.Width - 8f, 78f, StringFormat.GenericDefault);
                        g.DrawString(me.GetDescription(), new Font("Arial", 15), Brushes.Black, 20f, 200f, StringFormat.GenericDefault);
                        g.DrawString("Friends: " + me.GetFriendsCount(), new Font("Arial", 20), Brushes.Black, 460f, 15f, StringFormat.GenericDefault);
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

        [Command("Color")]
        public async Task ColorCmd(params string[] args)
        {
            var me = Program.p.cm.GetProfile(Context.User.Id);
            if (me == null)
            {
                await ReplyAsync("You don't have a profile. You must at first generate it with the 'Profile' command.");
                return;
            }
            if (await me.UpdateColor(args))
            {
                await ReplyAsync("Your profile background color was updated.");
            }
            else
            {
                await ReplyAsync("This color does not exist.");
            }
        }
    }
}
