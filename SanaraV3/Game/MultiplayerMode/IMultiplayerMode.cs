using Discord;
using System.Collections.Generic;

namespace SanaraV3.Game.MultiplayerMode
{
    public interface IMultiplayerMode
    {
        public void Init(List<IUser> users);

        public string PrePost(); // Called before a post is sent

        public void PreAnswerCheck(IUser user); // Called before checking an user answer

        public void AnswerIsCorrect(IUser user); // Called when an user find a good answer
    }
}
