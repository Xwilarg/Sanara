using Discord;
using System.Text;

namespace Sanara.Compatibility;

public class CommonEmbedBuilder
{
    private readonly List<CommonEmbedField> _fields = new();

    public void AddField(string name, string content, bool isInline = false)
    {
        _fields.Add(new() { Name = name, Content = content, IsInline = isInline });
    }

    public void WithFooter(string content)
    {
        _footer = content;
    }

    public string? Title { set; get; }
    public string? Description { set; get; }
    public Color? Color { set; get; }
    public string? ImageUrl { set; get; }
    public string? Url { set; get; }

    private string _footer;

    public Discord.Embed ToDiscord()
    {
        return new EmbedBuilder()
        {
            Title = Title,
            Description = Description,
            Color = Color,
            ImageUrl = ImageUrl,
            Url = Url,

            Fields = _fields.Select(x => new EmbedFieldBuilder
            {
                Name = x.Name,
                Value = x.Content,
                IsInline = x.IsInline
            }).ToList(),

            Footer = _footer == null ? null : new EmbedFooterBuilder
            {
                Text = _footer
            }
        }.Build();
    }

    public RevoltSharp.Embed ToRevolt()
    {
        StringBuilder str = new();
        if (Title != null)
        {
            if (Url != null) str.Append($"### [{Title}]({Url})");
            else str.Append($"### {Title}");
        }

        if (Description != null)
        {
            if (Title != null)
            {
                str.AppendLine();
                str.AppendLine();
            }

            str.Append(Description);
        }

        return new RevoltSharp.EmbedBuilder()
        {
            Description = str.ToString(),
            Color = Color == null ? null : new RevoltSharp.RevoltColor(Color.Value.R, Color.Value.G, Color.Value.B),
            Image = ImageUrl
        }.Build();
    }
}

public record CommonEmbedField
{
    public string Name;
    public string Content;
    public bool IsInline = false;
}
