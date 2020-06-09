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
            Utilities.CheckAvailability(Context.Guild, Program.Module.Community);
            await Program.p.DoAction(Context.User, Program.Module.Community);
            Profile profile;
            if (args.Length == 0)
            {
                profile = Program.p.cm.GetOrCreateProfile(Context.Message, Context.User);
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
                        await ReplyAsync(Sentences.ErrorPrivate(Context.Guild));
                        return;
                    }
                    if (v == Visibility.FriendsOnly && (me == null || !me.IsIdFriend(profile.GetId())))
                    {
                        await ReplyAsync(Sentences.ErrorFriendsOnly(Context.Guild));
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
            Utilities.CheckAvailability(Context.Guild, Program.Module.Community);
            await Program.p.DoAction(Context.User, Program.Module.Community);
            var me = Program.p.cm.GetProfile(Context.User.Id);
            if (me == null)
            {
                await ReplyAsync(Sentences.NeedProfile(Context.Guild));
                return;
            }
            if (me.UpdateDescription(string.Join(" ", args)))
            {
                await ReplyAsync(Sentences.DescriptionUpdated(Context.Guild));
            }
            else
            {
                await ReplyAsync(Sentences.DescriptionTooLong(Context.Guild));
            }
        }

        [Command("Color")]
        public async Task ColorCmd(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Community);
            await Program.p.DoAction(Context.User, Program.Module.Community);
            var me = Program.p.cm.GetProfile(Context.User.Id);
            if (me == null)
            {
                await ReplyAsync(Sentences.NeedProfile(Context.Guild));
                return;
            }
            if (args.Length == 0)
            {
                await ReplyAsync(Modules.Tools.Sentences.HelpColor(Context.Guild));
                return;
            }
            if (await me.UpdateColor(args))
            {
                await ReplyAsync(Sentences.ColorUpdated(Context.Guild));
            }
            else
            {
                await ReplyAsync(Modules.Tools.Sentences.InvalidColor(Context.Guild));
            }
        }

        [Command("Visibility")]
        public async Task VisibilityCmd(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Community);
            await Program.p.DoAction(Context.User, Program.Module.Community);
            var me = Program.p.cm.GetProfile(Context.User.Id);
            if (me == null)
            {
                await ReplyAsync(Sentences.NeedProfile(Context.Guild));
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
                await ReplyAsync(Sentences.VisibilityHelp(Context.Guild));
            }
            await ReplyAsync(Sentences.VisibilityUpdated(Context.Guild));
        }

        [Command("Unfriend"), Alias("Remove friend")]
        public async Task Unfriend(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Community);
            await Program.p.DoAction(Context.User, Program.Module.Community);
            var me = Program.p.cm.GetProfile(Context.User.Id);
            if (me == null)
            {
                await ReplyAsync(Sentences.NeedProfile(Context.Guild));
                return;
            }
            if (args.Length == 0)
            {
                await ReplyAsync(Sentences.UnfriendHelp(Context.Guild));
                return;
            }
            var profile = await GetProfileAsync(args);
            if (profile == null)
                return;
            if (profile.GetId() == Context.User.Id)
            {
                await ReplyAsync(Sentences.UnfriendYourself(Context.Guild));
                return;
            }
            if (me.RemoveFriend(profile))
            {
                await ReplyAsync(Sentences.UnfriendDone(Context.Guild, profile.GetUsername()));
            }
            else
            {
                await ReplyAsync(Sentences.UnfriendError(Context.Guild, profile.GetUsername()));
            }
        }


        [Command("Friend"), Alias("Add friend")]
        public async Task Friend(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Community);
            await Program.p.DoAction(Context.User, Program.Module.Community);
            var me = Program.p.cm.GetProfile(Context.User.Id);
            if (me == null)
            {
                await ReplyAsync(Sentences.NeedProfile(Context.Guild));
                return;
            }
            if (args.Length == 0)
            {
                await ReplyAsync(Sentences.FriendHelp(Context.Guild));
                return;
            }
            var profile = await GetProfileAsync(args);
            if (profile == null)
                return;
            if (profile.GetId() == Context.User.Id)
            {
                await ReplyAsync(Sentences.FriendYourself(Context.Guild));
                return;
            }
            if (me.IsIdFriend(profile.GetId()))
            {
                await ReplyAsync(Sentences.FriendError(Context.Guild, profile.GetUsername()));
                return;
            }
            if (!await Program.p.cm.AddFriendRequestAsync(Context.Channel, me, profile))
            {
                await ReplyAsync(Sentences.FriendAlreadyActive(Context.Guild, profile.GetUsername()));
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

        [Command("Save all")]
        public async Task SaveAll(params string[] _)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Community);
            await Program.p.DoAction(Context.User, Program.Module.Community);
            if (Context.User.Id != Modules.Base.Sentences.ownerId)
                await ReplyAsync(Modules.Base.Sentences.OnlyMasterStr(Context.Guild));
            else
            {
                Program.p.cm.UpdateAllProfiles();
                await ReplyAsync(Modules.Base.Sentences.DoneStr(Context.Guild));
            }
        }
    }
}
