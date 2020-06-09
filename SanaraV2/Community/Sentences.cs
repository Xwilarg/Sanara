using Discord;
using SanaraV2.Modules.Base;

namespace SanaraV2.Community
{
    public static class Sentences
    {
        public static string FriendAccepted(IGuild guild, string receiver, string sender) { return (Translation.GetTranslation(guild, "friendAccepted", receiver, sender)); }
        public static string FriendRequest(IGuild guild, string receiver, string sender) { return (Translation.GetTranslation(guild, "friendRequest", receiver, sender)); }
        public static string ErrorPrivate(IGuild guild) { return (Translation.GetTranslation(guild, "errorPrivate")); }
        public static string ErrorFriendsOnly(IGuild guild) { return (Translation.GetTranslation(guild, "errorFriendsOnly")); }
        public static string NeedProfile(IGuild guild) { return (Translation.GetTranslation(guild, "needProfile")); }
        public static string DescriptionUpdated(IGuild guild) { return (Translation.GetTranslation(guild, "descriptionUpdated")); }
        public static string DescriptionTooLong(IGuild guild) { return (Translation.GetTranslation(guild, "descriptionTooLong")); }
        public static string VisibilityHelp(IGuild guild) { return (Translation.GetTranslation(guild, "visibilityHelp")); }
        public static string VisibilityUpdated(IGuild guild) { return (Translation.GetTranslation(guild, "visibilityUpdated")); }
        public static string UnfriendHelp(IGuild guild) { return (Translation.GetTranslation(guild, "unfriendHelp")); }
        public static string UnfriendYourself(IGuild guild) { return (Translation.GetTranslation(guild, "unfriendYourself")); }
        public static string UnfriendDone(IGuild guild, string user) { return (Translation.GetTranslation(guild, "unfriendDone", user)); }
        public static string UnfriendError(IGuild guild, string user) { return (Translation.GetTranslation(guild, "unfriendError", user)); }
        public static string FriendHelp(IGuild guild) { return (Translation.GetTranslation(guild, "friendHelp")); }
        public static string FriendYourself(IGuild guild) { return (Translation.GetTranslation(guild, "friendYourself")); }
        public static string FriendAlreadyActive(IGuild guild, string user) { return (Translation.GetTranslation(guild, "friendAlreadyActive", user)); }
        public static string FriendError(IGuild guild, string user) { return (Translation.GetTranslation(guild, "friendError", user)); }
        public static string ColorUpdated(IGuild guild) { return (Translation.GetTranslation(guild, "colorUpdated")); }
        public static string UserNotFound(IGuild guild) { return (Translation.GetTranslation(guild, "userNotFound")); }
        public static string ProfileNotFound(IGuild guild) { return (Translation.GetTranslation(guild, "profileNotFound")); }
    }
}
