using BooruSharp.Booru;

namespace SanaraV3.Game.Preload.Result
{
    public sealed class BooruQuizzPreloadResult : QuizzPreloadResult
    {
        public BooruQuizzPreloadResult(ABooru booru, string[] allowedFormats, string imageUrl, string[] answers) : base(imageUrl, answers)
        {
            Booru = booru;
            AllowedFormats = allowedFormats;
        }

        public ABooru Booru { get; }
        public string[] AllowedFormats { get; }
    }
}
