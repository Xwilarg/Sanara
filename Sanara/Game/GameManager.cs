using Discord;
using Sanara.Game.Preload;
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

                // Purge expired lobbies
                foreach (var key in new List<string>(_replayLobby.Where(x => x.Value.HasExpired).Select(x => x.Key)))
                {
                    _replayLobby.Remove(key);
                }

                Thread.Sleep(200);
            }
        }

        public AGame? GetGame(IChannel chan)
            => _games.FirstOrDefault(x => x.IsMyGame(chan.Id));

        public async Task CreateGameAsync(IChannel channel, AGame game)
        {
            var chanId = channel.Id.ToString();
            if (_replayLobby.ContainsKey(chanId))
            {
                var lobby = _replayLobby[chanId];
                _replayLobby.Remove(chanId);
                await lobby.Message.DeleteAsync();
            }
            _pendingGames.Add(chanId, game);
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

        public ReplayLobby? GetReplayLobby(IChannel chan)
        {
            if (_replayLobby.ContainsKey(chan.Id.ToString()))
            {
                return _replayLobby[chan.Id.ToString()];
            }
            return null;
        }

        /// <returns>Updated embed or null if user not in lobby</returns>
        public Embed? ToggleReadyLobby(ReplayLobby rLobby, IUser user)
        {
            var result = rLobby.ToggleReady(user);
            return result ? rLobby.GetEmbed() : null;
        }

        public async Task<bool> CheckRestartLobbyFullAsync(ICommandContext ctx)
        {
            var rLobby = _replayLobby[ctx.Channel.Id.ToString()];
            if (rLobby.IsAllReady)
            {
                var lobby = rLobby.CreateLobby();
                DeleteReadyLobby(ctx.Channel);
                var game = rLobby.Preload.CreateGame((IMessageChannel)ctx.Channel, rLobby.LastHost, new GameSettings(lobby, false));
                _games.Add(game);
                await game.StartAsync(ctx);
                return true;
            }
            return false;
        }

        public void DeleteReadyLobby(IChannel chan)
        {
            var chanId = chan.Id.ToString();
            _replayLobby.Remove(chanId);
        }

        public async Task StartGameAsync(ICommandContext ctx)
        {
            var chanId = ctx.Channel.Id.ToString();
            await _pendingGames[chanId].StartAsync(ctx);
            _games.Add(_pendingGames[chanId]);
            _pendingGames.Remove(chanId);
        }

        public ReplayLobby AddReplayLobby(IChannel chan, IPreload preload, Lobby lobby)
        {
            var rLobby = new ReplayLobby(preload, lobby.Host, lobby.GetUsers(), lobby.GetMultiplayerMode());
            _replayLobby.Add(chan.Id.ToString(), rLobby);
            return rLobby;
        }

        public bool IsChannelBusy(IChannel chan)
            => GetGame(chan) != null || _pendingGames.ContainsKey(chan.Id.ToString());

        private readonly Thread thread;

        private Dictionary<string, AGame> _pendingGames = new();
        private List<AGame> _games { get; } = new();
        private Dictionary<string, ReplayLobby> _replayLobby = new();
    }
}
