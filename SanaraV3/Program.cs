using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordUtils;
using Newtonsoft.Json;
using SanaraV3.Diaporama;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace SanaraV3
{
    public sealed class Program
    {
        private readonly CommandService _commands = new CommandService();

        private bool _didStart = false; // Keep track if the bot already started (mean it called the "Connected" callback)

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
            catch (System.Exception) // If an exception occur, the program exit and is relaunched
            {
                if (Debugger.IsAttached)
                    throw;
            }
        }

        public async Task StartAsync()
        {
            // Setting Logs callback
            StaticObjects.Client.Log += Utils.Log;
            _commands.Log += LogErrorAsync;
            await Utils.Log(new LogMessage(LogSeverity.Info, "Setup", "Initialising bot"));

            // Load credentials
            if (!File.Exists("Keys/Credentials.json"))
                throw new FileNotFoundException("Missing Credentials file");
            var _credentials = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText("Keys/Credentials.json"));

            // Create saves directories
            if (!Directory.Exists("Saves")) Directory.CreateDirectory("Saves");
            if (!Directory.Exists("Saves/Radio")) Directory.CreateDirectory("Saves/Radio");
            if (!Directory.Exists("Saves/Download")) Directory.CreateDirectory("Saves/Download");

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
            StaticObjects.Client.GuildAvailable += GuildJoined;
            StaticObjects.Client.JoinedGuild += GuildJoined;

            // Add readers
            _commands.AddTypeReader(typeof(IMessage), new TypeReader.IMessageReader());

            // Discord modules
            await _commands.AddModuleAsync<Module.Administration.InformationModule>(null);
            await _commands.AddModuleAsync<Module.Administration.SettingModule>(null);
            await _commands.AddModuleAsync<Module.Entertainment.FunModule>(null);
            await _commands.AddModuleAsync<Module.Entertainment.JapaneseModule>(null);
            await _commands.AddModuleAsync<Module.Entertainment.MediaModule>(null);
            await _commands.AddModuleAsync<Module.Game.GameModule>(null);
            await _commands.AddModuleAsync<Module.Nsfw.BooruModule>(null);
            await _commands.AddModuleAsync<Module.Nsfw.DoujinshiModule>(null);
            await _commands.AddModuleAsync<Module.Nsfw.CosplayModule>(null);
            await _commands.AddModuleAsync<Module.Radio.RadioModule>(null);
            await _commands.AddModuleAsync<Module.Tool.CommunicationModule>(null);
            await _commands.AddModuleAsync<Module.Tool.LanguageModule>(null);
            await _commands.AddModuleAsync<Module.Tool.ScienceModule>(null);

            await StaticObjects.Client.LoginAsync(TokenType.Bot, _credentials.BotToken);
            await StaticObjects.Client.StartAsync();

            // We keep the bot online
            await Task.Delay(-1);
        }

        private async Task GuildJoined(SocketGuild guild)
        {
            await StaticObjects.Db.InitGuildAsync(guild);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg.Author.IsBot) // We ignore messages from bots
                return;
            var msg = arg as SocketUserMessage;
            if (msg == null) return; // The message received isn't a message we can deal with

            int pos = 0;
            ITextChannel textChan = msg.Channel as ITextChannel;
            if (msg.HasMentionPrefix(StaticObjects.Client.CurrentUser, ref pos) || (textChan != null && msg.HasStringPrefix(StaticObjects.Db.GetGuild(textChan.GuildId).Prefix, ref pos)))
            {
                var context = new SocketCommandContext(StaticObjects.Client, msg);
                var result = await _commands.ExecuteAsync(context, pos, null);
                if (!result.IsSuccess)
                {
                    var error = result.Error.Value;
                    Console.WriteLine(error.ToString()); // TODO: Debug
                    if (error == CommandError.UnmetPrecondition)
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                    else if (error == CommandError.BadArgCount || error == CommandError.ParseFailed)
                        await context.Channel.SendMessageAsync("This command have some invalid parameters."); // TODO: Display help
                }
            }
            else if (!msg.Content.StartsWith("//") && !msg.Content.StartsWith("#")) // "Comment" message to ignore game parsing // TODO: Need to check if it's not the bot prefix
            {
                var game = StaticObjects.Games.Find(x => x.IsMyGame(msg.Channel.Id));
                if (game != null) // If we are in a game
                {
                    game.AddAnswer(msg);
                }
            }
        }

        private async Task ConnectedAsync()
        {
            _didStart = true;
            StaticObjects.ClientId = StaticObjects.Client.CurrentUser.Id;
            await StaticObjects.Client.SetGameAsync("Sanara V3 coming soon!", null, ActivityType.CustomStatus);
        }

        public async Task LogErrorAsync(LogMessage msg)
        {
            CommandException ce = msg.Exception as CommandException;
            if (ce != null)
            {
                if (msg.Exception.InnerException is Exception.CommandFailed) // Exception thrown from modules when a command failed
                {
                    await ce.Context.Channel.SendMessageAsync(msg.Exception.InnerException.Message);
                }
                else // Unexpected exception
                {
                    await Utils.Log(msg);
                    await ce.Context.Channel.SendMessageAsync("", false, new EmbedBuilder
                    {
                        Color = Color.Red,
                        Title = msg.Exception.InnerException.GetType().ToString(),
                        Description = "An error occured while executing last command.\nHere are some details about it: " + msg.Exception.InnerException.Message
                    }.Build());
                }
            }
            else
                await Utils.Log(msg);
        }
    }
}
