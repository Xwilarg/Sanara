﻿using BooruSharp.Booru;
using BooruSharp.Search.Post;

namespace Sanara.Module.Utility;

public struct TagsSearch
{
    public TagsSearch(List<string> artists, List<string> characters, List<string> sources, SearchResult post, ABooru booru)
    {
        Artists = artists;
        Characters = characters;
        Sources = sources;
        Post = post;
        Booru = booru;
    }

    public List<string> Artists { get; }
    public List<string> Characters { get; }
    public List<string> Sources { get; }
    public SearchResult Post { get; }
    public ABooru Booru { get; }
}
