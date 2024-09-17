namespace Sanara
{
    public record Credentials
    {
        public string BotToken;
        public string UploadWebsiteUrl;
        public string UploadWebsiteLocation;
        public string SentryKey;
        public string TopGgToken;
        public string MyDramaListApiKey;
        public string DebugGuild;
        /// <summary>
        /// Need to be created on Google console
        /// </summary>
        public string GoogleProjectId { set; get; }
    }
}
