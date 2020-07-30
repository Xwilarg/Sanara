using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordUtils;
using Newtonsoft.Json;
using SanaraV3.Exceptions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SanaraV3
{
    public sealed class Program
    {
        private readonly DiscordSocketClient _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
        });
        private readonly CommandService _commands = new CommandService();

        private bool _didStart = false; // Keep track if the bot already started (mean it called the "Connected" callback)

        private Credentials _credentials;

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
            catch (Exception) // If an exception occur, the program exit and is relaunched
            {
                if (Debugger.IsAttached)
                    throw;
            }
        }

        public async Task StartAsync()
        {
            // Setting Logs callback
            _client.Log += Utils.Log;
            _commands.Log += LogError;
            await Utils.Log(new LogMessage(LogSeverity.Info, "Setup", "Initialising bot"));

            // If the bot takes way too much time to start, we stop the program
            _ = Task.Run(async () =>
            {
                await Task.Delay(300000);
                if (!_didStart)
                    Environment.Exit(1);
            });

            // Load credentials
            if (!File.Exists("Keys/Credentials.json"))
                throw new FileNotFoundException("Missing Credentials file");
            _credentials = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText("Keys/Credentials.json"));

            // Initialize static objects
            StaticObjects.Init();

            // Create saves directories
            if (!Directory.Exists("Saves")) Directory.CreateDirectory("Saves");

            // Initialize culture
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo culture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = culture;

            // Discord callbacks
            _client.MessageReceived += HandleCommandAsync;
            _client.Connected += Connected;

            // Discord modules
            await _commands.AddModuleAsync<Modules.Nsfw.Booru>(null);

            await _client.LoginAsync(TokenType.Bot, _credentials.BotToken);
            await _client.StartAsync();

            // We keep the bot online
            await Task.Delay(-1);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg.Author.IsBot) // We ignore messages from bots
                return;

            var msg = arg as SocketUserMessage;
            if (msg == null) return; // The message received isn't a message we can deal with

            int pos = 0;
            if (msg.HasMentionPrefix(_client.CurrentUser, ref pos))
            {
                var context = new SocketCommandContext(_client, msg);
                var result = await _commands.ExecuteAsync(context, pos, null);
                if (!result.IsSuccess)
                {
                    var error = result.Error.Value;
                    if (error == CommandError.UnmetPrecondition || error == CommandError.BadArgCount)
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
        }

        private async Task Connected()
        {
            _didStart = true;
            await _client.SetGameAsync("Sanara V3 coming soon!", null, ActivityType.CustomStatus);
        }

        public async Task LogError(LogMessage msg)
        {
            CommandException ce = msg.Exception as CommandException;
            if (ce != null)
            {
                if (msg.Exception.InnerException is CommandFailed) // Exception thrown from modules when a command failed
                {
                    await ce.Context.Channel.SendMessageAsync(msg.Exception.InnerException.Message);
                }
                else // Unexpected exception
                {
                    await Utils.Log(msg);
                    await ce.Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
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
