using Discord;

namespace Sanara.Game.MultiplayerMode
{
    public class ScoreUser
    {
        public ScoreUser(IUser user)
        {
            User = user;
        }

        public void IncreaseScore()
        {
            Score++;
        }
        public int Score { private set; get; }
        public IUser User { private set; get; }
    }
}
