﻿namespace Sanara
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

        public enum TimestampInfo
        {
            None,
            TimeAgo,
            OnlyDate
        }

        public static string ToDiscordTimestamp(DateTime dt, TimestampInfo info)
        {
            var secs = (int)(dt - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            if (info == TimestampInfo.TimeAgo)
            {
                return $"<t:{secs}:R>";
            }
            if (info == TimestampInfo.OnlyDate)
            {
                return $"<t:{secs}:D>";
            }
            return $"<t:{secs}>";
        }
    }
}
