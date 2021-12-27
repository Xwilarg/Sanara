namespace Sanara
{
    public static class Utils
    {
        public static string CleanWord(string word)
            => string.Join("", word.Where(c => char.IsLetterOrDigit(c)));

        public static async Task<bool> IsLinkValid(string url)
        {
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                var response = await StaticObjects.HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                return response.IsSuccessStatusCode;
            }
            return false;
        }

        public static bool IsImage(string extension)
        {
            extension = extension.ToLowerInvariant();
            if (extension[0] == '.') extension = extension[1..];
            return (extension.StartsWith("gif") || extension.StartsWith("png") || extension.StartsWith("jpg")
                || extension.StartsWith("jpeg"));
        }

        public static string ToDiscordTimestamp(DateTime dt, bool useTimeAgo)
        {
            var secs = (int)(dt - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            if (useTimeAgo)
            {
                return $"<t:{secs}:R>";
            }
            return $"<t:{secs}>";
        }
    }
}
