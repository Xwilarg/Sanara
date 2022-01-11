using Discord;

namespace Sanara.Game
{
    public sealed class GameManager
    {
        public GameManager()
        {
            thread = new(new ThreadStart(Loop));
        }

        public void Init()
        {
            thread.Start();
        }

        private void Loop()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                foreach (var game in _games)
                {
                    _ = Task.Run(() => { game.CheckAnswersAsync().GetAwaiter().GetResult(); });
                    game.CheckTimerAsync().GetAwaiter().GetResult();
                }
                foreach (var g in _games.Where(x => x.AsLost()))
                    g.Dispose();
                _games.RemoveAll(x => x.AsLost()); // Remove all the game that were lost
                Thread.Sleep(200);
            }
        }

        public AGame? GetGame(IChannel chan)
            => _games.FirstOrDefault(x => x.IsMyGame(chan.Id));

        public string CreateGame(AGame game)
        {
            var id = Guid.NewGuid().ToString();
            _pendingGames.Add(id, game);
            return id;
        }

        public bool DoesLobbyExists(string id)
        {
            return _pendingGames.ContainsKey(id);
        }

        public async Task StartGameAsync(string id)
        {
            await _pendingGames[id].StartAsync();
            _pendingGames.Remove(id);
        }

        private readonly Thread thread;

        private Dictionary<string, AGame> _pendingGames = new();
        private List<AGame> _games { get; } = new();
    }
}
