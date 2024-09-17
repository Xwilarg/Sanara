using Discord;
using Sanara.Exception;
using System.Text;

namespace Sanara.Game.MultiplayerMode
{
    public class SpeedMode : IMultiplayerMode
    {
        public void Init(Random _, List<IUser> users)
        {
            _scores = new();
            foreach (var u in users)
                _scores.Add(u.Id, new(u));
            _remainingGames = 11;
        }

        public string PrePost()
        {
            _remainingGames--;
            if (_remainingGames == 0)
                throw new GameLost("Game ended");
            return _remainingGames + " remaining";
        }

        public void PreAnswerCheck(IUser user)
        { }

        public void AnswerIsCorrect(IUser user)
        {
            _scores[user.Id].IncreaseScore();
        }

        public bool Loose()
        {
            return false;
        }

        public string GetWinner()
        {
            var best = _scores.Values.Max(x => x.Score);
            var names = _scores.Where(x => x.Value.Score == best);
            return string.Join(", ", names.Select(x => x.Value.User.Mention));
        }

        public bool CanLooseAuto()
            => false;

        public string GetRules()
            => "You must be the first to answer, the player with the most points win.";

        public string GetOutroLoose()
        {
            StringBuilder str = new();
            str.AppendLine("Final score:");
            foreach (var s in _scores.OrderByDescending(x => x.Value.Score))
                str.AppendLine(Format.Sanitize(s.Value.User.Username) + ": " + s.Value.Score);
            return str.ToString();
        }

        private Dictionary<ulong, ScoreUser> _scores;
        protected int _remainingGames;
    }
}
