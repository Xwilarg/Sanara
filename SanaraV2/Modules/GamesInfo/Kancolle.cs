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
using SanaraV2.Modules.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Modules.GamesInfo
{
    [Group("Kancolle"), Alias("Kantai Collection", "KantaiCollection")]
    public class Kancolle : ModuleBase
    {
        Program p = Program.p;

        [Command("Drop", RunMode = RunMode.Async), Summary("Get informations about a drop")]
        public async Task Drop(params string[] shipNameArr)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
            Task<EmbedFieldBuilder> constructionTask = Task.Run(() => GetDropConstructionField(shipNameArr, Context.Guild.Id));
            Task<EmbedFieldBuilder> mapTask = Task.Run(() => GetDropMapField(shipNameArr, Context.Guild.Id));
            string name = string.Join("", shipNameArr);
            EmbedBuilder embed = new EmbedBuilder();
            try
            {
                embed.AddField(await mapTask);
                embed.AddField(await constructionTask);
                embed.Title = char.ToUpper(name[0]) + name.Substring(1);
                embed.Color = Color.Blue;
                await ReplyAsync("", false, embed.Build());
            }
            catch (ArgumentException ae)
            {
                await ReplyAsync(ae.Message);
            }
        }

        private async Task<EmbedFieldBuilder> GetDropConstructionField(string[] shipNameArr, ulong guildId)
        {
            string embedMsg = null;
            var result = await Features.GamesInfo.Kancolle.SearchDropConstruction(shipNameArr);
            switch (result.error)
            {
                case Features.GamesInfo.Error.Drop.Help:
                    throw new ArgumentException(Sentences.KancolleHelp(guildId));

                case Features.GamesInfo.Error.Drop.NotFound:
                    throw new ArgumentException(Sentences.ShipgirlDontExist(guildId));

                case Features.GamesInfo.Error.Drop.NotReferenced:
                    embedMsg = Sentences.ShipNotReferencedConstruction(Context.Guild.Id);
                    break;

                case Features.GamesInfo.Error.Drop.DontDrop:
                    embedMsg = Sentences.DontDropOnMaps(Context.Guild.Id);
                    break;

                case Features.GamesInfo.Error.Drop.None:
                    int i = 1;
                    foreach (var elem in result.answer.elems)
                    {
                        embedMsg += "**" + elem.chance + "%:** "
                            + Sentences.Fuel(guildId) + " " + elem.fuel + ", "
                            + Sentences.Ammos(guildId) + " " + elem.ammos + ", "
                            + Sentences.Iron(guildId) + " " + elem.iron + ", "
                            + Sentences.Bauxite(guildId) + " " + elem.bauxite + ", "
                            + Sentences.DevMat(guildId) + " " + elem.devMat
                            + Environment.NewLine;
                        i++;
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
            return (new EmbedFieldBuilder()
            {
                Name = "Construction drop",
                Value = embedMsg
            });
        }

        private async Task<EmbedFieldBuilder> GetDropMapField(string[] shipNameArr, ulong guildId)
        {
            string embedMsg = null;
            var result = await Features.GamesInfo.Kancolle.SearchDropMap(shipNameArr);
            switch (result.error)
            {
                case Features.GamesInfo.Error.Drop.Help:
                    throw new ArgumentException(Sentences.KancolleHelp(guildId));

                case Features.GamesInfo.Error.Drop.NotFound:
                    throw new ArgumentException(Sentences.ShipgirlDontExist(guildId));

                case Features.GamesInfo.Error.Drop.NotReferenced:
                    embedMsg = Sentences.ShipNotReferencedMap(Context.Guild.Id);
                    break;

                case Features.GamesInfo.Error.Drop.DontDrop:
                    embedMsg = Sentences.DontDropOnMaps(Context.Guild.Id);
                    break;

                case Features.GamesInfo.Error.Drop.None:
                    foreach (var k in result.answer.dropMap)
                    {
                        embedMsg += "**" + k.Key + ":** ";
                        switch (k.Value)
                        {
                            case Features.GamesInfo.Response.DropMapLocation.NormalOnly:
                                embedMsg += Sentences.OnlyNormalNodes(Context.Guild.Id);
                                break;

                            case Features.GamesInfo.Response.DropMapLocation.BossOnly:
                                embedMsg += Sentences.OnlyBossNode(Context.Guild.Id);
                                break;

                            case Features.GamesInfo.Response.DropMapLocation.Anywhere:
                                embedMsg += Sentences.AnyNode(Context.Guild.Id);
                                break;
                        }
                        embedMsg += Environment.NewLine;
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
            return (new EmbedFieldBuilder()
            {
                Name = "Map drop" + ((result.answer != null && result.answer.rarity != null) ? (Sentences.Rarity(guildId)    + ": " + result.answer.rarity.ToString() + " / 7") : ("")),
                Value = embedMsg
            });
        }

        [Command("", RunMode = RunMode.Async), Priority(-1)]
        public async Task CharacDefault(params string[] shipNameArr) => await Charac(shipNameArr);

        [Command("Charac", RunMode = RunMode.Async), Summary("Get informations about a Kancolle character")]
        public async Task Charac(params string[] shipNameArr)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
            var result = await Features.GamesInfo.Kancolle.SearchCharac(shipNameArr);
            switch (result.error)
            {
                case Features.GamesInfo.Error.Charac.Help:
                    await ReplyAsync(Sentences.KancolleHelp(Context.Guild.Id));
                    break;

                case Features.GamesInfo.Error.Charac.NotFound:
                    await ReplyAsync(Sentences.ShipgirlDontExist(Context.Guild.Id));
                    break;

                case Features.GamesInfo.Error.Charac.None:
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = Color.Blue,
                        Title = result.answer.name,
                        ImageUrl = result.answer.thumbnailUrl
                    };
                    foreach (Tuple<string, string> s in result.answer.allCategories)
                        embed.AddField(s.Item1, s.Item2);
                    await ReplyAsync("", false, embed.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}