using Newtonsoft.Json;

namespace SanaraV3.Game.Preload.Result
{
    public struct QuizzPreloadResult : IPreloadResult
    {
        public QuizzPreloadResult(string imageUrl, string[] answers)
        {
            ImageUrl = imageUrl;
            Answers = answers;
            id = answers[0];
        }

        [JsonProperty]
        public string id;

        [JsonProperty]
        public string ImageUrl; // URL to the image

        [JsonProperty]
        public string[] Answers;  // Possible answers
    }
}
