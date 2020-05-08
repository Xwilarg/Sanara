using SanaraV2.Modules.Base;

namespace SanaraV2.Community
{
    public static class Sentences
    {
        public static string FriendAccepted(ulong guildId, string receiver, string sender) { return (Translation.GetTranslation(guildId, "friendAccepted", receiver, sender)); }
        public static string FriendRequest(ulong guildId, string receiver, string sender) { return (Translation.GetTranslation(guildId, "friendRequest", receiver, sender)); }
        public static string ErrorPrivate(ulong guildId) { return (Translation.GetTranslation(guildId, "errorPrivate")); }
        public static string ErrorFriendsOnly(ulong guildId) { return (Translation.GetTranslation(guildId, "errorFriendsOnly")); }
        public static string NeedProfile(ulong guildId) { return (Translation.GetTranslation(guildId, "needProfile")); }
        public static string DescriptionUpdated(ulong guildId) { return (Translation.GetTranslation(guildId, "descriptionUpdated")); }
        public static string DescriptionTooLong(ulong guildId) { return (Translation.GetTranslation(guildId, "descriptionTooLong")); }
        public static string VisibilityHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "visibilityHelp")); }
        public static string VisibilityUpdated(ulong guildId) { return (Translation.GetTranslation(guildId, "visibilityUpdated")); }
        public static string UnfriendHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "unfriendHelp")); }
        public static string UnfriendYourself(ulong guildId) { return (Translation.GetTranslation(guildId, "unfriendYourself")); }
        public static string UnfriendDone(ulong guildId, string user) { return (Translation.GetTranslation(guildId, "unfriendDone", user)); }
        public static string UnfriendError(ulong guildId, string user) { return (Translation.GetTranslation(guildId, "unfriendError", user)); }
        public static string FriendHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "friendHelp")); }
        public static string FriendYourself(ulong guildId) { return (Translation.GetTranslation(guildId, "friendYourself")); }
        public static string FriendAlreadyActive(ulong guildId, string user) { return (Translation.GetTranslation(guildId, "friendAlreadyActive", user)); }
        public static string FriendError(ulong guildId, string user) { return (Translation.GetTranslation(guildId, "friendError", user)); }
        public static string ColorUpdated(ulong guildId) { return (Translation.GetTranslation(guildId, "colorUpdated")); }
        public static string UserNotFound(ulong guildId) { return (Translation.GetTranslation(guildId, "userNotFound")); }
        public static string ProfileNotFound(ulong guildId) { return (Translation.GetTranslation(guildId, "profileNotFound")); }
    }
}
