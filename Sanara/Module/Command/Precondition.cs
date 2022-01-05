namespace Sanara.Module.Command
{
    [Flags]
    public enum Precondition
    {
        None = 0,

        /// <summary>
        /// Can only be done in channels marked as NSFW
        /// </summary>
        NsfwOnly = 1,

        /// <summary>
        /// Can only be done by an admin or someone how have "Manage Guild" permission
        /// </summary>
        AdminOnly = 2,

        /// <summary>
        /// Can't be done in private message
        /// </summary>
        GuildOnly = 4
    }
}
