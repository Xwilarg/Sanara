using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Sanara.CustomClass;
using Sanara.Diaporama;
using Sanara.Exception;
using Sanara.Module;
using Sanara.Module.Administration;
using Sentry;
using System.Diagnostics;
using System.Globalization;

namespace Sanara
{
    public sealed class Program
    {
        private readonly CommandService _commands = new();

        private bool _didStart = false; // Keep track if the bot already started (mean it called the "Connected" callback)
        private Dictionary<string, Func<SocketSlashCommand, Task>> _commandsAssociations = new();

        public static async Task Main()
        {
            try
            {
                await new Program().StartAsync();
            }
            catch (FileNotFoundException) // This probably means a dll is missing
            {
                throw;
            }
            catch (System.Exception e)
            {
                if (!Debugger.IsAttached)
                {
                    if (!Directory.Exists("Logs"))
                        Directory.CreateDirectory("Logs");
                    File.WriteAllText("Logs/Crash-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ff") + ".txt", e.ToString());
                }
                else // If an exception occur, the program exit and is relaunched
                    throw;
            }
        }

        public async Task StartAsync()
        {
            await Log.LogAsync(new LogMessage(LogSeverity.Info, "Setup", "Initialising bot"));

            // Create saves directories
            if (!Directory.Exists("Saves")) Directory.CreateDirectory("Saves");
            if (!Directory.Exists("Saves/Download")) Directory.CreateDirectory("Saves/Download");
            if (!Directory.Exists("Saves/Game")) Directory.CreateDirectory("Saves/Game");

            // Setting Logs callback
            StaticObjects.Client.Log += Log.LogAsync;

            // Load credentials
            if (!File.Exists("Keys/Credentials.json"))
                throw new FileNotFoundException("Missing Credentials file");
            var _credentials = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText("Keys/Credentials.json"));

            // Set culture to invarriant (so we don't use , instead of . for decimal separator)
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // Initialize services
            await StaticObjects.InitializeAsync(_credentials);

            // If the bot takes way too much time to start, we stop the program
            // We do that after the StaticObjects initialization because the first time we load game cache, it can takes plenty of time
            _ = Task.Run(async () =>
            {
                await Task.Delay(Constants.PROGRAM_TIMEOUT);
                if (!_didStart)
                    Environment.Exit(1);
            });

            // Discord callbacks
            StaticObjects.Client.MessageReceived += HandleCommandAsync;
            StaticObjects.Client.Connected += ConnectedAsync;
            StaticObjects.Client.ReactionAdded += ReactionManager.ReactionAddedAsync;
            StaticObjects.Client.ReactionAdded += Module.Tool.LanguageModule.ReactionAddedAsync;
            StaticObjects.Client.GuildAvailable += GuildJoined;
            StaticObjects.Client.JoinedGuild += GuildJoined;
            StaticObjects.Client.JoinedGuild += ChangeGuildCountAsync;
            StaticObjects.Client.LeftGuild += ChangeGuildCountAsync;
            StaticObjects.Client.Disconnected += Disconnected;
            StaticObjects.Client.Ready += Ready;
            StaticObjects.Client.SlashCommandExecuted += SlashCommandExecuted;
            StaticObjects.Client.ButtonExecuted += ButtonExecuted;

            // Add readers
            _commands.AddTypeReader(typeof(IMessage), new TypeReader.IMessageReader());
            _commands.AddTypeReader(typeof(ImageLink), new TypeReader.ImageLinkReader());

            // Discord modules
            /*
            await _commands.AddModuleAsync<Module.Administration.InformationModule>(null);
            await _commands.AddModuleAsync<Module.Administration.SettingModule>(null);
            await _commands.AddModuleAsync<Module.Entertainment.FunModule>(null);
            await _commands.AddModuleAsync<Module.Entertainment.GameInfoModule>(null);
            await _commands.AddModuleAsync<Module.Entertainment.JapaneseModule>(null);
            await _commands.AddModuleAsync<Module.Entertainment.MediaModule>(null);
            await _commands.AddModuleAsync<Module.Game.GameModule>(null);
#if NSFW_BUILD
            await _commands.AddModuleAsync<Module.Entertainment.FunNsfwModule>(null);
            await _commands.AddModuleAsync<Module.Nsfw.BooruModule>(null);
            await _commands.AddModuleAsync<Module.Nsfw.DoujinModule>(null);
            await _commands.AddModuleAsync<Module.Nsfw.CosplayModule>(null);
            await _commands.AddModuleAsync<Module.Nsfw.VideoModule>(null);
            await _commands.AddModuleAsync<Module.Tool.LanguageNsfwModule>(null);
#endif
            await _commands.AddModuleAsync<Module.Nsfw.BooruSfwModule>(null);
            await _commands.AddModuleAsync<Module.Tool.CommunicationModule>(null);
            await _commands.AddModuleAsync<Module.Tool.LanguageModule>(null);
            await _commands.AddModuleAsync<Module.Tool.ScienceModule>(null);
            */
            await _commands.AddModuleAsync<DeprecationNotice>(null);

            await StaticObjects.Client.LoginAsync(TokenType.Bot, _credentials.BotToken);
            await StaticObjects.Client.StartAsync();

            // We keep the bot online
            await Task.Delay(-1);
        }

        private async Task ButtonExecuted(SocketMessageComponent ctx)
        {
            if (StaticObjects.Errors.ContainsKey(ctx.Data.CustomId))
            {
                var e = StaticObjects.Errors[ctx.Data.CustomId];
                await ctx.RespondAsync(embed: new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = e.GetType().ToString(),
                    Description = e.Message
                }.Build());
            }
        }

        private async Task SlashCommandExecuted(SocketSlashCommand ctx)
        {
            if (!_commandsAssociations.ContainsKey(ctx.CommandName.ToUpperInvariant()))
            {
                throw new NotImplementedException($"Unknown command {ctx.CommandName}");
            }
            try
            {
                await _commandsAssociations[ctx.CommandName.ToUpperInvariant()](ctx);
                StaticObjects.LastMessage = DateTime.UtcNow;

                if (StaticObjects.Website != null)
                {
                    await StaticObjects.Website.AddNewMessageAsync();
                    await StaticObjects.Website.AddNewCommandAsync(ctx.CommandName.ToUpperInvariant());
                }
            }
            catch (System.Exception e)
            {
                if (e is CommandFailed)
                {
                    ctx.RespondAsync(e.Message, ephemeral: true);
                }
                await Log.LogErrorAsync(e, ctx);
            }
        }

        private async Task Ready()
        {
            // Commands already loaded
            if (_commandsAssociations.Count != 0)
            {
                return;
            }

            var isDebug = StaticObjects.DebugGuildId != 0 && Debugger.IsAttached;
            if (isDebug)
            {
                await StaticObjects.Client.GetGuild(StaticObjects.DebugGuildId).DeleteApplicationCommandsAsync();
            }
            List<ISubmodule> _submodules = new();
            _submodules.Add(new InformationModule());
            foreach (var s in _submodules)
            {
                foreach (var c in s.GetCommands())
                {
                    if (isDebug)
                    {
                        await StaticObjects.Client.GetGuild(StaticObjects.DebugGuildId).CreateApplicationCommandAsync(c.SlashCommand);
                    }
                    else
                    {
                        await StaticObjects.Client.CreateGlobalApplicationCommandAsync(c.SlashCommand);
                    }
                    _commandsAssociations.Add(c.SlashCommand.Name.Value.ToUpperInvariant(), c.Callback);
                }
            }

#if NSFW_BUILD
            await StaticObjects.Client.SetActivityAsync(new Discord.Game("https://sanara.zirk.eu", ActivityType.Watching));
#endif
            // The bot is now really ready to interact with people
            StaticObjects.Started = DateTime.UtcNow;
        }

        private Task Disconnected(System.Exception e)
        {
            if (!File.Exists("Saves/Logs"))
                Directory.CreateDirectory("Saves/Logs");
            File.WriteAllText("Saves/Logs/" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log", "Bot disconnected. Exception:\n" + e.ToString());
            return Task.CompletedTask;
        }

        private async Task ChangeGuildCountAsync(SocketGuild _)
        {
            await StaticObjects.UpdateTopGgAsync();
        }

        private async Task GuildJoined(SocketGuild guild)
        {
            await StaticObjects.Db.InitGuildAsync(guild);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg.Author.IsBot || arg is not SocketUserMessage msg) return; // The message received isn't a message we can deal with

            ITextChannel? textChan = msg.Channel as ITextChannel;

            // Deprecation warning
            int pos = 0;
            var prefix = textChan == null ? "s." : StaticObjects.Db.GetGuild(textChan.GuildId).Prefix;
            if (msg.HasMentionPrefix(StaticObjects.Client.CurrentUser, ref pos) || msg.HasStringPrefix(prefix, ref pos))
            {
                if (textChan != null && !StaticObjects.Help.IsModuleAvailable(textChan.GuildId, msg.Content.Substring(pos).ToLower()))
                {
                    return ;
                }
                var context = new SocketCommandContext(StaticObjects.Client, msg);
                await _commands.ExecuteAsync(context, pos, null);
            }

            // TODO: What about commands?
            if (!msg.Content.StartsWith("//") && !msg.Content.StartsWith("#")) // "Comment" message to ignore game parsing
            {
                var game = StaticObjects.Games.Find(x => x.IsMyGame(msg.Channel.Id));
                game?.AddAnswer(msg);
            }
        }

        private async Task ConnectedAsync()
        {
            _didStart = true;
            StaticObjects.ClientId = StaticObjects.Client.CurrentUser.Id;
            await StaticObjects.UpdateTopGgAsync();

            StaticObjects.Website?.KeepSendStats();
        }
    }
}
