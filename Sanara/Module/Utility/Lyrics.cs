using System.Text;
using HtmlAgilityPack;

namespace Sanara.Module.Utility;

public class Lyrics
{
    public enum DisplayMode
    {
        Kanji,
        Hiragana,
        Romaji
    }

    public static async Task<string> GetRawLyricsAsync(HtmlDocument html, DisplayMode mode)
    {
        var strTarget = mode == DisplayMode.Romaji ? "romaji" : "hiragana";
        var lyrics = html.DocumentNode.SelectSingleNode($"//div[contains(@class, '{strTarget}')]");
        
        StringBuilder res = new();
        foreach (var child in lyrics.ChildNodes)
        {
            if (child.NodeType == HtmlNodeType.Text)
            {
                res.Append(child.InnerText);
            }
            else if (child.Name == "br")
            {
                //res.AppendLine();
            }
            else
            {
                if (mode == DisplayMode.Kanji) res.Append(child.ChildNodes[0].InnerHtml);
                else res.Append(child.ChildNodes[1].InnerHtml);
            }
            res.Append(' ');
        }
        return res.ToString();
    }
}
