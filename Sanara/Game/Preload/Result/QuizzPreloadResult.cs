using Newtonsoft.Json;

namespace Sanara.Game.Preload.Result
{
    public class QuizzPreloadResult : IPreloadResult
    {
        public QuizzPreloadResult(string imageUrl, string[] answers)
        {
            ImageUrl = imageUrl;
            Answers = answers;
            id = answers[0];
        }

        [JsonProperty]
        public string id;

        /// <summary>
        /// URL to the image
        /// </summary>
        [JsonProperty]
        public string ImageUrl;

        /// <summary>
        /// Possible answers
        /// </summary>
        [JsonProperty]
        public string[] Answers;

        public override string ToString()
        {
            return "Quizz Preload " + Answers[0];
        }
    }
}
