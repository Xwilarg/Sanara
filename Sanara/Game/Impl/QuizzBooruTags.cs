using Discord;
using Sanara.Compatibility;
using Sanara.Game.Preload;
using System.Web;

namespace Sanara.Game.Impl
{
    public sealed class QuizzBooruTags : QuizzBooru
    {
        public QuizzBooruTags(IServiceProvider provider, CommonMessageChannel textChan, CommonUser user, IPreload preload, GameSettings settings) : base(provider, textChan, user, preload, settings)
        { }

        protected override string[] GetPostInternal()
        {
            base.GetPostInternal();

            var results = _booru.GetRandomPostsAsync(3, _current.ImageUrl).GetAwaiter().GetResult();
            if (results.Length < 3)
                throw new IndexOutOfRangeException("No result with tags " + _current.ImageUrl);

            _current = new Preload.Result.QuizzPreloadResult(_current.ImageUrl, _current.Answers.Select(x => HttpUtility.UrlDecode(x)).ToArray());

            return results.Select(x => x.FileUrl.AbsoluteUri).ToArray();
        }
    }
}
