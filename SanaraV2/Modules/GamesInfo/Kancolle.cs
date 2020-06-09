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
            Utilities.CheckAvailability(Context.Guild, Program.Module.Kancolle);
            await p.DoAction(Context.User, Program.Module.Kancolle);
            Task<EmbedFieldBuilder> constructionTask = Task.Run(() => GetDropConstructionField(shipNameArr));
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

        private async Task<EmbedFieldBuilder> GetDropConstructionField(string[] shipNameArr)
        {
            string embedMsg = null;
            var result = await Features.GamesInfo.Kancolle.SearchDropConstruction(shipNameArr);
            switch (result.error)
            {
                case Features.GamesInfo.Error.Drop.Help:
                    throw new ArgumentException(Sentences.KancolleHelp(Context.Guild));

                case Features.GamesInfo.Error.Drop.NotFound:
                    throw new ArgumentException(Sentences.ShipgirlDontExist(Context.Guild));

                case Features.GamesInfo.Error.Drop.NotReferenced:
                    embedMsg = Sentences.ShipNotReferencedConstruction(Context.Guild);
                    break;

                case Features.GamesInfo.Error.Drop.DontDrop:
                    embedMsg = Sentences.DontDropOnMaps(Context.Guild);
                    break;

                case Features.GamesInfo.Error.Drop.None:
                    int i = 1;
                    foreach (var elem in result.answer.elems)
                    {
                        embedMsg += "**" + elem.chance + "%:** "
                            + Sentences.Fuel(Context.Guild) + " " + elem.fuel + ", "
                            + Sentences.Ammos(Context.Guild) + " " + elem.ammos + ", "
                            + Sentences.Iron(Context.Guild) + " " + elem.iron + ", "
                            + Sentences.Bauxite(Context.Guild) + " " + elem.bauxite + ", "
                            + Sentences.DevMat(Context.Guild) + " " + elem.devMat
                            + Environment.NewLine;
                        i++;
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
            return (new EmbedFieldBuilder()
            {
                Name = Sentences.ConstructionDrop(Context.Guild),
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
                    throw new ArgumentException(Sentences.KancolleHelp(Context.Guild));

                case Features.GamesInfo.Error.Drop.NotFound:
                    throw new ArgumentException(Sentences.ShipgirlDontExist(Context.Guild));

                case Features.GamesInfo.Error.Drop.NotReferenced:
                    embedMsg = Sentences.ShipNotReferencedMap(Context.Guild);
                    break;

                case Features.GamesInfo.Error.Drop.DontDrop:
                    embedMsg = Sentences.DontDropOnMaps(Context.Guild);
                    break;

                case Features.GamesInfo.Error.Drop.None:
                    foreach (var k in result.answer.dropMap)
                    {
                        embedMsg += "**" + k.Key + ":** ";
                        switch (k.Value)
                        {
                            case Features.GamesInfo.Response.DropMapLocation.NormalOnly:
                                embedMsg += Sentences.OnlyNormalNodes(Context.Guild);
                                break;

                            case Features.GamesInfo.Response.DropMapLocation.BossOnly:
                                embedMsg += Sentences.OnlyBossNode(Context.Guild);
                                break;

                            case Features.GamesInfo.Response.DropMapLocation.Anywhere:
                                embedMsg += Sentences.AnyNode(Context.Guild);
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
                Name = Sentences.MapDrop(Context.Guild) + ((result.answer != null && result.answer.rarity != null) ? (Sentences.Rarity(Context.Guild)    + ": " + result.answer.rarity.ToString() + " / 7") : ("")),
                Value = embedMsg
            });
        }

        [Command("", RunMode = RunMode.Async), Priority(-1)]
        public async Task CharacDefault(params string[] shipNameArr) => await Charac(shipNameArr);

        [Command("Charac", RunMode = RunMode.Async), Summary("Get informations about a Kancolle character")]
        public async Task Charac(params string[] shipNameArr)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Kancolle);
            await p.DoAction(Context.User, Program.Module.Kancolle);
            var result = await Features.GamesInfo.Kancolle.SearchCharac(shipNameArr);
            switch (result.error)
            {
                case Features.GamesInfo.Error.Charac.Help:
                    await ReplyAsync(Sentences.KancolleHelp(Context.Guild));
                    break;

                case Features.GamesInfo.Error.Charac.NotFound:
                    await ReplyAsync(Sentences.ShipgirlDontExist(Context.Guild));
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