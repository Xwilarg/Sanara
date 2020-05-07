﻿using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV2.Community
{
    public class CommunityManager
    {
        public CommunityManager()
        {
            if (!Directory.Exists("Saves/Assets"))
                Directory.CreateDirectory("Saves/Assets");
            if (Directory.Exists("../../Assets"))
                foreach (var file in Directory.GetFiles("../../Assets"))
                {
                    var fi = new FileInfo(file);
                    File.Copy(file, "Saves/Assets/" + fi.Name, true);
                }
            _profiles = new Dictionary<ulong, Profile>();
        }

        public void GenerateProfile(Profile profile, Discord.IUser user, Image pfpImage = null)
        {
            profile.UpdateProfile(user);
            using (var model = new Bitmap("Saves/Assets/Background.png"))
            {
                using (var bp = new Bitmap(model.Width, model.Height))
                {
                    using (var g = Graphics.FromImage(bp))
                    {
                        g.DrawImage(model, 0, 0);
                        Color color = profile.GetBackgroundColor();
                        Brush backgroundBrush = new SolidBrush(Color.FromArgb(50, color.R, color.G, color.B));
                        g.FillRectangle(backgroundBrush, 0, 0, model.Width, model.Height);
                        if (user != null)
                        {
                            if (pfpImage == null) pfpImage = profile.GetProfilePicture(user);
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
                        }

                        SizeF usernameSize = g.MeasureString(profile.GetUsername(), new Font("Arial", 23));
                        g.DrawString(profile.GetUsername(), new Font("Arial", 23), Brushes.Black, 170f, 70f, StringFormat.GenericDefault);
                        g.DrawString("#" + profile.GetDiscriminator(), new Font("Arial", 17), Brushes.Black, 170f + usernameSize.Width - 8f, 78f, StringFormat.GenericDefault);
                        g.DrawString(profile.GetDescription(), new Font("Arial", 15), Brushes.Black, 20f, 200f, StringFormat.GenericDefault);
                        g.DrawString("Friends: " + profile.GetFriendsCount(), new Font("Arial", 20), Brushes.Black, 460f, 15f, StringFormat.GenericDefault);
                    }
                    bp.Save("Saves/Profiles/" + user.Id + ".png");
                }
            }
        }

        public void AddProfile(ulong id, Profile p) => _profiles.Add(id, p);
        public Profile GetOrCreateProfile(Discord.IUser user)
        {
            if (_profiles.ContainsKey(user.Id)) return _profiles[user.Id];
            Profile p = new Profile(user);
            _profiles.Add(user.Id, p);
            Program.p.db.AddProfile(p);
            return p;
        }
        public Profile GetProfile(ulong id)
        {
            if (_profiles.ContainsKey(id)) return _profiles[id];
            return null;
        }
        public Profile GetProfile(string user)
        {
            return _profiles.Select(x => x.Value).Where(x => user == x.GetUsername() || user == x.GetUsername() + "#" + x.GetDiscriminator()).FirstOrDefault();
        }
        private Dictionary<ulong, Profile> _profiles;

        public async Task CreateMyProfile()
        {
            Profile p = GetOrCreateProfile(Program.p.client.CurrentUser);
            await p.UpdateColor(new[] { "green" });
            p.UpdateDescription("Welcome on my profile!\n\nIf you need any help feel free to check:\n - Website: https://sanara.zirk.eu\n - Official Server: https://discord.gg/H6wMRYV\n - GitHub: https://github.com/Xwilarg/Sanara");
            p.UpdateVisibility(Visibility.Public);
            using (HttpClient hc = new HttpClient()) // We can't get the bot profile picture from CurrentUser so we need to hardcode it
                GenerateProfile(p, Program.p.client.CurrentUser, Image.FromStream(await hc.GetStreamAsync("https://cdn.discordapp.com/avatars/329664361016721408/620b71193a2769a44f06eee302ddf0bf.png?size=128")));
        }
    }
}
