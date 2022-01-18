namespace Sanara.UnitTests
{
    public class Utils
    {
        public static async Task<bool> IsLinkValidAsync(string url)
        {
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                using (HttpClient hc = new())
                {
                    var response = await hc.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                    return response.IsSuccessStatusCode;
                }
            }
            return false;
        }
    }
}
