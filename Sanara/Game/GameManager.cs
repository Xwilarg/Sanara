using Discord;
using Sanara.Game.Preload;
using Sanara.Game.Preload.Impl;
using Sanara.Module.Command;

namespace Sanara.Game
{
    public sealed class GameManager
    {
        public GameManager()
        {
            thread = new(new ThreadStart(Loop));
            List<string> allNames = [];
            foreach (var p in Preloads)
            {
#if !NSFW_BUILD
                if (!p.IsSafe())
                {
                    continue;
                }
#endif
                allNames.Add(p.Name);// TODO: + (option == null ? "" : "-" + option));
            }
            AllGameNames = [.. allNames];
        }

        public void Init(IServiceProvider provider)
        {
            foreach (var p in Preloads)
            {
#if !NSFW_BUILD
                if (!p.IsSafe())
                {
                    Log.LogAsync(new LogMessage(LogSeverity.Verbose, "Static Preload", p.Name + " was skipped")).GetAwaiter().GetResult();
                    continue;
                }
#endif
                _ = Task.Run(async () =>
                {
                    try
                    {
                        p.Init(provider);
                        await Log.LogAsync(new LogMessage(LogSeverity.Verbose, "Static Preload", p.Name + " successfully loaded"));
                    }
                    catch (System.Exception e)
                    {
                        await Log.LogErrorAsync(e, null);
                        await Log.LogAsync(new LogMessage(LogSeverity.Verbose, "Static Preload", p.Name + " failed to load"));
                    }
                });
            }

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
                for (int i = _replayLobby.Count - 1; i >= 0; i--)
                {
                    var key = _replayLobby.Keys.ElementAt(i);
                    var lobby = _replayLobby[key];
                    if (lobby.HasExpired)
                    {
                        _replayLobby.Remove(key);
                    }
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

        public async Task<bool> CheckRestartLobbyFullAsync(IContext ctx)
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

        public async Task StartGameAsync(IContext ctx)
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

        public IPreload[] Preloads { set; get; } =
            [
                /*new ArknightsAudioPreload(),
                new KancolleAudioPreload(),

                new ShiritoriHardPreload(),*/

                new ShiritoriPreload(),
            new KancollePreload(),
            new ArknightsPreload(),
            new GirlsFrontlinePreload(),
            new AzurLanePreload(),
            new FateGOPreload(),
            new PokemonPreload(),
            new NikkePreload(),
            new AnimePreload(),
#if NSFW_BUILD
            new BooruQuizzPreload(),
            new BooruFillPreload()
#endif
            ];
        public string[] AllGameNames { set; get; }

        public Dictionary<string, BooruSharp.Search.Tag.TagType> QuizzTagsCache { get; } = [];
        public Dictionary<string, BooruSharp.Search.Tag.TagType> GelbooruTags { get; } = [];
    }
}
