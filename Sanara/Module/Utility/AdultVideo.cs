using System.Text.RegularExpressions;

namespace Sanara.Module.Utility
{
    public class AdultVideo
    {
        public static async Task<string> DoJavmostHttpRequestAsync(string url)
        {
            int redirectCounter = 0;
            string html;
            HttpRequestMessage request = new(new HttpMethod("GET"), url);
            do
            {
                request.Headers.Add("Host", "www.javmost.cx");
                html = await (await StaticObjects.HttpClient.SendAsync(request)).Content.ReadAsStringAsync();
                Match redirect = Regex.Match(html, "<p>The document has moved <a href=\"([^\"]+)\">");
                if (redirect.Success)
                    request = new HttpRequestMessage(new HttpMethod("GET"), redirect.Groups[1].Value);
                else
                    break;
                redirectCounter++;
            } while (redirectCounter < 10);
            return html;
        }
    }
}
