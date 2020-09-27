using Newtonsoft.Json;
using System.Collections.Generic;

namespace SanaraV3.Database
{
    public class Guild
    {
        public Guild(string id)
        {
            this.id = id;
            _scores = new Dictionary<string, int>();
        }

        [JsonProperty]
        public string Prefix = "s.";

        [JsonProperty]
        public string id;

        [JsonProperty]
        public bool Anonymize = false;

        [JsonProperty]
        public string[] AvailabilityModules = new string[0];

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
