namespace Sanara
{
    public static class Constants
    {
        /// <summary>
        /// (5 min) Time in ms where the program would exit itself if it didn't start
        /// </summary>
        public static readonly int PROGRAM_TIMEOUT = 300_000;

        /// <summary>
        /// Max nb of line for YouTube embed description
        /// </summary>
        public static readonly int YOUTUBE_DESC_MAX_SIZE = 10;

        /// <summary>
        /// Emotes used by the diaporama features
        /// </summary>
        public static readonly string[] DIAPORAMA_EMOTES = new[] { "◀️", "▶️", "⏪", "⏩" };
    }
}
