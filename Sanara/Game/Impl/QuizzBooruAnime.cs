using Discord;
using Sanara.Game.Preload;
using Sanara.Game.Preload.Result;

namespace Sanara.Game.Impl
{
    public sealed class QuizzBooruAnime : QuizzBooru
    {
        public QuizzBooruAnime(IMessageChannel textChan, IUser user, IPreload preload, GameSettings settings) : base(textChan, user, preload, settings)
        { }

        protected override string[] GetPostInternal()
        {
            base.GetPostInternal();

            var results = _booru.GetRandomPostsAsync(10, _current.ImageUrl).GetAwaiter().GetResult();
            results = results.Where(x => _allowedFormats.Contains(Path.GetExtension(x.FileUrl.AbsoluteUri)) && !x.Tags.Contains("western") && !x.Tags.Contains("web") && x.Rating == BooruSharp.Search.Post.Rating.Safe).ToArray();
            if (results.Length == 0)
                throw new IndexOutOfRangeException("No result with correct format found");

            var result = results[StaticObjects.Random.Next(results.Length)];

            List<string> answers = new List<string>();

            foreach (var t in result.Tags)
            {
                var name = _booru.ToString() + "_" + t;
                if (StaticObjects.QuizzTagsCache.ContainsKey(name))
                {
                    if (StaticObjects.QuizzTagsCache[name] == BooruSharp.Search.Tag.TagType.Copyright)
                        answers.Add(t);
                }
                else
                {
                    var info = _booru.GetTagAsync(t).GetAwaiter().GetResult();
                    lock (StaticObjects.QuizzTagsCache)
                    {
                        StaticObjects.QuizzTagsCache.Add(name, info.Type);
                    }
                    if (info.Type == BooruSharp.Search.Tag.TagType.Copyright)
                        answers.Add(t);
                }
            }
            if (answers.Count == 0)
                throw new IndexOutOfRangeException("No answer found for " + result.PostUrl.AbsoluteUri);
            _current = new QuizzPreloadResult(_current.ImageUrl, answers.ToArray());

            return new[] { result.FileUrl.AbsoluteUri };
        }
    }
}
