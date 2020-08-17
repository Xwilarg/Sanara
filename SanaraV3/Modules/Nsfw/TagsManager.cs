﻿using BooruSharp.Booru;
using BooruSharp.Search;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Nsfw
{
    public class TagsManager
    {
        /// <summary>
        /// Associate an id with a booru and a post
        /// </summary>
        private Dictionary<int, Tuple<ABooru, BooruSharp.Search.Post.SearchResult>> _tags = new Dictionary<int, Tuple<ABooru, BooruSharp.Search.Post.SearchResult>>();

        public void AddTag(int id, ABooru booru, BooruSharp.Search.Post.SearchResult post)
        {
            if (!_tags.ContainsKey(id))
                _tags.Add(id, new Tuple<ABooru, BooruSharp.Search.Post.SearchResult>(booru, post));
        }

        public async Task<TagsSearch?> GetTagAsync(int id)
        {
            if (!_tags.ContainsKey(id))
                return null;

            List<string> artists = new List<string>();
            List<string> characters = new List<string>();
            List<string> sources = new List<string>();

            var post = _tags[id];

            foreach (string s in post.Item2.tags)
            {
                try
                {
                    switch ((await post.Item1.GetTagAsync(s)).type)
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

            return new TagsSearch
            {
                Artists = artists,
                Characters = characters,
                Sources = sources,
                Post = post.Item2,
                Booru = post.Item1
            };
        }
    }
}
