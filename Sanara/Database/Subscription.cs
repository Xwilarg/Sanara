using Newtonsoft.Json;

namespace Sanara.Database
{
    public class Subscription
    {
        public Subscription(string id, int value)
        {
            this.id = id;
            this.value = value;
        }

        [JsonProperty]
        public string id;

        [JsonProperty]
        public int value;
    }
}
