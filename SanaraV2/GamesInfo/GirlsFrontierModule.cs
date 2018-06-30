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
using SanaraV2.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2.GamesInfo
{
    [Group("Girls Frontline"), Alias("Girlsfrontline")]
    public class GirlsFrontierModule : ModuleBase
    {
        Program p = Program.p;

        [Command("", RunMode = RunMode.Async), Priority(-1)]
        public async Task CharacDefault(params string[] shipNameArr) => await Charac(shipNameArr);

        [Command("Charac", RunMode = RunMode.Async), Summary("Get informations about a Girls Frontline character (trivia)")]
        public async Task Charac(params string[] shipNameArr) // TODO Refactor (duplicated of Kancolle command)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.GirlsFrontier);
            if (shipNameArr.Length == 0)
            {
                await ReplyAsync(Sentences.GirlsFrontlineHelp(Context.Guild.Id));
                return;
            }
            string shipName = Utilities.AddArgs(shipNameArr);
            Wikia.CharacInfo? infos = Wikia.GetCharacInfos(shipName, Wikia.WikiaType.GirlsFrontline);
            if (infos == null)
            {
                await ReplyAsync(Sentences.TDollDontExist(Context.Guild.Id));
                return;
            }
            string thumbnailPath = null;
            List<string> finalStr = FillGirlsFrontlineInfos(infos.Value.id, Context.Guild.Id);
            IGuildUser me = await Context.Guild.GetUserAsync(Base.Sentences.myId);
            if (me.GuildPermissions.AttachFiles)
            {
                thumbnailPath = Wikia.DownloadCharacThumbnail(infos.Value.thumbnail);
                await Context.Channel.SendFileAsync(thumbnailPath);
            }
            foreach (string s in finalStr)
                await ReplyAsync(s);
            if (me.GuildPermissions.AttachFiles)
                File.Delete(thumbnailPath);
        }

        [Command("Infos", RunMode = RunMode.Async), Summary("Get informations about a Girls Frontline character (stats)")]
        public async Task Infos(params string[] nameArr)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.GirlsFrontier);
            if (nameArr.Length == 0)
            {
                await ReplyAsync(Sentences.GirlsFrontlineHelp(Context.Guild.Id));
                return;
            }
            Wikia.CharacInfo? infos = Wikia.GetCharacInfos(Utilities.AddArgs(nameArr), Wikia.WikiaType.GirlsFrontline);
            if (infos == null)
            {
                await ReplyAsync(Sentences.TDollDontExist(Context.Guild.Id));
                return;
            }
            IGuildUser me = await Context.Guild.GetUserAsync(Base.Sentences.myId);

            List<string> jsonInfos = infos.Value.infos.Split('|').ToList();
            string finalStr = "";
            Color color;
            string rarity = FindElement(jsonInfos, "rarity =");
            int rarityValue = rarity.Count(delegate (char c) { return (c == '★'); });
            if (rarityValue == 2) color = new Color(255, 255, 255);
            else if (rarityValue == 3) color = new Color(173, 216, 230);
            else if (rarityValue == 4) color = new Color(144, 238, 144);
            else if (rarityValue == 5) color = Color.Orange;
            else color = new Color(0, 0, 0);
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = infos.Value.name,
                Color = color,
                ImageUrl = infos.Value.thumbnail
            };
            embed.AddField(Sentences.ClassStr(Context.Guild.Id), FindElement(jsonInfos, "firearmclass ="), true);
            embed.AddField(Sentences.Rarity(Context.Guild.Id), rarity, true);
            embed.AddField(Sentences.ManufTime(Context.Guild.Id), FindElement(jsonInfos, "manuftimer ="), true);
            embed.AddField(Sentences.GrowthGrade(Context.Guild.Id), FindElement(jsonInfos, "growthgrade ="), true);
            embed.AddField(Sentences.OperationalEffectiveness(Context.Guild.Id), FindElement(jsonInfos, "baseopef =") + " → " + FindElement(jsonInfos, "maxopef ="), true);
            embed.AddField(Sentences.Health(Context.Guild.Id), "**" + FindElement(jsonInfos, "hpgrade =") + ":** " + FindElement(jsonInfos, "basehp =") + " → " + FindElement(jsonInfos, "maxhp ="), true);
            embed.AddField(Sentences.Evasion(Context.Guild.Id), "**" + FindElement(jsonInfos, "evagrade =") + ":** " + FindElement(jsonInfos, "baseeva =") + " → " + FindElement(jsonInfos, "maxeva ="), true);
            embed.AddField(Sentences.Speed(Context.Guild.Id), "**" + FindElement(jsonInfos, "movespeedgrade =") + ":** " + FindElement(jsonInfos, "movespeed ="), true);
            embed.AddField(Sentences.Damage(Context.Guild.Id), "**" + FindElement(jsonInfos, "dmggrade =") + ":** " + FindElement(jsonInfos, "basedmg =") + " → " + FindElement(jsonInfos, "maxdmg ="), true);
            embed.AddField(Sentences.Accuracy(Context.Guild.Id), "**" + FindElement(jsonInfos, "accgrade =") + ":** " + FindElement(jsonInfos, "baseacc =") + " → " + FindElement(jsonInfos, "maxacc ="), true);
            embed.AddField(Sentences.RateOfFire(Context.Guild.Id), "**" + FindElement(jsonInfos, "rofgrade =") + ":** " + FindElement(jsonInfos, "baserof =") + " → " + FindElement(jsonInfos, "maxrof ="), true);
            embed.AddField(Sentences.AmmoConsumption(Context.Guild.Id), FindElement(jsonInfos, "baseammoconsump =") + " → " + FindElement(jsonInfos, "maxammoconsump ="), true);
            embed.AddField(Sentences.RationConsumption(Context.Guild.Id), FindElement(jsonInfos, "baserationconsump =") + " → " + FindElement(jsonInfos, "maxrationconsump ="), true);
            string charaIntro = FindElement(jsonInfos, "charaintro =");
            if (charaIntro == null)
                charaIntro = FindElement(jsonInfos, "engain =");
            if (charaIntro == null)
                charaIntro = FindElement(jsonInfos, "ensecretary1 =");
            embed.Footer = new EmbedFooterBuilder()
            {
                Text = charaIntro
            };
            await ReplyAsync("", false, embed.Build());
        }

        private static string FindElement(List<string> infos, string toFind)
        {
            string elem = infos.Find(x => x.StartsWith(toFind));
            return ((elem == null) ? (null) : (elem.Split('=')[1].Replace("\n", "")));
        }

        public static List<string> FillGirlsFrontlineInfos(string shipId, ulong guildId)
        {
            return (Wikia.FillWikiaInfos(shipId, guildId, new Dictionary<string, Func<ulong, string>>
            {
                { "Model & Historical Information", Sentences.ModelAndHistoricalInformation },
                { "Basic", Sentences.Basic },
                { "Technical", Sentences.Technical }
            }, Wikia.WikiaType.GirlsFrontline));
        }
    }
}