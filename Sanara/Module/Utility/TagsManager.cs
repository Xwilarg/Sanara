using BooruSharp.Booru;
using BooruSharp.Search;

namespace Sanara.Module.Utility;

public class TagsManager
{
    /// <summary>
    /// Associate an id with a booru and a post
    /// </summary>
    private readonly Dictionary<string, Tuple<ABooru, BooruSharp.Search.Post.SearchResult>> _tags = new();

    public void AddTag(string id, ABooru booru, BooruSharp.Search.Post.SearchResult post)
    {
        if (!_tags.ContainsKey(id))
            _tags.Add(id, new Tuple<ABooru, BooruSharp.Search.Post.SearchResult>(booru, post));
    }

    public bool ContainsTag(string id) => _tags.ContainsKey(id);

    public async Task<TagsSearch?> GetTagAsync(string id)
    {
        if (!_tags.ContainsKey(id))
            return null;

        List<string> artists = new();
        List<string> characters = new();
        List<string> sources = new();

        var post = _tags[id];
        _tags.Remove(id);

        foreach (string s in post.Item2.Tags)
        {
            try
            {
                switch ((await post.Item1.GetTagAsync(s)).Type)
                {
                    case BooruSharp.Search.Tag.TagType.Artist:
                        if (artists.Count == 10)
                            artists.Add("...");
                        else if (artists.Count < 10)
                            artists.Add(s);
                        break;

                    case BooruSharp.Search.Tag.TagType.Character:
                        if (characters.Count == 10)
                            characters.Add("...");
                        else if (characters.Count < 10)
                            characters.Add(s);
                        break;

                    case BooruSharp.Search.Tag.TagType.Copyright:
                        if (sources.Count == 10)
                            sources.Add("...");
                        else if (sources.Count < 10)
                            sources.Add(s);
                        break;
                }
            }
            catch (InvalidTags)
            { } // Just in case
        }

        return new TagsSearch(artists, characters, sources, post.Item2, post.Item1);
    }
}
