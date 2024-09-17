namespace Sanara
{
    public record Credentials
    {
        public string BotToken { set; get; }
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
    }
}
