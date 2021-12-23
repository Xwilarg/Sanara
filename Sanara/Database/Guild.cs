using Newtonsoft.Json;

namespace Sanara.Database
{
    public class Guild
    {
        public Guild(string id)
        {
            this.id = id;
            _scores = new();
        }

        [JsonProperty]
#if NSFW_BUILD
        public string Prefix = "s.";
#else
        public string Prefix = "h.";
#endif

        [JsonProperty]
        public string id;

        [JsonProperty]
        public bool Anonymize = false;

        [JsonProperty]
        public string[] AvailabilityModules = Array.Empty<string>();

        [JsonProperty]
        public bool TranslateUsingFlags = false;

        // We can't serialize scores to keep compatibility with SanaraV2 db
        public bool DoesContainsGame(string name)
            => _scores.ContainsKey(name);

        public int GetScore(string name)
            => _scores[name];

        public void UpdateScore(string name, int score)
        {
            if (_scores.ContainsKey(name))
                _scores[name] = score;
            else
                _scores.Add(name, score);
        }

        private Dictionary<string, int> _scores;
    }
}
