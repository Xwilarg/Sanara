using System.Text.RegularExpressions;

namespace Sanara
{
    public static class Utils
    {
        public static string PadNumber(int nb)
            => nb > 9 ? $"{nb}" : $"0{nb}";

        public static bool EasyCompare(string a, string b)
        {
            string va, vb;
            if (a.Any(x => char.IsLetterOrDigit(x)) || b.Any(x => char.IsLetterOrDigit(x)))
            {
                va = CleanWord(a);
                vb = CleanWord(b);
            }
            else
            {
                va = a;
                vb = b;
            }
            return va == vb || (va[^1] == 's' && va[..^1] == vb) || (vb[^1] == 's' && vb[..^1] == va);
        }

        public static string CleanWord(string word)
            => string.Join("", word.Where(c => char.IsLetterOrDigit(c))).ToLowerInvariant();

        public static string ToWordCase(string word)
            => char.ToUpper(word[0]) + string.Join("", word.Skip(1)).ToLower();

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

        public static int GCD(int a, int b)
        {
            return b == 0 ? Math.Abs(a) : GCD(b, a % b);
        }

        // From: https://gist.github.com/Davidblkx/e12ab0bb2aff7fd8072632b396538560
        public static int GetStringDistance(string a, string b)
        {
            var source1Length = a.Length;
            var source2Length = b.Length;

            var matrix = new int[source1Length + 1, source2Length + 1];

            // First calculation, if one entry is empty return full length
            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            // Initialization of matrix with row size source1Length and columns size source2Length
            for (var i = 0; i <= source1Length; i++)
                matrix[i, 0] = i;
            for (var j = 0; j <= source2Length; j++)
                matrix[0, j] = j;

            // Calculate rows and columns distances
            for (var i = 1; i <= source1Length; i++)
            {
                for (var j = 1; j <= source2Length; j++)
                {
                    var cost = (b[j - 1] == a[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            return matrix[source1Length, source2Length];
        }

        public static string? CleanHtml(string msg)
        {
            if (msg == null) return null;
            msg = Regex.Replace(msg, "<br *\\/>", "\n");
            msg = Regex.Replace(msg, "<\\/br>", "\n");
            msg = Regex.Replace(msg, "<b>([^<]+)<\\/b>", "**$1**");
            msg = Regex.Replace(msg, "<strong>([^<]+)<\\/strong>", "**$1**");
            msg = Regex.Replace(msg, "<a href=\"([^\"]+)\">([^<]+)<\\/a>", "[$2]($1)");
            msg = Regex.Replace(msg, "<[^>]+>([^<]+)<\\/[^>]+>", "$1");
            msg = Regex.Replace(msg, "<\\/?[^>]+>", "");
            return msg;
        }
    }
}
