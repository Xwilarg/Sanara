using Discord;
using System.Collections.Generic;

namespace SanaraV3.Game.MultiplayerMode
{
    public interface IMultiplayerMode
    {
        public void Init(List<IUser> users);

        public string PrePost(); // Called before a post is sent

        public void PreAnswerCheck(IUser user);
    }
}
