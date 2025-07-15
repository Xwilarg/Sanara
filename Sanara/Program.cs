using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Cloud.Translate.V3;
using Google.Cloud.Vision.V1;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using RevoltSharp;
using Sanara.Compatibility;
using Sanara.Database;
using Sanara.Exception;
using Sanara.Game;
using Sanara.Module.Command;
using Sanara.Module.Command.Context.Discord;
using Sanara.Module.Command.Context.Revolt;
using Sanara.Module.Command.Impl;
using Sanara.Module.Utility;
using Sanara.Service;
using Sanara.Subscription;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Web;
using VndbSharp;

namespace Sanara;

public sealed class Program
{
    private bool _didStart = false; // Keep track if the bot already started (mean it called the "Connected" callback)
    private readonly Dictionary<string, CommandData> _commandsAssociations = [];

    private IServiceProvider _provider;
    private DiscordSocketClient _discordClient;
    private RevoltClient _revoltClient;

    private ulong _debugGuildId;

    public static ulong ClientId;

    public static async Task<IServiceProvider> CreateProviderAsync(DiscordSocketClient client, Credentials credentials)
    {
        Db db = new();

        GameManager gameManager = new();

        SubscriptionManager sub = new();

        var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 Sanara");

        var coll = new ServiceCollection();
        coll
            .AddSingleton(http)
            .AddSingleton<HtmlWeb>() // HTML Parser
            .AddSingleton<Vndb>() // VNDB Client
            .AddSingleton<Random>()
            .AddSingleton<Db>()
            .AddSingleton<StatData>()
            .AddSingleton(sub)
            .AddSingleton(db)
            .AddSingleton(gameManager)
            .AddSingleton<BooruService>()
            .AddSingleton<TranslatorService>()
            .AddSingleton<TopGGClient>()
            .AddSingleton(new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            })
            .AddSingleton<JapaneseConverter>();

        if (File.Exists("Keys/GoogleAPI.json") && credentials.GoogleProjectId != null) // Requires cloudtranslate.generalModels.predict
        {
            var transBuilder = new TranslationServiceClientBuilder
            {
                CredentialsPath = "Keys/GoogleAPI.json"
            };
            coll.AddSingleton(transBuilder.Build());
            var visionBuilder = new ImageAnnotatorClientBuilder
            {
                CredentialsPath = "Keys/GoogleAPI.json"
            };
            coll.AddSingleton(visionBuilder.Build());

        }

        // May only be null in test context
        if (client != null) coll.AddSingleton(client);
        if (credentials != null) coll.AddSingleton(credentials);

        return coll.BuildServiceProvider();
    }

    public static async Task Main()
    {
        try
        {
            // Create saves directories
            if (!Directory.Exists("Saves")) Directory.CreateDirectory("Saves");
            if (!Directory.Exists("Saves/Download")) Directory.CreateDirectory("Saves/Download");
            if (!Directory.Exists("Saves/Game")) Directory.CreateDirectory("Saves/Game");

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
        _discordClient = new(new()
        {
#if DEBUG
            LogLevel = LogSeverity.Verbose,
#endif
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.MessageContent
        });
        _revoltClient = new(ClientMode.WebSocket, new ClientConfig()
        {
            LogMode = RevoltLogSeverity.Info
        });
        _discordClient.Log += Log.LogAsync;

        // Setting Logs callback
        await Log.LogAsync(new LogMessage(LogSeverity.Info, "Setup", "Initialising bot"));

        // Load credentials
        if (!File.Exists("Keys/Credentials.json"))
        {
            if (!Directory.Exists("Keys")) Directory.CreateDirectory("Keys");
            File.WriteAllText("Keys/Credentials.json", JsonSerializer.Serialize(new Credentials(), new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
            throw new FileNotFoundException("Missing Credentials file");
        }
        var credentials = JsonSerializer.Deserialize<Credentials>(File.ReadAllText("Keys/Credentials.json"))!;

        if (credentials.BotToken.DiscordToken == null && credentials.BotToken.RevoltToken == null) throw new FileNotFoundException("Missing bot token");

        if (credentials.UploadWebsiteLocation != null)
        {
            if (!credentials.UploadWebsiteLocation.EndsWith("/")) credentials.UploadWebsiteLocation += "/";

            if (!credentials.UploadWebsiteUrl.EndsWith("/")) credentials.UploadWebsiteUrl += "/"; // Makes sure the URL end with a /
        }

        _debugGuildId = credentials.DebugGuild;

        // Set culture to invarriant (so we don't use , instead of . for decimal separator)
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;


        if (credentials.SentryKey != null)
        {
            SentrySdk.Init(credentials.SentryKey);
        }

        _provider = await CreateProviderAsync(_discordClient, credentials);

        await _provider.GetRequiredService<Db>().InitAsync();

        Log.Init(_provider.GetRequiredService<Db>());

        _ = Task.Run(async () => { try { await _provider.GetRequiredService<SubscriptionManager>().InitAsync(_provider); } catch (System.Exception e) { await Log.LogErrorAsync(e, null); } });
        _ = Task.Run(async () => { try {
                Action<IServiceProvider>[] targets = [
                    Game.Preload.Impl.Static.Shiritori.Init,
                    Game.Preload.Impl.Static.Arknights.Init,
                    Game.Preload.Impl.Static.AzurLane.Init,
                    Game.Preload.Impl.Static.FateGO.Init,
                    Game.Preload.Impl.Static.GirlsFrontline.Init,
                    Game.Preload.Impl.Static.Kancolle.Init,
                    Game.Preload.Impl.Static.Pokemon.Init
                ];
                foreach (var t in targets)
                {
                    try
                    {
                        t(_provider);
                    }
                    catch (System.Exception e)
                    {
                        await Log.LogErrorAsync(e, null);
                    }
                }
                _provider.GetRequiredService<GameManager>().Init(_provider);
            } catch (System.Exception e) { await Log.LogErrorAsync(e, null); } });


        // If the bot takes way too much time to start, we stop the program
        // We do that after the StaticObjects initialization because the first time we load game cache, it can takes plenty of time
        _ = Task.Run(async () =>
        {
            await Task.Delay(Constants.PROGRAM_TIMEOUT);
            if (!_didStart)
                Environment.Exit(1);
        });

        if (credentials.BotToken.DiscordToken != null)
        {
            // Discord callbacks

            // Reactions to messages
            _discordClient.ReactionAdded += async (Cacheable<IUserMessage, ulong> msg, Cacheable<IMessageChannel, ulong> chan, SocketReaction react) =>
            {
                await Module.Utility.Language.TranslateFromReactionAsync(_provider, msg, chan, react);
            };

            // Games
            _discordClient.MessageReceived += HandleCommandAsync;

            // Ready the bot
            _discordClient.Connected += ConnectedAsync;
            _discordClient.Ready += Ready;
            _discordClient.Disconnected += Disconnected;

            // Db
            _discordClient.GuildAvailable += GuildJoined;
            _discordClient.JoinedGuild += GuildJoined;

            // Guild count
            _discordClient.JoinedGuild += ChangeGuildCountAsync;
            _discordClient.LeftGuild += ChangeGuildCountAsync;

            // Interactions
            _discordClient.SlashCommandExecuted += SlashCommandExecuted;
            _discordClient.ButtonExecuted += ButtonExecuted;
            _discordClient.SelectMenuExecuted += SelectMenuExecuted;

            await _discordClient.LoginAsync(TokenType.Bot, credentials.BotToken.DiscordToken);
            await _discordClient.StartAsync();
        }
        if (credentials.BotToken.RevoltToken != null)
        {
            _revoltClient.OnMessageRecieved += OnMessageReceivedRevolt;

            _revoltClient.OnReady += OnReadyRevolt;

            await _revoltClient.LoginAsync(credentials.BotToken.RevoltToken, AccountType.Bot);
            await _revoltClient.StartAsync();
        }

        // We keep the bot online
        await Task.Delay(-1);
    }

    public async Task UpdateTopGgAsync()
    {
        var auth = _provider.GetService<TopGGClient>();
        if (auth != null && auth.ShouldSend) // Make sure to not spam the API
        {
            await auth.SendAsync(_discordClient.Guilds.Count);
        }
    }

    private bool DoesFailAdminOnlyPrecondition(CommonTextChannel? tChan, CommonUser user)
    {
        return tChan != null && tChan.OwnerId != user.Id && !user.IsAdmin;
    }

    private bool DoesFailNsfwOnlyPrecondition(CommonTextChannel? tChan)
    {
        return tChan != null && !tChan.IsNsfw;
    }

    private async Task LaunchCommandAsync(CommandData cmd, CommonUser? user, CommonTextChannel? tChan, bool isSlashCommand, Func<string, bool, Task> errorMsgAsync, Func<Task<IContext>> ctxCreatorAsync)
    {
        if (cmd.IsAdminOnly && DoesFailAdminOnlyPrecondition(tChan, user))
        {
            await errorMsgAsync("This command can only be done by a guild administrator", true);
        }
        else if (cmd.SlashCommand.IsNsfw && DoesFailNsfwOnlyPrecondition(tChan))
        {
            await errorMsgAsync("This command can only be done in NSFW channels", true);
        }
        else if (cmd.SlashCommand.ContextTypes != null && !cmd.SlashCommand.ContextTypes.Contains(InteractionContextType.Guild) &&
            tChan == null)
        {
            await errorMsgAsync("This command can only be done in a guild", true);
        }
        else
        {
            try
            {
                var context = await ctxCreatorAsync();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var db = _provider.GetRequiredService<Db>();

                        await db.AddNewCommandAsync(cmd.SlashCommand.Name.ToUpperInvariant(), isSlashCommand);
                        _provider.GetRequiredService<StatData>().LastMessage = DateTime.UtcNow;
                        await cmd.Callback(context);
                        await db.AddCommandSucceed();
                    }
                    catch (System.Exception e)
                    {
                        if (e is CommandFailed ce)
                        {
                            await errorMsgAsync(e.Message, ce.Ephemeral);
                        }
                        else
                        {
                            await Log.LogErrorAsync(e, context);
                        }
                    }
                });
            }
            catch (System.Exception e)
            {

                if (e is CommandFailed ce)
                {
                    await errorMsgAsync(e.Message, ce.Ephemeral);
                }
                else
                {
                    await Log.LogErrorAsync(e, null);
                }
            }
        }
    }

    public static bool IsBotOwner(CommonUser user) => user.Id == "144851584478740481"; // TODO: ew, and won't work on Revolt

    private readonly List<ulong> _pendingRequests = [];
    private readonly List<string> _downloadRequests = [];

    private async Task SelectMenuExecuted(SocketMessageComponent arg)
    {
        if (arg.Data.CustomId == "delCache" && IsBotOwner(new CommonUser(arg.User)))
        {
            await _provider.GetRequiredService<Db>().DeleteCacheAsync(arg.Data.Values.ElementAt(0));
            await arg.RespondAsync("Cache deleted", ephemeral: true);
        }
    }

    private async Task ButtonExecuted(SocketMessageComponent arg)
    {
        var ctx = new ComponentCommandContext(_provider, arg);
        if (_pendingRequests.Contains(arg.User.Id))
        {
            await ctx.ReplyAsync("A component request is already being treated for you, please retry afterward", ephemeral: true);
            return;
        }
        _pendingRequests.Add(arg.User.Id);
        try
        {
            if (arg.Data.CustomId == "globalStats")
            {
                var embed = new CommonEmbedBuilder();

                // Get informations about games
                StringBuilder str = new();
                List<string> gameNames = [];
                foreach (var elem in _provider.GetRequiredService<GameManager>().Preloads)
                {
                    // We only get games once so we skip when we get the "others" versions (like audio)
                    //if (elem.GetNameArg() != null && elem.GetNameArg() != "hard")
                    //    continue;
                    // var fullName = name + (elem.GetNameArg() != null ? $" {elem.GetNameArg()}" : "");
                    try
                    {
                        var loadInfo = elem.Load();
                        if (loadInfo != null)
                            str.AppendLine($"**{Utils.ToWordCase(elem.Name)}**: {elem.Load().Count} words.");
                        else // Get information at runtime
                            str.AppendLine($"**{Utils.ToWordCase(elem.Name)}**: None");
                    }
                    catch (System.Exception e)
                    {
                        str.AppendLine($"**{Utils.ToWordCase(elem.Name)}**: Failed to load: {e.GetType().ToString()}");
                    }
                }
                embed.AddField("Games", str.ToString());

                // Get information about subscriptions
                var subs = _provider.GetRequiredService<SubscriptionManager>().GetSubscriptionCount(_provider);
                embed.AddField("Subscriptions",
                    subs == null ?
                        "Not yet initialized" :
#if NSFW_BUILD
                        string.Join("\n", subs.Select(x => "**" + char.ToUpper(x.Key[0]) + string.Join("", x.Key.Skip(1)) + "**: " + x.Value)));
#else
                "**Anime**: " + subs["anime"]);
#endif

                await ctx.ReplyAsync(embed: embed);
                _pendingRequests.Remove(arg.User.Id);
            }
            else if (arg.Data.CustomId.StartsWith("tr-") && _provider.GetRequiredService<TranslatorService>().TranslationOriginalText.ContainsKey(arg.Data.CustomId[3..]))
            {
                await ctx.ReplyAsync(embed: new CommonEmbedBuilder
                {
                    Title = "Original Text",
                    Description = _provider.GetRequiredService<TranslatorService>().TranslationOriginalText[arg.Data.CustomId[3..]],
                    Color = Color.Blue,
                });
                _provider.GetRequiredService<TranslatorService>().TranslationOriginalText.Remove(arg.Data.CustomId[3..]);
                _pendingRequests.Remove(arg.User.Id);
            }
            else if (arg.Data.CustomId == "dump")
            {
                if (!DoesFailAdminOnlyPrecondition(arg.Channel is ITextChannel tChan ? new CommonTextChannel(tChan) : null, new CommonUser(arg.User)))
                {
                    await Module.Button.Settings.DatabaseDump(ctx);
                }
                else
                {
                    throw new CommandFailed("You don't have the permissions to do this");
                }
                _pendingRequests.Remove(arg.User.Id);
            }
            else if (arg.Data.CustomId == "flag")
            {
                if (!DoesFailAdminOnlyPrecondition(arg.Channel is ITextChannel tChan ? new CommonTextChannel(tChan) : null, new CommonUser(arg.User)))
                {
                    var c = (ITextChannel)ctx.Channel;
                    var guildId = c.GuildId;
                    var newValue = !_provider.GetRequiredService<Db>().GetGuild(guildId).TranslateUsingFlags;
                    await _provider.GetRequiredService<Db>().UpdateFlagAsync(guildId, newValue);
                    await ctx.ReplyAsync($"Translation from flag is now {(newValue ? "enabled" : "disabled")}", ephemeral: true);
                    // TODO: Update display
                }
                else
                {
                    throw new CommandFailed("You don't have the permissions to do this");
                }
                _pendingRequests.Remove(arg.User.Id);
            }
            else if (arg.Data.CustomId.StartsWith("delSub-"))
            {
                await arg.DeferLoadingAsync();
                if (!DoesFailAdminOnlyPrecondition(arg.Channel is ITextChannel tChan ? new CommonTextChannel(tChan) : null, new CommonUser(arg.User)))
                {
                    await Module.Button.Settings.RemoveSubscription(ctx, arg.Data.CustomId[7..]);
                }
                else
                {
                    throw new CommandFailed("You don't have the permissions to do this");
                }
                _pendingRequests.Remove(arg.User.Id);
            }
            else if (arg.Data.CustomId.StartsWith("error-") && Log.Errors.ContainsKey(arg.Data.CustomId))
            {
                var e = Log.Errors[arg.Data.CustomId];
                await ctx.ReplyAsync(embed: new CommonEmbedBuilder
                {
                    Color = Color.Red,
                    Title = e.GetType().ToString(),
                    Description = e.Message
                }, ephemeral: true);
                _pendingRequests.Remove(arg.User.Id);
            }
            else if (arg.Data.CustomId.StartsWith("booru-"))
            {
                var target = arg.Data.CustomId[6..];
                var data = _provider.GetRequiredService<BooruService>().Results[target];
                var embed = new CommonEmbedBuilder
                {
                    Color = Color.Blue,
                    Description = $"Tags\n{string.Join(", ", data.Tags)}",
                    ImageUrl = data.PreviewUrl?.ToString()
                };
                embed.AddField("Size", $"{data.Width}x{data.Height}", true);
                if (!string.IsNullOrEmpty(data.Source)) embed.AddField("Source", HttpUtility.HtmlDecode(data.Source), true);
                await ctx.ReplyAsync(embed: embed, ephemeral: true);
                _pendingRequests.Remove(arg.User.Id);
            }
            else if (arg.Data.CustomId.StartsWith("lyrics-"))
            {
                var target = arg.Data.CustomId[7..];
                Lyrics.DisplayMode mode;
                if (target.StartsWith("kanji-"))
                {
                    mode = Lyrics.DisplayMode.Kanji;
                    target = target[6..];
                }
                else if (target.StartsWith("hiragana-"))
                {
                    mode = Lyrics.DisplayMode.Hiragana;
                    target = target[9..];
                }
                else if (target.StartsWith("romaji-"))
                {
                    mode = Lyrics.DisplayMode.Romaji;
                    target = target[7..];
                }
                else throw new NotImplementedException();
                var web = ctx.Provider.GetRequiredService<HtmlWeb>();
                Console.WriteLine($"https://utaten.com{target}");
                var html = web.Load($"https://utaten.com{target}");

                var d = await Lyrics.GetRawLyricsAsync(html, mode);
                var embed = arg.Message.Embeds.First();
                await arg.Message.ModifyAsync(x => x.Embed = new Discord.EmbedBuilder() {
                    Description = d,
                    Title = embed.Title,
                    ImageUrl = embed.Image?.Url
                }.Build());
                await ctx.ReplyAsync("Lyrics updated", ephemeral: true);
                _pendingRequests.Remove(arg.User.Id);
            }
            else if (arg.Data.CustomId.StartsWith("download-"))
            {
                var target = arg.Data.CustomId[9..];
                lock (_downloadRequests)
                {
                    if (_downloadRequests.Contains(target))
                    {
                        return;
                    }
                    _downloadRequests.Add(target);
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await arg.Message.ModifyAsync(x => x.Components = new ComponentBuilder().Build());
                        }
                        catch (System.Exception e)
                        {
                            await Log.LogErrorAsync(e, ctx);
                        }
                        finally
                        {
                            _downloadRequests.Remove(target);
                        }
                    });
                }

                if (target.StartsWith("ehentai-"))
                {
                    var ids = target[8..];
                    var type = ids[0];
                    ids = ids[2..];
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await ctx.ReplyAsync("Your file is being downloaded...");
                            await EHentai.EHentaiDownloadAsync(ctx, _provider, ids, type switch
                            {
                                'c' => EHentai.EHentaiType.Cosplay,
                                'd' => EHentai.EHentaiType.Doujinshi,
                                _ => (EHentai.EHentaiType)(-1)
                            });
                        }
                        catch (System.Exception e)
                        {
                            await Log.LogErrorAsync(e, ctx);
                        }
                        finally
                        {
                            _pendingRequests.Remove(arg.User.Id);
                        }
                    });
                }
            }
            else if (arg.Data.CustomId.StartsWith("replay/"))
            {
                var id = arg.Data.CustomId[7..];
                var chanId = ctx.Channel.Id;
                switch (id)
                {
                    case "ready":
                        var rLobby = _provider.GetRequiredService<GameManager>().GetReplayLobby(ctx.Channel);
                        if (rLobby == null)
                        {
                            await ctx.ReplyAsync("This lobby expired, please create a new one with the play command", ephemeral: true);
                        }
                        else
                        {
                            var embed = _provider.GetRequiredService<GameManager>().ToggleReadyLobby(rLobby, ctx.User);
                            if (embed != null)
                            {
                                await ctx.ReplyAsync("Ready state changed", ephemeral: true);
                                if (await _provider.GetRequiredService<GameManager>().CheckRestartLobbyFullAsync(ctx))
                                {
                                    await arg.Message.DeleteAsync();
                                }
                                else
                                {
                                    await arg.Message.ModifyAsync(x => x.Embed = embed.ToDiscord());
                                }
                            }
                            else
                            {
                                await ctx.ReplyAsync("You are not in the lobby", ephemeral: true); // TODO
                            }
                        }
                        break;

                    case "delete":
                        _provider.GetRequiredService<GameManager>().DeleteReadyLobby(ctx.Channel);
                        await ctx.ReplyAsync("Replay lobby deleted", ephemeral: true);
                        await arg.Message.DeleteAsync();
                        break;

                    default:
                        throw new NotImplementedException("Invalid id " + id);
                }
                _pendingRequests.Remove(arg.User.Id);
            }
            else if (arg.Data.CustomId.StartsWith("game/"))
            {
                var id = arg.Data.CustomId[5..];
                var chanId = ctx.Channel.Id.ToString();
                var lobby = _provider.GetRequiredService<GameManager>().GetLobby(chanId);
                if (lobby == null)
                {
                    await ctx.ReplyAsync("This lobby is closed", ephemeral: true);
                }
                else
                {
                    switch (id)
                    {
                        case "start":
                            if (lobby.IsHost(ctx.User))
                            {
                                await arg.DeferLoadingAsync();
                                await _provider.GetRequiredService<GameManager>().StartGameAsync(ctx);
                                await arg.Message.DeleteAsync();
                            }
                            else
                            {
                                await ctx.ReplyAsync("Only the host can do that", ephemeral: true);
                            }
                            break;

                        case "join":
                            if (lobby.IsHost(ctx.User))
                            {
                                await ctx.ReplyAsync("You can't leave the lobby as the host, press the cancel button instead", ephemeral: true);
                            }
                            else
                            {
                                await arg.DeferLoadingAsync();
                                var state = lobby.ToggleUser(ctx.User);
                                await ctx.ReplyAsync($"You {(state ? "joined" : "leaved")} the lobby", ephemeral: true);
                                await arg.Message.ModifyAsync(x => x.Embed = lobby.GetIntroEmbed().ToDiscord());
                            }
                            break;

                        case "multi":
                            if (lobby.IsHost(ctx.User))
                            {
                                lobby.ToggleMultiplayerMode();
                                await ctx.ReplyAsync("You changed the multiplayer type", ephemeral: true);
                                await arg.Message.ModifyAsync(x => x.Embed = lobby.GetIntroEmbed().ToDiscord());
                            }
                            else
                            {
                                await ctx.ReplyAsync("Only the host can do that", ephemeral: true);
                            }
                            break;

                        case "cancel":
                            if (lobby.IsHost(ctx.User))
                            {
                                _provider.GetRequiredService<GameManager>().RemoveLobby(chanId);
                                await arg.Message.DeleteAsync();
                                await ctx.ReplyAsync($"The lobby was cancelled", ephemeral: true);
                            }
                            else
                            {
                                await ctx.ReplyAsync("Only the host can do that", ephemeral: true);
                            }
                            break;

                        default:
                            throw new NotImplementedException("Invalid id " + id);
                    }
                }
                _pendingRequests.Remove(arg.User.Id);
            }
            else
            {
                await ctx.ReplyAsync("There is no data associated to this button, that probably mean it was already requested", ephemeral: true);
                _pendingRequests.Remove(arg.User.Id);
            }
        }
        catch (CommandFailed cf)
        {
            await ctx.ReplyAsync(cf.Message, ephemeral: true);
            _pendingRequests.Remove(arg.User.Id);
        }
        catch (System.Exception ex)
        {
            await Log.LogErrorAsync(ex, ctx);
            _pendingRequests.Remove(arg.User.Id);
        }
    }

    private async Task SlashCommandExecuted(SocketSlashCommand arg)
    {
        if (!_commandsAssociations.ContainsKey(arg.CommandName.ToUpperInvariant()))
        {
            throw new NotImplementedException($"Unknown command {arg.CommandName}");
        }
        var cmd = _commandsAssociations[arg.CommandName.ToUpperInvariant()];
        await LaunchCommandAsync(cmd, new CommonUser(arg.User), arg.Channel is ITextChannel tChan ? new CommonTextChannel(tChan) : null, true, async (string content, bool ephemeral) =>
        {
            if (arg.HasResponded)
            {
                await arg.ModifyOriginalResponseAsync(x => x.Content = content);
            }
            else
            {
                await arg.RespondAsync(content, ephemeral: ephemeral);
            }
        }, async () =>
        {
            //if (cmd.NeedDefer)
            {
                await arg.DeferAsync(); // Somehow all commands not defer-ed fail
            }
            return new SlashCommandContext(_provider, arg);
        });
    }

    public static ISubmodule[] Submodules => [
        new Media(),
        new Module.Command.Impl.Game(),
        new Module.Command.Impl.Language(),
        new Doujin(),
        new Music(),
        new Module.Command.Impl.Settings(),
        new Module.Command.Impl.Subscription()
    ];

    private void OnReadyRevolt(SelfUser _)
    {
        Ready().GetAwaiter().GetResult();
    }

    private async Task Ready()
    {
        if (_commandsAssociations.Count != 0)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                var submobules = Submodules;

                SocketGuild? debugGuild = null;
                if (_debugGuildId != 0 && Debugger.IsAttached)
                {
                    debugGuild = _discordClient?.GetGuild(_debugGuildId);
                }

                // Preload commands
                foreach (var s in submobules)
                {
                    await Log.LogAsync(new(LogSeverity.Verbose, "Cmd Preload", $"[Module] {s.Name}"));
                    foreach (var c in s.GetCommands(_provider)
#if !NSFW_BUILD
                        // NSFW build doesn't preload NSFW commands
                        .Where(x => !x.SlashCommand.IsNsfw)
#endif
                    )
                    {
                        await Log.LogAsync(new(LogSeverity.Verbose, "Cmd Preload", $"[Command] {c.SlashCommand.Name}"));
                        _commandsAssociations.Add(c.SlashCommand.Name.ToUpperInvariant(), c);
                    }
                }

                // Send everything to Discord
                if (_discordClient.ConnectionState != ConnectionState.Disconnected)
                {
                    foreach (var c in _commandsAssociations.Values.Select(x => x.SlashCommand.Build()))
                    {
                        if (debugGuild != null)
                        {
                            await debugGuild.CreateApplicationCommandAsync(c);
                        }
                        else
                        {
                            await _discordClient.CreateGlobalApplicationCommandAsync(c);
                        }
                    }
                }

                if (_discordClient.ConnectionState != ConnectionState.Disconnected)
                {
                    var cmds = _commandsAssociations.Values.Select(x => x.SlashCommand.Build());
                    if (debugGuild != null)
                    {
                        await debugGuild.BulkOverwriteApplicationCommandAsync(cmds.ToArray());
                    }
                    else
                    {
                        await _discordClient.BulkOverwriteGlobalApplicationCommandsAsync(cmds.ToArray());
                    }
                }
                await Log.LogAsync(new LogMessage(LogSeverity.Info, "Ready Handler", "Commands loaded"));
            }
            catch (System.Exception ex)
            {
                await Log.LogErrorAsync(ex, null);
                await Log.LogAsync(new LogMessage(LogSeverity.Critical, "Ready Handler", "Some commands failed to load!"));
            }

            // The bot is now really ready to interact with people
            _provider.GetRequiredService<StatData>().Started = DateTime.UtcNow;

            await _provider.GetRequiredService<Db>().UpdateGuildCountAsync(_discordClient);
        });
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
        await _provider.GetRequiredService<TopGGClient>().SendAsync(_discordClient.Guilds.Count);
        await _provider.GetRequiredService<Db>().UpdateGuildCountAsync(_discordClient);
    }

    private async Task GuildJoined(SocketGuild guild)
    {
        await _provider.GetRequiredService<Db>().InitGuildAsync(_provider, guild);
    }

    private async void OnMessageReceivedRevolt(Message arg) // TODO: Need to Task.Run?
    {
        if (arg.Author.IsBot || arg is not UserMessage msg || string.IsNullOrWhiteSpace(msg.Content)) return;

        var myMention = $"<@{_revoltClient.CurrentUser.Id}>";
        if (msg.Content.StartsWith(myMention))
        {
            var content = msg.Content[myMention.Length..].TrimStart();
            var commandStr = content.Split(' ')[0].ToUpperInvariant();
            CommandData? cmd = null;
            if (_commandsAssociations.ContainsKey(commandStr))
            {
                cmd = _commandsAssociations[commandStr];
            }
            if (cmd == null && _commandsAssociations.Any(x => x.Value.Aliases.Contains(commandStr)))
            {
                cmd = _commandsAssociations.FirstOrDefault(x => x.Value.Aliases.Contains(commandStr)).Value;
            }
            if (cmd != null)
            {
                await LaunchCommandAsync(cmd, new CommonUser(msg.Author), msg.Channel is RevoltSharp.TextChannel tChan ? new CommonTextChannel(tChan) : null, false, async (string content, bool ephemeral) =>
                {
                    await msg.Channel.SendMessageAsync(content);
                }, async () =>
                {
                    var newContent = content[commandStr.Length..].TrimStart();
                    if (msg.Attachments.Any())
                    {
                        // newContent += " " + msg.Attachments.ElementAt(0).Url; TODO
                    }
                    return new RevoltMessageCommandContext(_provider, msg, newContent, cmd);
                });
            }
        }
        else if (!msg.Content.StartsWith("//") && !msg.Content.StartsWith("#"))
        {
            // TODO: Implement game stuffs
        }
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        if (arg.Author.IsBot || arg is not SocketUserMessage msg || string.IsNullOrWhiteSpace(msg.Content)) return; // The message received isn't one we can deal with

        // Deprecation warning
        int pos = 0;
        if (msg.HasMentionPrefix(_discordClient.CurrentUser, ref pos))
        {
            var content = msg.Content[pos..];
            var commandStr = content.Split(' ')[0].ToUpperInvariant();
            CommandData? cmd = null;
            if (_commandsAssociations.ContainsKey(commandStr))
            {
                cmd = _commandsAssociations[commandStr];
            }
            if (cmd == null && _commandsAssociations.Any(x => x.Value.Aliases.Contains(commandStr)))
            {
                cmd = _commandsAssociations.FirstOrDefault(x => x.Value.Aliases.Contains(commandStr)).Value;
            }
            if (cmd != null)
            {
                await LaunchCommandAsync(cmd, new CommonUser(msg.Author), msg.Channel is ITextChannel tChan ? new CommonTextChannel(tChan) : null, false, async (string content, bool ephemeral) =>
                {
                    await msg.ReplyAsync(content);
                }, async () =>
                {
                    var newContent = content[commandStr.Length..].TrimStart();
                    if (msg.Attachments.Any())
                    {
                        newContent += " " + msg.Attachments.ElementAt(0).Url;
                    }
                    return new DiscordMessageCommandContext(_provider, msg, newContent, cmd);
                });
            }
        }
        else if (!msg.Content.StartsWith("//") && !msg.Content.StartsWith("#"))
        {
            var context = new GameCommandContext(_provider, msg);
            var game = _provider.GetRequiredService<GameManager>().GetGame(new(msg.Channel));
            if (game != null && game.CanPlay(new CommonUser(msg.Author)))
            {
                game.AddAnswer(context);
            }
        }
    }

    private async Task ConnectedAsync()
    {
        _didStart = true;
        ClientId = _discordClient.CurrentUser.Id;
        var drama = _provider.GetService<TopGGClient>();
        if (drama != null) drama.Init(_discordClient.CurrentUser.Id, _provider.GetRequiredService<Credentials>().TopGgToken);
        await _provider.GetService<TopGGClient>().SendAsync(_discordClient.Guilds.Count);
    }
}
