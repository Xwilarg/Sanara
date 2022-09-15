using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Sanara.Exception;
using Sanara.Module.Button;
using Sanara.Module.Command;
using Sanara.Module.Command.Context;
using Sanara.Module.Command.Impl;
using System.Diagnostics;
using System.Globalization;
using System.Text;

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
            try
            {
                await StaticObjects.InitializeAsync(_credentials);
            }
            catch (System.Exception e)
            {
                await Log.LogErrorAsync(e, null);
                throw;
            }

            // If the bot takes way too much time to start, we stop the program
            // We do that after the StaticObjects initialization because the first time we load game cache, it can takes plenty of time
            _ = Task.Run(async () =>
            {
                await Task.Delay(Constants.PROGRAM_TIMEOUT);
                if (!_didStart)
                    Environment.Exit(1);
            });

            // Discord callbacks

            // Reactions to messages
            StaticObjects.Client.ReactionAdded += Module.Utility.Language.TranslateFromReactionAsync;

            // Games
            StaticObjects.Client.MessageReceived += HandleCommandAsync;

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
            StaticObjects.Client.SelectMenuExecuted += SelectMenuExecuted;

            await StaticObjects.Client.LoginAsync(TokenType.Bot, _credentials.BotToken);
            await StaticObjects.Client.StartAsync();

            // We keep the bot online
            await Task.Delay(-1);
        }

        private bool DoesFailNsfwOnlyPrecondition(ITextChannel? tChan)
        {
            return tChan != null && !tChan.IsNsfw;
        }

        private bool DoesFailAdminOnlyPrecondition(ITextChannel? tChan, IUser user)
        {
            return tChan != null && tChan.Guild.OwnerId != user.Id && !((IGuildUser)user).GuildPermissions.ManageGuild;
        }

        private async Task LaunchCommandAsync(Module.Command.CommandInfo cmd, IUser user, ITextChannel? tChan, bool isSlashCommand, Func<string, bool, Task> errorMsgAsync, Func<Task<Module.Command.ICommandContext>> ctxCreatorAsync)
        {
            if ((cmd.Precondition & Precondition.OwnerOnly) != 0 && user.Id != 144851584478740481) // TODO: May want to not hardcode that
            {
                await errorMsgAsync("This command can only be done by the bot owner", true);
            }
            else if ((cmd.Precondition & Precondition.NsfwOnly) != 0 && DoesFailNsfwOnlyPrecondition(tChan))
            {
                await errorMsgAsync("This command can only be done in NSFW channels", true);
            }
            else if ((cmd.Precondition & Precondition.AdminOnly) != 0 && DoesFailAdminOnlyPrecondition(tChan, user))
            {
                await errorMsgAsync("This command can only be done by a guild administrator", true);
            }
            else if ((cmd.Precondition & Precondition.GuildOnly) != 0 &&
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
                            await StaticObjects.Db.AddNewCommandAsync(cmd.SlashCommand.Name.Value.ToUpperInvariant(), isSlashCommand);
                            StaticObjects.LastMessage = DateTime.UtcNow;
                            await cmd.Callback(context);
                            await StaticObjects.Db.AddCommandSucceed();
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

        private readonly List<ulong> _pendingRequests = new();

        private async Task SelectMenuExecuted(SocketMessageComponent arg)
        {
            if (arg.Data.CustomId == "delCache" && StaticObjects.IsBotOwner(arg.User))
            {
                await StaticObjects.Db.DeleteCacheAsync(arg.Data.Values.ElementAt(0));
                await arg.RespondAsync("Cache deleted", ephemeral: true);
            }
        }

        private async Task ButtonExecuted(SocketMessageComponent arg)
        {
            var ctx = new ComponentCommandContext(arg);
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
                    var embed = new EmbedBuilder();

                    // Get informations about games
                    StringBuilder str = new();
                    List<string> gameNames = new();
                    foreach (var elem in StaticObjects.Preloads)
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
                    var subs = StaticObjects.GetSubscriptionCount();
                    embed.AddField("Subscriptions",
                        subs == null ?
                            "Not yet initialized" :
#if NSFW_BUILD
                            string.Join("\n", subs.Select(x => "**" + char.ToUpper(x.Key[0]) + string.Join("", x.Key.Skip(1)) + "**: " + x.Value)));
#else
                    "**Anime**: " + subs["anime"]);
#endif

                    await ctx.ReplyAsync(embed: embed.Build());
                    _pendingRequests.Remove(arg.User.Id);
                }
                else if (arg.Data.CustomId == "dump")
                {
                    if (!DoesFailAdminOnlyPrecondition(arg.Channel as ITextChannel, arg.User))
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
                    if (!DoesFailAdminOnlyPrecondition(arg.Channel as ITextChannel, arg.User))
                    {
                        var tChan = (ITextChannel)ctx.Channel;
                        var guildId = tChan.GuildId;
                        var newValue = !StaticObjects.Db.GetGuild(guildId).TranslateUsingFlags;
                        await StaticObjects.Db.UpdateFlagAsync(guildId, newValue);
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
                    if (!DoesFailAdminOnlyPrecondition(arg.Channel as ITextChannel, arg.User))
                    {
                        await Module.Button.Settings.RemoveSubscription(ctx, arg.Data.CustomId[7..]);
                    }
                    else
                    {
                        throw new CommandFailed("You don't have the permissions to do this");
                    }
                    _pendingRequests.Remove(arg.User.Id);
                }
                else if (arg.Data.CustomId.StartsWith("error-") && StaticObjects.Errors.ContainsKey(arg.Data.CustomId))
                {
                    var e = StaticObjects.Errors[arg.Data.CustomId];
                    await ctx.ReplyAsync(embed: new EmbedBuilder
                    {
                        Color = Color.Red,
                        Title = e.GetType().ToString(),
                        Description = e.Message
                    }.Build(), ephemeral: true);
                    _pendingRequests.Remove(arg.User.Id);
                }
                else if (arg.Data.CustomId.StartsWith("tags-") && StaticObjects.Tags.ContainsTag(arg.Data.CustomId))
                {
                    await arg.DeferLoadingAsync();
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Booru.GetTagsAsync(ctx, arg.Data.CustomId);
                            _pendingRequests.Remove(arg.User.Id);
                        }
                        catch (System.Exception ex)
                        {
                            await Log.LogErrorAsync(ex, ctx);
                            _pendingRequests.Remove(arg.User.Id);
                        }
                    });
                }
                else if (arg.Data.CustomId.StartsWith("ehentai-") && StaticObjects.EHentai.Contains(arg.Data.CustomId))
                {
                    StaticObjects.EHentai.Remove(arg.Data.CustomId);
                    await arg.DeferLoadingAsync();
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var id = arg.Data.CustomId.Split('/');
                            await Cosplay.DownloadCosplayAsync(ctx, id[1], id[2]);
                            _pendingRequests.Remove(arg.User.Id);
                        }
                        catch (System.Exception ex)
                        {
                            await Log.LogErrorAsync(ex, ctx);
                            _pendingRequests.Remove(arg.User.Id);
                        }
                    });
                }
                /*else if (arg.Data.CustomId.StartsWith("doujinshi-") && StaticObjects.Doujinshis.Contains(arg.Data.CustomId))
                {
                    StaticObjects.Doujinshis.Remove(arg.Data.CustomId);
                    await arg.DeferLoadingAsync();
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var id = arg.Data.CustomId.Split('/').Last();
                            await Doujinshi.DownloadDoujinshiAsync(ctx, id);
                            _pendingRequests.Remove(arg.User.Id);
                        }
                        catch (System.Exception ex)
                        {
                            await Log.LogErrorAsync(ex, ctx);
                            _pendingRequests.Remove(arg.User.Id);
                        }
                    });
                }*/
                else if (arg.Data.CustomId.StartsWith("replay/"))
                {
                    var id = arg.Data.CustomId[7..];
                    var chanId = ctx.Channel.Id.ToString();
                    switch (id)
                    {
                        case "ready":
                            var rLobby = StaticObjects.GameManager.GetReplayLobby(ctx.Channel);
                            if (rLobby == null)
                            {
                                await ctx.ReplyAsync("This lobby expired, please create a new one with the play command", ephemeral: true);
                            }
                            else
                            {
                                var embed = StaticObjects.GameManager.ToggleReadyLobby(rLobby, ctx.User);
                                if (embed != null)
                                {
                                    await ctx.ReplyAsync("Ready state changed", ephemeral: true);
                                    if (await StaticObjects.GameManager.CheckRestartLobbyFullAsync(ctx))
                                    {
                                        await arg.Message.DeleteAsync();
                                    }
                                    else
                                    {
                                        await arg.Message.ModifyAsync(x => x.Embed = embed);
                                    }
                                }
                                else
                                {
                                    await ctx.ReplyAsync("You are not in the lobby", ephemeral: true); // TODO
                                }
                            }
                            break;

                        case "delete":
                            StaticObjects.GameManager.DeleteReadyLobby(ctx.Channel);
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
                    var lobby = StaticObjects.GameManager.GetLobby(chanId);
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
                                    await StaticObjects.GameManager.StartGameAsync(ctx);
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
                                    await arg.Message.ModifyAsync(x => x.Embed = lobby.GetIntroEmbed());
                                }
                                break;

                            case "multi":
                                if (lobby.IsHost(ctx.User))
                                {
                                    lobby.ToggleMultiplayerMode();
                                    await ctx.ReplyAsync("You changed the multiplayer type", ephemeral: true);
                                    await arg.Message.ModifyAsync(x => x.Embed = lobby.GetIntroEmbed());
                                }
                                else
                                {
                                    await ctx.ReplyAsync("Only the host can do that", ephemeral: true);
                                }
                                break;

                            case "cancel":
                                if (lobby.IsHost(ctx.User))
                                {
                                    StaticObjects.GameManager.RemoveLobby(chanId);
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
            await LaunchCommandAsync(cmd, arg.User, arg.Channel as ITextChannel, true, async (string content, bool ephemeral) =>
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
                if (cmd.NeedDefer)
                {
                    await arg.DeferAsync();
                }
                return new SlashCommandContext(arg);
            });
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
                            if ((c.Precondition & Precondition.OwnerOnly) == 0)
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
            await StaticObjects.Client.SetActivityAsync(new Discord.Game("Prefix are gone, please use slash commands!", ActivityType.Watching));
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
            if (arg.Author.IsBot || arg is not SocketUserMessage msg || msg.Content == null || msg.Content == "") return; // The message received isn't one we can deal with

            // Deprecation warning
            int pos = 0;
            if (msg.HasMentionPrefix(StaticObjects.Client.CurrentUser, ref pos))
            {
                var content = msg.Content[pos..];
                var commandStr = content.Split(' ')[0].ToUpperInvariant();
                Module.Command.CommandInfo? cmd = null;
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
                    await LaunchCommandAsync(cmd, msg.Author, msg.Channel as ITextChannel, false, async (string content, bool ephemeral) =>
                    {
                        await msg.ReplyAsync(content);
                    }, async () =>
                    {
                        var newContent = content[commandStr.Length..].TrimStart();
                        if (msg.Attachments.Any())
                        {
                            newContent += " " + msg.Attachments.ElementAt(0).Url;
                        }
                        return new MessageCommandContext(msg, newContent, cmd);
                    });
                }
            }
            else if (!msg.Content.StartsWith("//") && !msg.Content.StartsWith("#"))
            {
                var context = new GameCommandContext(msg);
                var game = StaticObjects.GameManager.GetGame(msg.Channel);
                if (game != null && game.CanPlay(msg.Author))
                {
                    game.AddAnswer(context);
                }
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
