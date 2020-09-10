namespace SanaraV3.Games.Preload.Result
{
    public struct QuizzPreloadResult : IPreloadResult
    {
        public QuizzPreloadResult(string imageUrl, string[] answers)
        {
            ImageUrl = imageUrl;
            Answers = answers;
        }

        public string ImageUrl { get; } // URL to the image
        public string[] Answers { get; }  // Possible answers
    }
}
