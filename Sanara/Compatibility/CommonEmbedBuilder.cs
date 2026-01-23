using Discord;
using System.Dynamic;
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

    private string GetEmbedDescription(bool includeDescription, int maxFieldCount = -1)
    {
        StringBuilder str = new();
        if (Title != null)
        {
            if (Url != null) str.Append($"### [{Title}]({Url})");
            else str.Append($"### {Title}");
        }

        if (Description != null && includeDescription)
        {
            if (Title != null)
            {
                str.AppendLine();
                str.AppendLine();
            }

            str.Append(Description);
        }

        if (_fields.Count > 0)
        {
            if (str.ToString() != string.Empty)
            {
                str.AppendLine();
            }

            str.Append(string.Join("\n\n", _fields.Take(maxFieldCount == -1 ? _fields.Count : maxFieldCount).Select(x =>
            {
                return $"#### {x.Name}\n{x.Content}";
            })));

            if (!includeDescription && str.ToString().Length > 1000 && maxFieldCount > 2)
            {
                return GetEmbedDescription(false, maxFieldCount - 1);
            }
        }

        return str.ToString();
    }

    public StoatSharp.Embed ToRevolt()
    {
        var desc = GetEmbedDescription(true);
        if (desc.Length > 1000) desc = GetEmbedDescription(false, _fields.Count);
        if (string.IsNullOrWhiteSpace(desc)) return null;

        return new StoatSharp.EmbedBuilder()
        {
            Description = desc,
            Color = Color == null ? null : new StoatSharp.StoatColor(Color.Value.R, Color.Value.G, Color.Value.B),
            Image = null
        }.Build();
    }
}

public record CommonEmbedField
{
    public string Name;
    public string Content;
    public bool IsInline = false;
}
