using BooruSharp.Booru;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Compatibility;
using Sanara.Exception;
using Sanara.Game.MultiplayerMode;
using Sanara.Game.PostMode;
using Sanara.Game.Preload;
using Sanara.Module.Command;
using System.Web;

namespace Sanara.Game.Impl
{
    /// <summary>
    /// Basic quizz game
    /// </summary>
    public class FillAllBooru : AGame
    {
        public FillAllBooru(IServiceProvider provider, CommonMessageChannel textChan, CommonUser user, IPreload preload, GameSettings settings) : base(provider, textChan, user, preload, new UrlMode(), new SpeedFillAllBooruMode(), settings)
        { }

        protected override string[] GetPostInternal()
        {
            _booru = new()
            {
                HttpClient = _provider.GetRequiredService<HttpClient>()
            };
            var post = _booru.GetRandomPostAsync().GetAwaiter().GetResult();
            var tags = post.Tags.Select(x => HttpUtility.UrlDecode(x)).ToList();
            var gm = _provider.GetRequiredService<GameManager>();
            tags.RemoveAll(x => // TODO: Put that in some cache
            {
                if (gm.GelbooruTags.ContainsKey(x))
                    return gm.GelbooruTags[x] == BooruSharp.Search.Tag.TagType.Metadata;
                var tag = _booru.GetTagAsync(x).GetAwaiter().GetResult();
                gm.GelbooruTags.Add(x, tag.Type);
                return tag.Type == BooruSharp.Search.Tag.TagType.Metadata;
            });
            _allTags = tags.ToArray();
            _foundTags = new List<string>();
            _nbNeed = _lobby.MultiplayerType != MultiplayerType.VERSUS ? (int)Math.Floor(_allTags.Length * 75.0 / 100) : _allTags.Length;
            return new[] { post.FileUrl.AbsoluteUri };
        }

        protected override Task CheckAnswerInternalAsync(IContext answer)
        {
            string userAnswer = answer.GetArgument<string>("answer");
            var foundTag = _allTags.Where(x => Utils.EasyCompare(x, userAnswer)).FirstOrDefault();
            if (foundTag == null)
                throw new InvalidGameAnswer("");
            if (_foundTags.Contains(foundTag))
                throw new InvalidGameAnswer("This tag was already found.");
            _foundTags.Add(foundTag);

            if (_lobby.MultiplayerType == MultiplayerType.VERSUS)
            {
                _versusMode.AnswerIsCorrect(answer.User);
            }

            if (_nbNeed != _foundTags.Count)
                throw new InvalidGameAnswer($"{(_lobby.MultiplayerType != MultiplayerType.VERSUS ? "You" : answer.User.Username)} found a tag!\n{_nbNeed - _foundTags.Count} remaining.");
            return Task.CompletedTask;
        }

        protected override string GetAnswer()
        {
            return "Here are the tags you didn't find: " + string.Join(", ", _allTags.Where(x => !_foundTags.Contains(x)).Select(x => x.Replace('_', ' ')));
        }

        protected override int GetGameTime()
            => 60;

        protected override string GetSuccessMessage(CommonUser _)
            => _lobby.MultiplayerType != MultiplayerType.VERSUS ? "You found at least 75% of the tags on the image!" : null;

        protected override string GetHelp()
            => _lobby.MultiplayerType != MultiplayerType.VERSUS ? "You have " + _nbNeed + " tags out of " + _allTags.Length + " to find." : "There are " + _allTags.Length + " tags on the image.";

        private string[] _allTags;
        private List<string> _foundTags;
        private int _nbNeed; // Number of tags you need to find (75% of total count)

        private Gelbooru _booru;
    }
}
