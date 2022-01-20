﻿using Discord;
using Sanara.Module.Command;

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

        public bool RemoveLobby(string id)
        {
            return _pendingGames.Remove(id);
        }

        public Lobby? GetLobby(string id)
        {
            if (!_pendingGames.ContainsKey(id))
            {
                return null;
            }
            return _pendingGames[id].GetLobby();
        }

        public async Task StartGameAsync(ICommandContext ctx, string id)
        {
            await _pendingGames[id].StartAsync(ctx);
            _games.Add(_pendingGames[id]);
            _pendingGames.Remove(id);
        }

        private readonly Thread thread;

        private Dictionary<string, AGame> _pendingGames = new();
        private List<AGame> _games { get; } = new();
    }
}
