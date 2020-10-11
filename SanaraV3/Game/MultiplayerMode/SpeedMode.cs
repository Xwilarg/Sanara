using Discord;
using System.Collections.Generic;

namespace SanaraV3.Game.MultiplayerMode
{
    public class SpeedMode : IMultiplayerMode
    {
        public void Init(List<IUser> users)
        { }

        public string PrePost()
            => null;

        public void PreAnswerCheck(IUser user)
        { }
    }
}
