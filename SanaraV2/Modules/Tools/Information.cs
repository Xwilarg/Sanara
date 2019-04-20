/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DynamicExpresso;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Tools
{
    public class Information : ModuleBase
    {
        Program p = Program.p;

        private struct Eval
        {
            public DiscordSocketClient Client { set; get; }
            public ICommandContext Context { set; get; }
        }

        [Command("Eval")]
        public async Task EvalFct(params string[] args)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Information);
            if (Context.User.Id != Base.Sentences.ownerId)
                await ReplyAsync(Base.Sentences.OnlyMasterStr(Context.Guild.Id));
            else if (args.Length == 0)
                await ReplyAsync(Sentences.EvalHelp(Context.Guild.Id));
            else
            {
                Interpreter interpreter = new Interpreter()
                    .SetVariable("Context", Context)
                    .SetVariable("p", Program.p)
                    .EnableReflection();
                try
                {
                    await ReplyAsync(interpreter.Eval(string.Join(" ", args)).ToString());
                }
                catch (Exception e)
                {
                    await ReplyAsync(Sentences.EvalError(Context.Guild.Id, e.Message));
                }
            }
        }

        [Command("Help"), Summary("Give the help"), Alias("Commands")]
        public async Task Help()
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Information);
            await ReplyAsync("", false, Sentences.Help(Context.Guild.Id, (Context.Channel as ITextChannel).IsNsfw, Context.User.Id == Base.Sentences.ownerId));
        }

        [Command("GDPR"), Summary("Show infos the bot have about the user and the guild")]
        public async Task GDPR(params string[] command)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Information);
            await ReplyAsync("", false, new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "Datas saved about " + Context.Guild.Name,
                Description = await Program.p.db.GetGuild(Context.Guild.Id)
            }.Build());
        }

        [Command("Status"), Summary("Display which commands aren't available because of missing files")]
        public async Task Status()
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Information);
            int yes = 0;
            int no = 0;
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = "Services availability"
            };
            string description = "";
            for (Program.Module i = 0; i < Program.Module.Youtube; i++)
                description += "**" + i.ToString() + "**: " + ((Program.p.db.IsAvailable(Context.Guild.Id, i)) ? (Sentences.Enabled(Context.Guild.Id)) : (Sentences.Disabled(Context.Guild.Id))) + Environment.NewLine;
            embed.Description = description;
            if (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Radio))
            {
                embed.AddField("Radio Module",
                    "**Opus dll:** " + ((File.Exists("opus.dll") ? ("Yes") : ("No"))) + Environment.NewLine +
                    "**Lib Sodium dll:** " + ((File.Exists("libsodium.dll") ? ("Yes") : ("No"))) + Environment.NewLine +
                    "**Ffmpeg:** " + ((File.Exists("ffmpeg.exe") ? ("Yes") : ("No"))) + Environment.NewLine +
                    "**youtube-dl:** " + ((File.Exists("youtube-dl.exe") ? ("Yes") : ("No"))) + Environment.NewLine +
                    "**YouTube API key:** " + ((p.youtubeService != null ? ("Yes") : ("No"))));
                if (File.Exists("opus.dll") && File.Exists("libsodium.dll") && File.Exists("ffmpeg.exe") && p.youtubeService != null)
                    yes++;
                else
                    no++;
            }
            if (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Game))
            {
                embed.AddField("Game Module",
                "**Shiritori:** " + ((Program.p.shiritoriDict == null) ? ("Not loaded") : (Program.p.shiritoriDict.Count + " words")) + Environment.NewLine +
                "**Booru quizz:** " + ((Program.p.booruDict == null) ? ("Not loaded") : (Program.p.booruDict.Count + " tags")) + Environment.NewLine +
                "**Anime quizz:** " + ((Program.p.animeDict == null) ? ("Not loaded") : (Program.p.animeDict.Count + " anime names")) + Environment.NewLine +
                "**KanColle quizz :** " + ((Program.p.kancolleDict == null) ? ("Not loaded") : (Program.p.kancolleDict.Count + " shipgirl names")));
                //"**Fire Emblem quizz:** " + ((Program.p.fireEmblemDict == null) ? ("Not loaded") : (Program.p.fireEmblemDict.Count + " character names")));
                if (Program.p.shiritoriDict != null)
                    yes++;
                else
                    no++;
                if (Program.p.booruDict != null)
                    yes++;
                else
                    no++;
                if (Program.p.animeDict != null)
                    yes++;
                else
                    no++;
                if (Program.p.kancolleDict != null)
                    yes++;
                else
                    no++;
                /*if (Program.p.fireEmblemDict != null)
                    yes++;
                else
                    no++;*/
            }
            if (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Linguistic))
            {
                embed.AddField("Linguistic Module - Translations",
                "**Google Translate API key:** " + ((p.translationClient != null ? ("Yes") : ("No"))) + Environment.NewLine +
                "**Google Vision API key:** " + ((p.visionClient != null ? ("Yes") : ("No"))));
                if (p.translationClient != null)
                {
                    yes++;
                    if (p.visionClient != null)
                        yes++;
                    else
                        no++;
                }
                else
                    no += 2;
            }
            if (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Youtube))
            {
                embed.AddField("YouTube Module", "**YouTube API key:** " + ((p.youtubeService != null) ? ("Yes") : ("No")));
                if (p.youtubeService != null)
                    yes++;
                else
                    no++;
            }
            if (yes + no == 0)
                yes++;
            int max = yes + no;
            embed.Color = new Color(no * 255 / max, yes * 255 / max, 0);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("Invite", RunMode = RunMode.Async), Summary("Get invitation link")]
        public async Task Invite()
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Information);
            await ReplyAsync("<https://discordapp.com/oauth2/authorize?client_id=329664361016721408&permissions=3196928&scope=bot>");
        }
    }
}