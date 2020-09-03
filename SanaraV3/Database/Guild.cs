using Newtonsoft.Json;

namespace SanaraV3.Database
{
    public class Guild
    {
        public Guild(string id)
        {
            this.id = id;
        }

        [JsonProperty]
        public string Prefix = "s.";

        [JsonProperty]
        public string id;
    }
}
