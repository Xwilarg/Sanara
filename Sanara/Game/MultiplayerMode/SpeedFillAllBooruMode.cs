using Discord;
using Sanara.Exception;
using System.Text;

namespace Sanara.Game.MultiplayerMode
{
    public class SpeedFillAllBooruMode : IMultiplayerMode
    {
        public void Init(List<IUser> users)
        {
            _scores = new Dictionary<IUser, int>();
            foreach (var u in users)
                _scores.Add(u, 0);
            _isDone = false;
        }

        public string PrePost()
        {
            if (!_isDone) _isDone = true;
            else
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

        public string GetRules()
            => "You must find as much tags as possible, the player with the most points win.";

        public string GetOutroLoose()
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("Final score:");
            foreach (var s in _scores.OrderByDescending(x => x.Value))
                str.AppendLine(Format.Sanitize(s.Key.Username) + ": " + s.Value);
            return str.ToString();
        }

        private Dictionary<IUser, int> _scores;
        protected bool _isDone;
    }
}
