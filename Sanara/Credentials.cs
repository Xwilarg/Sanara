namespace Sanara
{
    public record Credentials
    {
        public BotToken BotToken { set; get; } = new();
        public string UploadWebsiteUrl { set; get; }
        public string UploadWebsiteLocation { set; get; }
        public string SentryKey { set; get; }
        public string TopGgToken { set; get; }
        public string MyDramaListApiKey { set; get; }
        public ulong DebugGuild { set; get; }
        /// <summary>
        /// Need to be created on Google console
        /// </summary>
        public string GoogleProjectId { set; get; }
        public string VndbToken { set; get; }
        public GelbooruAuth Gelbooru { set; get; }
    }

    public record GelbooruAuth
    {
        public string UserId { set; get; }
        public string ApiKey { set; get; }
    }

    public record BotToken
    {
        public string DiscordToken { set; get; }
        public string RevoltToken { set; get; }
    }
}
