using Discord;
using Sanara.Compatibility;

namespace Sanara.Game.MultiplayerMode
{
    public class ScoreUser
    {
        public ScoreUser(CommonUser user)
        {
            User = user;
        }

        public void IncreaseScore()
        {
            Score++;
        }
        public int Score { private set; get; }
        public CommonUser User { private set; get; }
    }
}
