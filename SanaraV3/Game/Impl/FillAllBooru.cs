﻿using Discord;
using Discord.WebSocket;
using DiscordUtils;
using SanaraV3.Exception;
using SanaraV3.Game.MultiplayerMode;
using SanaraV3.Game.Preload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SanaraV3.Game.Impl
{
    /// <summary>
    /// Basic quizz game
    /// </summary>
    public class FillAllBooru : AGame
    {
        public FillAllBooru(IMessageChannel textChan, IUser user, IPreload preload, GameSettings settings) : base(textChan, user, preload, StaticObjects.ModeUrl, new SpeedFillAllBooruMode(), settings)
        { }

        protected override string[] GetPostInternal()
        {
            var post = StaticObjects.Gelbooru.GetRandomPostAsync().GetAwaiter().GetResult();
            var tags = post.tags.Select(x => HttpUtility.UrlDecode(x)).ToList();
            tags.RemoveAll(x => // TODO: Put that in some cache
            {
                if (StaticObjects.GelbooruTags.ContainsKey(x))
                    return StaticObjects.GelbooruTags[x] == BooruSharp.Search.Tag.TagType.Metadata;
                var tag = StaticObjects.Gelbooru.GetTagAsync(x).GetAwaiter().GetResult();
                StaticObjects.GelbooruTags.Add(x, tag.type);
                return tag.type == BooruSharp.Search.Tag.TagType.Metadata;
            });
            _allTags = tags.ToArray();
            _foundTags = new List<string>();
            _nbNeed = _lobby == null ? (int)Math.Floor(_allTags.Length * 75.0 / 100) : _allTags.Length;
            return new[] { post.fileUrl.AbsoluteUri };
        }

        protected override Task CheckAnswerInternalAsync(SocketUserMessage answer)
        {
            string userAnswer = Utils.CleanWord(answer.Content);
            var foundTag = _allTags.Where(x => Utils.CleanWord(x) == userAnswer).FirstOrDefault();
            if (foundTag == null)
                throw new InvalidGameAnswer("");
            if (_foundTags.Contains(foundTag))
                throw new InvalidGameAnswer("This tag was already found.");
            _foundTags.Add(foundTag);

            if (_lobby != null)
            {
                _multiplayerMode.AnswerIsCorrect(answer.Author);
            }

            if (_nbNeed != _foundTags.Count)
                throw new InvalidGameAnswer($"{(_lobby == null ? "You" : answer.Author.Username)} found a tag!\n{_nbNeed - _foundTags.Count} remaining.");
            return Task.CompletedTask;
        }

        protected override string GetAnswer()
        {
            return "Here are the tags you didn't find: " + string.Join(", ", _allTags.Where(x => !_foundTags.Contains(x)).Select(x => x.Replace('_', ' ')));
        }

        protected override int GetGameTime()
            => 60;

        protected override string GetSuccessMessage(IUser _)
            => _lobby == null ? "You found at least 75% of the tags on the image!" : null;

        protected override string GetHelp()
            => _lobby == null ? "You have " + _nbNeed + " tags out of " + _allTags.Length + " to find." : "There are " + _allTags.Length + " tags on the image.";

        private string[] _allTags;
        private List<string> _foundTags;
        private int _nbNeed; // Number of tags you need to find (75% of total count)
    }
}
