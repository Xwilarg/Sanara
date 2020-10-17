using Discord;
using SanaraV3.Exception;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanaraV3.Game.MultiplayerMode
{
    public class SpeedMode : IMultiplayerMode
    {
        public void Init(List<IUser> users)
        {
            _scores = new Dictionary<IUser, int>();
            foreach (var u in users)
                _scores.Add(u, 0);
            _remainingGames = 11;
        }

        public string PrePost()
        {
            _remainingGames--;
            if (_remainingGames == 0)
                throw new GameLost("Game ended");
            return null;
        }

        public void PreAnswerCheck(IUser user)
        { }

        public void AnswerIsCorrect(IUser user)
        {
            _scores[user]++;
        }

        public bool Loose()
        {
            return false;
        }

        public string GetWinner()
        {
            var best = _scores.Values.Max();
            var names = _scores.Where(x => x.Value == best);
            return string.Join(", ", names.Select(x => x.Key.Mention));
        }

        public bool CanLooseAuto()
            => true;

        public string GetOutroLoose()
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("Final score:");
            foreach (var s in _scores.OrderByDescending(x => x.Value))
                str.AppendLine(Format.Sanitize(s.Key.Username) + ": " + s.Value);
            return str.ToString();
        }

        private Dictionary<IUser, int> _scores;
        private int _remainingGames;
    }
}
