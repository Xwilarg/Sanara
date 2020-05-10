using Discord.WebSocket;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
            _friendRequests = new Dictionary<ulong, FriendRequest>();
            _thread = new Thread(new ThreadStart(Update));
            _thread.Start();
        }

        private void Update() // We update all profiles in db every hour
        {
            while (Thread.CurrentThread.IsAlive)
            {
                Thread.Sleep(1800000);
                Thread.Sleep(1800000); // Wait one hour
                UpdateAllProfiles();
            }
        }

        public void UpdateAllProfiles()
        {
            var profiles = _profiles.Select(x => x.Value);
            for (int i = profiles.Count() - 1; i >= 0; i--)
                profiles.ElementAt(i).UpdateProfile();
        }

        public async Task ProgressAchievementAsync(AchievementID achievementID, int progression, string key, Discord.IUserMessage msg, ulong userId)
        {
            var profile = GetProfile(userId);
            if (profile != null) // Achievements don't progress if we don't have a profile
            {
                await profile.ProgressAchievementAsync(achievementID, msg, progression, key);
            }
        }

        public void GenerateProfile(Profile profile, Discord.IUser user, Image pfpImage = null)
        {
            if (user != null)
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
                        g.DrawString("Achievements: ", new Font("Arial", 20), Brushes.Black, 460f, 45f, StringFormat.GenericDefault);
                        int i = 0;
                        foreach (var a in profile.GetAchievements())
                        {
                            Brush medalBrush;
                            if (a.Item2 == 1) medalBrush = new SolidBrush(Color.FromArgb(184, 115, 51)); // Copper
                            else if (a.Item2 == 2) medalBrush = new SolidBrush(Color.FromArgb(196, 202, 206)); // Silver
                            else medalBrush = new SolidBrush(Color.FromArgb(255, 215, 0)); // Gold
                            int xPos = 460 + ((i % 4) * 70);
                            int yPos = 80 + ((i / 4) * 70);
                            g.FillPie(medalBrush, new Rectangle(xPos, yPos, 64, 64), 0, 360);
                            g.DrawImage(a.Item1, xPos, yPos);
                            i++;
                        }
                    }
                    bp.Save("Saves/Profiles/" + profile.GetId() + ".png");
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

        public async Task<bool> AddFriendRequestAsync(Discord.IMessageChannel chan, Profile author, Profile target)
        {
            if (_friendRequests.Any(x => x.Value.author == author && x.Value.destinator == target))
                return false;
            var textChan = (Discord.ITextChannel)chan;
            if (target.GetId() == Program.p.client.CurrentUser.Id)
            {
                var sMsg = await chan.SendMessageAsync(Sentences.FriendAccepted(textChan.GuildId, target.GetUsername() + "#" + target.GetDiscriminator(), author.GetUsername() + "#" + author.GetDiscriminator()));
                await sMsg.AddReactionAsync(new Discord.Emoji("✅"));
                await target.AddFriendAsync(author, sMsg);
                return true;
            }
            var msg = await chan.SendMessageAsync(Sentences.FriendRequest(textChan.GuildId, target.GetUsername() + "#" + target.GetDiscriminator(), author.GetUsername() + "#" + author.GetDiscriminator()));
            _friendRequests.Add(msg.Id, new FriendRequest { author = author, destinator = target });
            await msg.AddReactionAsync(new Discord.Emoji("✅"));
            await msg.AddReactionAsync(new Discord.Emoji("❌"));
            _ = Task.Run(async () =>
            {
                await Task.Delay(600000); // 10 minutes
                if (_friendRequests.ContainsKey(msg.Id))
                {
                    _friendRequests.Remove(msg.Id);
                    await msg.DeleteAsync();
                }
            });
            return true;
        }
        public async Task DeleteFriendRequestAsync(Discord.IUserMessage msg, ulong user)
        {
            if (_friendRequests.ContainsKey(msg.Id))
            {
                if (_friendRequests[msg.Id].author.GetId() == user || _friendRequests[msg.Id].destinator.GetId() == user)
                {
                    _friendRequests.Remove(msg.Id);
                    await msg.DeleteAsync();
                }
            }
        }
        public async Task AcceptFriendRequestAsync(Discord.IUserMessage msg, ulong user)
        {
            if (_friendRequests.ContainsKey(msg.Id))
            {
                if (_friendRequests[msg.Id].destinator.GetId() == user)
                {
                    var elem = _friendRequests[msg.Id];
                    await elem.destinator.AddFriendAsync(elem.author, msg);
                    var textChan = (Discord.ITextChannel)msg.Channel;
                    await msg.ModifyAsync(x => x.Content = Sentences.FriendAccepted(textChan.GuildId, elem.destinator.GetUsername() + "#" + elem.destinator.GetDiscriminator(), elem.author.GetUsername() + "#" + elem.author.GetDiscriminator()));
                    await msg.RemoveReactionAsync(new Discord.Emoji("✅"), Program.p.client.CurrentUser);
                    await msg.RemoveReactionAsync(new Discord.Emoji("❌"), Program.p.client.CurrentUser);
                    _friendRequests.Remove(msg.Id);
                }
            }
        }

        public async Task CreateMyProfile()
        {
            Profile p = GetOrCreateProfile(Program.p.client.CurrentUser);
            await p.UpdateColor(new[] { "green" });
            p.UpdateDescription("Welcome on my profile!\n\nIf you need any help feel free to check:\n - Website: https://sanara.zirk.eu\n - Official Server: https://discord.gg/H6wMRYV\n - GitHub: https://github.com/Xwilarg/Sanara");
            p.UpdateVisibility(Visibility.Public);
            using (HttpClient hc = new HttpClient()) // We can't get the bot profile picture from CurrentUser so we need to hardcode it
                GenerateProfile(p, Program.p.client.CurrentUser, Image.FromStream(await hc.GetStreamAsync("https://cdn.discordapp.com/avatars/329664361016721408/620b71193a2769a44f06eee302ddf0bf.png?size=128")));
        }

        public async Task ReactionAdded(Discord.Cacheable<Discord.IUserMessage, ulong> msg, ISocketMessageChannel chan, SocketReaction react)
        {
            string emote = react.Emote.ToString();
            if (react.UserId != Program.p.client.CurrentUser.Id && (emote == "✅" || emote == "❌"))
            {
                var downloadedMsg = await msg.GetOrDownloadAsync();
                if (downloadedMsg.Author.Id == Program.p.client.CurrentUser.Id)
                {
                    if (emote == "✅")
                        await AcceptFriendRequestAsync(downloadedMsg, react.UserId);
                    else if (emote == "❌")
                        await DeleteFriendRequestAsync(downloadedMsg, react.UserId);
                }
            }
        }

        private Dictionary<ulong, Profile> _profiles;
        private Dictionary<ulong, FriendRequest> _friendRequests;
        private Thread _thread;
    }
}
