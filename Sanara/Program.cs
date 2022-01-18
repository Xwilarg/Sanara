using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Sanara.Diaporama;
using Sanara.Exception;
using Sanara.Module.Button;
using Sanara.Module.Command;
using Sanara.Module.Command.Impl;
using System.Diagnostics;
using System.Globalization;

namespace Sanara
{
    public sealed class Program
    {
        private bool _didStart = false; // Keep track if the bot already started (mean it called the "Connected" callback)
        private readonly Dictionary<string, Module.Command.CommandInfo> _commandsAssociations = new();

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

            // DEPRECATED
            StaticObjects.Client.MessageReceived += HandleCommandAsync;
            StaticObjects.Client.ReactionAdded += ReactionManager.ReactionAddedAsync;

            // Ready the bot
            StaticObjects.Client.Connected += ConnectedAsync;
            StaticObjects.Client.Ready += Ready;
            StaticObjects.Client.Disconnected += Disconnected;

            // Db
            StaticObjects.Client.GuildAvailable += GuildJoined;
            StaticObjects.Client.JoinedGuild += GuildJoined;

            // Guild count
            StaticObjects.Client.JoinedGuild += ChangeGuildCountAsync;
            StaticObjects.Client.LeftGuild += ChangeGuildCountAsync;

            // Interactions
            StaticObjects.Client.SlashCommandExecuted += SlashCommandExecuted;
            StaticObjects.Client.AutocompleteExecuted += AutocompleteExecuted;
            StaticObjects.Client.ButtonExecuted += ButtonExecuted;

            await StaticObjects.Client.LoginAsync(TokenType.Bot, _credentials.BotToken);
            await StaticObjects.Client.StartAsync();

            // We keep the bot online
            await Task.Delay(-1);
        }

        private async Task AutocompleteExecuted(SocketAutocompleteInteraction arg)
        {
            var input = arg.Data.Current.Value;
            if (arg.Data.CommandName == "adultvideo")
            {
                if (arg.Channel is ITextChannel tChan && !tChan.IsNsfw)
                {
                    await arg.RespondAsync(new[]
                    {
                        new AutocompleteResult("This command must be done in a NSFW channel", string.Empty)
                    });
                }
                else
                {
                    if ((string)input == string.Empty)
                    {
                        await arg.RespondAsync(StaticObjects.JavmostCategories
                            .Select(tag =>
                            {
                                return new AutocompleteResult(tag.Tag, tag.Tag.ToLowerInvariant());
                            })
                            .Take(25)
                        );
                    }
                    else
                    {
                        await arg.RespondAsync(StaticObjects.JavmostCategories
                            .Where(tag => tag.Item1.ToLowerInvariant().StartsWith((string)input))
                            .Select(tag =>
                            {
                                return new AutocompleteResult(tag.Tag, tag.Tag.ToLowerInvariant());
                            })
                            .Take(25)
                        );
                    }
                }
            }
        }

        private async Task ButtonExecuted(SocketMessageComponent ctx)
        {
            try
            {
                if (ctx.Data.CustomId == "dump")
                {
                    await Module.Button.Settings.DatabaseDump(ctx);
                }
                else if (ctx.Data.CustomId.StartsWith("delSub-"))
                {
                    await Module.Button.Settings.RemoveSubscription(ctx, ctx.Data.CustomId[7..]);
                }
                else if (StaticObjects.Errors.ContainsKey(ctx.Data.CustomId))
                {
                    var e = StaticObjects.Errors[ctx.Data.CustomId];
                    await ctx.RespondAsync(embed: new EmbedBuilder
                    {
                        Color = Color.Red,
                        Title = e.GetType().ToString(),
                        Description = e.Message
                    }.Build(), ephemeral: true);
                }
                else
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (StaticObjects.Cosplays.Contains(ctx.Data.CustomId))
                            {
                                StaticObjects.Cosplays.Remove(ctx.Data.CustomId);
                                await ctx.DeferLoadingAsync();
                                var id = ctx.Data.CustomId.Split('/');
                                await Cosplay.DownloadCosplayAsync(ctx, id[1], id[2]);
                            }
                            else if (StaticObjects.Doujinshis.Contains(ctx.Data.CustomId))
                            {
                                StaticObjects.Doujinshis.Remove(ctx.Data.CustomId);
                                await ctx.DeferLoadingAsync();
                                var id = ctx.Data.CustomId.Split('/').Last();
                                await Booru.GetTagsAsync(ctx, id);
                            }
                            else if (ctx.Data.CustomId.StartsWith("game/"))
                            {
                                var id = ctx.Data.CustomId.Split('/');
                                if (StaticObjects.GameManager.DoesLobbyExists(id[1]))
                                {
                                    switch (id[2])
                                    {
                                        case "start":
                                            await StaticObjects.GameManager.StartGameAsync(ctx, id[1]);
                                            break;

                                        default:
                                            throw new NotImplementedException("Invalid id " + id[2]);
                                    }
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            await Log.LogErrorAsync(ex, ctx);
                        }
                    });
                }
            }
            catch (System.Exception ex)
            {
                await Log.LogErrorAsync(ex, ctx);
            }
        }

        private async Task SlashCommandExecuted(SocketSlashCommand ctx)
        {
            if (!_commandsAssociations.ContainsKey(ctx.CommandName.ToUpperInvariant()))
            {
                throw new NotImplementedException($"Unknown command {ctx.CommandName}");
            }
            var cmd = _commandsAssociations[ctx.CommandName.ToUpperInvariant()];
            _ = Task.Run(async () =>
            {
                try
                {
                    var tChan = ctx.Channel as ITextChannel;
                    if ((cmd.Precondition & Precondition.NsfwOnly) != 0 &&
                        tChan != null && !tChan.IsNsfw)
                    {
                        await ctx.RespondAsync("This command can only be done in NSFW channels", ephemeral: true);
                    }
                    else if ((cmd.Precondition & Precondition.AdminOnly) != 0 &&
                        tChan != null && tChan.Guild.OwnerId != ctx.User.Id && !((IGuildUser)ctx.User).GuildPermissions.ManageGuild)
                    {
                        await ctx.RespondAsync("This command can only be done by a guild administrator", ephemeral: true);
                    }
                    else if ((cmd.Precondition & Precondition.GuildOnly) != 0 &&
                        tChan == null)
                    {
                        await ctx.RespondAsync("This command can only be done in a guild", ephemeral: true);
                    }
                    else
                    {
                        if (cmd.NeedDefer)
                        {
                            await ctx.DeferAsync();
                        }

                        await cmd.Callback(new SocketSlashCommandContext(ctx));
                        StaticObjects.LastMessage = DateTime.UtcNow;

                        await StaticObjects.Db.AddNewCommandAsync(ctx.CommandName.ToUpperInvariant());
                    }
                }
                catch (System.Exception e)
                {
                    if (e is CommandFailed)
                    {
                        if (ctx.HasResponded)
                        {
                            await ctx.ModifyOriginalResponseAsync(x => x.Content = e.Message);
                        }
                        else
                        {
                            await ctx.RespondAsync(e.Message, ephemeral: true);
                        }
                    }
                    else
                    {
                        await Log.LogErrorAsync(e, ctx);
                    }
                }
            });
        }

        private async Task Ready()
        {
            // Commands already loaded
            if (_commandsAssociations.Count != 0)
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    List<ISubmodule> _submodules = new();

                    // Add submodules
                    _submodules.Add(new Entertainment());
                    _submodules.Add(new Module.Command.Impl.Game());
                    _submodules.Add(new Language());
                    _submodules.Add(new NSFW());
                    _submodules.Add(new Module.Command.Impl.Settings());
                    _submodules.Add(new Module.Command.Impl.Subscription());
                    _submodules.Add(new Tool());

                    StaticObjects.Help = new(_submodules);
                    File.WriteAllText("Saves/Help.json", JsonConvert.SerializeObject(StaticObjects.Help.Data));

                    SocketGuild? debugGuild = null;
                    if (StaticObjects.DebugGuildId != 0 && Debugger.IsAttached)
                    {
                        debugGuild = StaticObjects.Client.GetGuild(StaticObjects.DebugGuildId);
                    }

                    foreach (var s in _submodules)
                    {
                        foreach (var c in s.GetCommands()
#if !NSFW_BUILD
                            .Where(x => (x.Precondition & Precondition.NsfwOnly) == 0)
#endif
                        )
                        {
                            if (c.Precondition != Precondition.None)
                            {
                                c.SlashCommand.Description = $"({c.Precondition}) {c.SlashCommand.Description}";
                            }
                            if (debugGuild != null)
                            {
                                await debugGuild.CreateApplicationCommandAsync(c.SlashCommand);
                            }
                            else
                            {
                                await StaticObjects.Client.CreateGlobalApplicationCommandAsync(c.SlashCommand);
                            }
                            _commandsAssociations.Add(c.SlashCommand.Name.Value.ToUpperInvariant(), c);
                        }
                    }

                    var cmds = _commandsAssociations.Values
#if !NSFW_BUILD
                            .Where(x => (x.Precondition & Precondition.NsfwOnly) == 0)
#endif
                    .Select(x => x.SlashCommand);
                    if (debugGuild != null)
                    {
                        await debugGuild.BulkOverwriteApplicationCommandAsync(cmds.ToArray());
                    }
                    else
                    {
                        await StaticObjects.Client.BulkOverwriteGlobalApplicationCommandsAsync(cmds.ToArray());
                    }
                    await Log.LogAsync(new LogMessage(LogSeverity.Info, "Ready Handler", "Commands loaded"));
                }
                catch (System.Exception ex)
                {
                    await Log.LogErrorAsync(ex, null);
                    await Log.LogAsync(new LogMessage(LogSeverity.Critical, "Ready Handler", "Some commands failed to load!"));
                }

                // The bot is now really ready to interact with people
                StaticObjects.Started = DateTime.UtcNow;

                await StaticObjects.Db.UpdateGuildCountAsync();
            });

#if NSFW_BUILD
            await StaticObjects.Client.SetActivityAsync(new Discord.Game("https://sanara.zirk.eu", ActivityType.Watching));
#endif
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
            await StaticObjects.Db.UpdateGuildCountAsync();
        }

        private async Task GuildJoined(SocketGuild guild)
        {
            await StaticObjects.Db.InitGuildAsync(guild);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg.Author.IsBot || arg is not SocketUserMessage msg) return; // The message received isn't a message we can deal with


            // Deprecation warning
            int pos = 0;
            var prefix = msg.Channel is not ITextChannel textChan ? "s." : StaticObjects.Db.GetGuild(textChan.GuildId).Prefix;
            if (msg.HasMentionPrefix(StaticObjects.Client.CurrentUser, ref pos) || msg.HasStringPrefix(prefix, ref pos))
            {
                var context = new SocketCommandContext(StaticObjects.Client, msg);
                //await _commands.ExecuteAsync(context, pos, null);
            }
        }

        private async Task ConnectedAsync()
        {
            _didStart = true;
            StaticObjects.ClientId = StaticObjects.Client.CurrentUser.Id;
            await StaticObjects.UpdateTopGgAsync();
        }
    }
}
