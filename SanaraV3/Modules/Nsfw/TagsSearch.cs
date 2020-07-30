using BooruSharp.Booru;
using System.Collections.Generic;

namespace SanaraV3.Modules.Nsfw
{
    public struct TagsSearch
    {
        public List<string> Artists;
        public List<string> Characters;
        public List<string> Sources;
        public BooruSharp.Search.Post.SearchResult Post;
        public ABooru Booru;
    }
}
