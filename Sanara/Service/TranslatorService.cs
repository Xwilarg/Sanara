namespace Sanara.Service;

public class TranslatorService
{
    public TranslatorService()
    {
        foreach (var elem in ISO639)
        {
            ISO639Reverse.Add(elem.Value, elem.Key);
        }
    }

    public Dictionary<string, string> ISO639 { set; get; } = new()
    {
        { "fr", "french" },
        { "en", "english" },
        { "ja", "japanese" },
        { "ru", "russian" },
        { "zh", "chinese" },
        { "ko", "korean" },
        { "ge", "german" },
        { "es", "spanish" },
        { "nl", "dutch" },
    };
    public Dictionary<string, string> Flags { set; get; } = new()
    {
        { "🇫🇷", "fr" },
        { "🇺🇸", "en" },
        { "🇬🇧", "en" },
        { "🇯🇵", "ja" },
        { "🇷🇺", "ru" },
        { "🇹🇼", "zh" },
        { "🇨🇳", "zh" },
        { "🇰🇷", "ko" },
        { "🇩🇪", "de" },
        { "🇪🇸", "es" },
        { "🇳🇱", "nl" }
    };
    public Dictionary<string, string> ISO639Reverse { set; get; } = [];

    public Dictionary<string, string> TranslationOriginalText { set; get; } = [];
}
