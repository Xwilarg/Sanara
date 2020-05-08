using Discord;
using Discord.Commands;
using SanaraV2.Modules.Base;
using System.IO;
using System.Threading.Tasks;

namespace SanaraV2.Community
{
    [Group("Profile"), Alias("P")]
    public class CommunityModule : ModuleBase
    {
        [Command(""), Priority(-1)]
        public async Task ProfileDefault(params string[] args)
            => await Profile(args);

        [Command("Show"), Alias("Get")]
        public async Task Profile(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Community);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Community);
            Profile profile;
            if (args.Length == 0)
            {
                profile = Program.p.cm.GetOrCreateProfile(Context.User);
                Program.p.cm.GenerateProfile(profile, Context.User);
            }
            else
            {
                profile = await GetProfileAsync(args);
                if (profile == null)
                    return;
                if (profile.GetId() != Context.User.Id)
                {
                    Visibility v = profile.GetVisibility();
                    var me = Program.p.cm.GetProfile(Context.User.Id);
                    if (v == Visibility.Private)
                    {
                        await ReplyAsync("This profile is private.");
                        return;
                    }
                    if (v == Visibility.FriendsOnly && (me == null || !me.IsIdFriend(profile.GetId())))
                    {
                        await ReplyAsync("You must be friend with this user to see his/her profile.");
                        return;
                    }
                }
                if (!File.Exists("Saves/Profiles/" + profile.GetId() + ".png")) // If somehow the profile card is no longer available
                    Program.p.cm.GenerateProfile(profile, null);
            }
            await Context.Channel.SendFileAsync("Saves/Profiles/" + profile.GetId() + ".png");
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
            if (args.Length == 0)
            {
                await ReplyAsync("You must give the color you want.");
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

        [Command("Visibility")]
        public async Task VisibilityCmd(params string[] args)
        {
            var me = Program.p.cm.GetProfile(Context.User.Id);
            if (me == null)
            {
                await ReplyAsync("You don't have a profile. You must at first generate it with the 'Profile' command.");
                return;
            }
            string visibility = string.Join("", args).ToLower().Replace(" ", "");
            if (visibility == "public")
                me.UpdateVisibility(Visibility.Public);
            else if (visibility == "friendsonly" || visibility == "friendonly")
                me.UpdateVisibility(Visibility.FriendsOnly);
            else if (visibility == "private")
                me.UpdateVisibility(Visibility.Private);
            else
            {
                await ReplyAsync("Your profile visibility must be 'Public', 'Friends Only' or 'Private'.");
            }
            await ReplyAsync("Your visibility preference were updated.");
        }

        [Command("Unfriend"), Alias("Remove friend")]
        public async Task Unfriend(params string[] args)
        {
            var me = Program.p.cm.GetProfile(Context.User.Id);
            if (me == null)
            {
                await ReplyAsync("You don't have a profile. You must at first generate it with the 'Profile' command.");
                return;
            }
            if (args.Length == 0)
            {
                await ReplyAsync("You must give the user you want to unfriend.");
                return;
            }
            var profile = await GetProfileAsync(args);
            if (profile == null)
                return;
            if (profile.GetId() == Context.User.Id)
            {
                await ReplyAsync("You can't unfriend yourself.");
                return;
            }
            if (me.RemoveFriend(profile))
            {
                await ReplyAsync("You are no longer friend with " + profile.GetUsername());
            }
            else
            {
                await ReplyAsync(profile.GetUsername() + " is not your friend.");
            }
        }


        [Command("Friend"), Alias("Add friend")]
        public async Task Friend(params string[] args)
        {
            var me = Program.p.cm.GetProfile(Context.User.Id);
            if (me == null)
            {
                await ReplyAsync("You don't have a profile. You must at first generate it with the 'Profile' command.");
                return;
            }
            if (args.Length == 0)
            {
                await ReplyAsync("You must give the user you want to be friend with.");
                return;
            }
            var profile = await GetProfileAsync(args);
            if (profile == null)
                return;
            if (profile.GetId() == Context.User.Id)
            {
                await ReplyAsync("You can't add yourself as a friend.");
                return;
            }
            if (me.IsIdFriend(profile.GetId()))
            {
                await ReplyAsync(profile.GetUsername() + " is already your friend.");
                return;
            }
            if (!await Program.p.cm.AddFriendRequestAsync(Context.Channel, me, profile))
            {
                await ReplyAsync("You already have an active friend request with " + profile.GetUsername() + ".");
            }
        }

        private async Task<Profile> GetProfileAsync(string[] args)
        {
            Profile profile;
            var user = await Utilities.GetUser(string.Join(" ", args), Context.Guild);
            if (user == null)
            {
                if (!ulong.TryParse(string.Join(" ", args), out ulong userId))
                    profile = Program.p.cm.GetProfile(userId);
                else
                    profile = Program.p.cm.GetProfile(string.Join(" ", args));
            }
            else
                profile = Program.p.cm.GetProfile(user.Id);
            if (user == null)
            {
                await ReplyAsync("This user does not exist.");
                return null;
            }
            if (profile == null)
            {
                await ReplyAsync("This user does not have a profile.");
                return null;
            }
            return profile;
        }
    }
}
