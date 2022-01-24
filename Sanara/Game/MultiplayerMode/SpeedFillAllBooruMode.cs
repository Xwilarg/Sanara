using Discord;
using Sanara.Exception;
using System.Text;

namespace Sanara.Game.MultiplayerMode
{
    public class SpeedFillAllBooruMode : IMultiplayerMode
    {
        public void Init(List<IUser> users)
        {
            _scores = new();
            foreach (var u in users)
                _scores.Add(u.Id, new(u));
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
            => true;

        public string GetRules()
            => "You must find as much tags as possible, the player with the most points win.";

        public string GetOutroLoose()
        {
            StringBuilder str = new();
            str.AppendLine("Final score:");
            foreach (var s in _scores.OrderByDescending(x => x.Value.Score))
                str.AppendLine(Format.Sanitize(s.Value.User.Username) + ": " + s.Value.Score);
            return str.ToString();
        }

        private Dictionary<ulong, ScoreUser> _scores;
        protected bool _isDone;
    }
}
