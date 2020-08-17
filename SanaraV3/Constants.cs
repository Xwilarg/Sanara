namespace SanaraV3
{
    public static class Constants
    {
        public static readonly int PROGRAM_TIMEOUT = 300000; // (5 min) Time in ms where the program would exit itself if it didn't start

        // MEDIA
        public static readonly int YOUTUBE_DESC_MAX_SIZE = 10; // Max nb of line for YouTube embed description

        // DIAPORAMA
        public static readonly string[] DIAPORAMA_EMOTES = new[] { "◀️", "▶️", "⏪", "⏩" }; // Emotes used by the diaporama features
    }
}
