﻿using Discord;
using SanaraV3.Game.Preload;
using System;
using System.Linq;
using System.Web;

namespace SanaraV3.Game.Impl
{
    public sealed class QuizzBooruTags : QuizzBooru
    {
        public QuizzBooruTags(IMessageChannel textChan, IUser user, IPreload preload, GameSettings settings) : base(textChan, user, preload, settings)
        { }

        protected override string[] GetPostInternal()
        {
            base.GetPostInternal();

            var results = _booru.GetRandomPostsAsync(3, _current.ImageUrl).GetAwaiter().GetResult();
            if (results.Length < 3)
                throw new IndexOutOfRangeException("No result with tags " + _current.ImageUrl);

            _current = new Preload.Result.QuizzPreloadResult(_current.ImageUrl, _current.Answers.Select(x => HttpUtility.UrlDecode(x)).ToArray());

            return results.Select(x => x.fileUrl.AbsoluteUri).ToArray();
        }
    }
}
