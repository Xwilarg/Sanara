namespace SanaraV3.Game.Preload.Impl.Static
{
    public static class Common
    {
        public static string RemoveAccents(string input)
            => input.Replace("µ", "mu").Replace('ö', 'o').Replace('Ö', 'O').Replace('é', 'e').Replace('É', 'E').Replace('â', 'a').Replace('Â', 'A').Replace('è', 'e')
                .Replace("ō", "ou").Replace("ū", "uu").Replace("á", "a").Replace("ú", "u").Replace("ó", "o").Replace("ð", "d").Replace("&Amp;", "&").Replace("&#39;", "'");
    }
}
