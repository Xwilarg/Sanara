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
using System.Globalization;
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

        public struct Intervalle
        {
            public Intervalle(int min, int? max = null, char? grade = null)
            {
                this.min = min;
                this.max = max;
                this.grade = grade;
            }

            public int min;
            public int? max;
            public char? grade;
        }

        public struct TDollInfos
        {
            public TDollInfos(List<string> jsonInfos)
            {
                firearmClass = FindElement(jsonInfos, "firearmclass =");
                rarity = FindElement(jsonInfos, "rarity =");
                manuTime = DateTime.ParseExact(FindElement(jsonInfos, "manuftimer ="), "HH:mm:ss", CultureInfo.CurrentCulture);
                growthGrade = FindElement(jsonInfos, "growthgrade =")[0];
                operationalEffectiveness = new Intervalle(Convert.ToInt32(FindElement(jsonInfos, "baseopef =")), Convert.ToInt32(FindElement(jsonInfos, "maxopef =")));
                health = new Intervalle(Convert.ToInt32(FindElement(jsonInfos, "basehp =")), Convert.ToInt32(FindElement(jsonInfos, "maxhp =")), FindElement(jsonInfos, "hpgrade =")[0]);
                evasion = new Intervalle(Convert.ToInt32(FindElement(jsonInfos, "baseeca =")), Convert.ToInt32(FindElement(jsonInfos, "maxeva =")), FindElement(jsonInfos, "evagrade =")[0]);
                speed = new Intervalle(Convert.ToInt32(FindElement(jsonInfos, "movespeed =")), null, FindElement(jsonInfos, "movespeedgrade =")[0]);
                damage = new Intervalle(Convert.ToInt32(FindElement(jsonInfos, "basedmg =")), Convert.ToInt32(FindElement(jsonInfos, "maxdmg =")), FindElement(jsonInfos, "dmggrade =")[0]);
                accuracy = new Intervalle(Convert.ToInt32(FindElement(jsonInfos, "baseacc =")), Convert.ToInt32(FindElement(jsonInfos, "maxacc =")), FindElement(jsonInfos, "accgrade =")[0]);
                rateOfFire = new Intervalle(Convert.ToInt32(FindElement(jsonInfos, "baserof =")), Convert.ToInt32(FindElement(jsonInfos, "maxrof =")), FindElement(jsonInfos, "rofgrade =")[0]);
                ammoConsumption = new Intervalle(Convert.ToInt32(FindElement(jsonInfos, "baseammoconsump =")), Convert.ToInt32(FindElement(jsonInfos, "maxammoconsump =")));
                rationConsumption = new Intervalle(Convert.ToInt32(FindElement(jsonInfos, "baserationconsump =")), Convert.ToInt32(FindElement(jsonInfos, "maxrationconsump =")));
                intro = FindElement(jsonInfos, "charaintro =");
                if (intro == null)
                    intro = FindElement(jsonInfos, "engain =");
                if (intro == null)
                    intro = FindElement(jsonInfos, "ensecretary1 =");
            }

            public string firearmClass;
            public string rarity;
            public DateTime manuTime;
            public char growthGrade;
            public Intervalle operationalEffectiveness;
            public Intervalle health;
            public Intervalle evasion;
            public Intervalle speed;
            public Intervalle damage;
            public Intervalle accuracy;
            public Intervalle rateOfFire;
            public Intervalle ammoConsumption;
            public Intervalle rationConsumption;
            public string intro;
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
            TDollInfos jsonInfos = new TDollInfos(infos.Value.infos.Split('|').ToList());
            Color color;
            int rarityValue = jsonInfos.rarity.Count(delegate (char c) { return (c == '★'); });
            if (rarityValue == 2) color = new Color(255, 255, 255);
            else if (rarityValue == 3) color = new Color(173, 216, 230);
            else if (rarityValue == 4) color = new Color(144, 238, 144);
            else if (rarityValue == 5) color = new Color(255, 255, 0);
            else color = new Color(0, 0, 0);
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = infos.Value.name,
                Color = color,
                ImageUrl = infos.Value.thumbnail
            };
            Console.WriteLine(infos.Value.thumbnail);
            embed.AddField(Sentences.ClassStr(Context.Guild.Id), jsonInfos.firearmClass, true);
            embed.AddField(Sentences.Rarity(Context.Guild.Id), jsonInfos.rarity, true);
            embed.AddField(Sentences.ManufTime(Context.Guild.Id), jsonInfos.manuTime.ToString("HH:mm:ss"), true);
            embed.AddField(Sentences.GrowthGrade(Context.Guild.Id), jsonInfos.growthGrade, true);
            embed.AddField(Sentences.OperationalEffectiveness(Context.Guild.Id), jsonInfos.operationalEffectiveness.min + " → " + jsonInfos.operationalEffectiveness.max, true);
            embed.AddField(Sentences.Health(Context.Guild.Id), "**" + jsonInfos.health.grade + ":** " + jsonInfos.health.min + " → " + jsonInfos.health.max, true);
            embed.AddField(Sentences.Evasion(Context.Guild.Id), "**" + jsonInfos.evasion.grade + ":** " + jsonInfos.evasion.min + " → " + jsonInfos.evasion.max, true);
            embed.AddField(Sentences.Speed(Context.Guild.Id), "**" + jsonInfos.speed.grade + ":** " + jsonInfos.speed.min, true);
            embed.AddField(Sentences.Damage(Context.Guild.Id), "**" + jsonInfos.damage.grade + ":** " + jsonInfos.damage.min + " → " + jsonInfos.damage.max, true);
            embed.AddField(Sentences.Accuracy(Context.Guild.Id), "**" + jsonInfos.accuracy.grade + ":** " + jsonInfos.accuracy.min + " → " + jsonInfos.accuracy.max, true);
            embed.AddField(Sentences.RateOfFire(Context.Guild.Id), "**" + jsonInfos.rateOfFire.grade + ":** " + jsonInfos.rateOfFire.min + " → " + jsonInfos.rateOfFire.max, true);
            embed.AddField(Sentences.AmmoConsumption(Context.Guild.Id), jsonInfos.ammoConsumption.min + " → " + jsonInfos.ammoConsumption.max, true);
            embed.AddField(Sentences.RationConsumption(Context.Guild.Id), jsonInfos.rationConsumption.min + " → " + jsonInfos.rationConsumption.max, true);
            embed.Footer = new EmbedFooterBuilder()
            {
                Text = jsonInfos.intro
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("Compare", RunMode = RunMode.Async), Summary("Compare two T-Dolls")]
        public async Task Compare(params string[] nameArr)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.GirlsFrontier);
            if (nameArr.Length <= 1)
            {
                await ReplyAsync(Sentences.GirlsFrontlineHelp(Context.Guild.Id));
                return;
            }
            Wikia.CharacInfo? c1 = Wikia.GetCharacInfos(nameArr[0], Wikia.WikiaType.GirlsFrontline);
            Wikia.CharacInfo? c2 = Wikia.GetCharacInfos(Utilities.AddArgs(nameArr.Skip(1).ToArray()), Wikia.WikiaType.GirlsFrontline);
            if (c1 == null || c2 == null)
            {
                await ReplyAsync(Sentences.TDollDontExist(Context.Guild.Id));
                return;
            }
            TDollInfos infosC1 = new TDollInfos(c1.Value.infos.Split('|').ToList());
            TDollInfos infosC2 = new TDollInfos(c2.Value.infos.Split('|').ToList());
            await ReplyAsync("```" + Environment.NewLine +
                AddPadding("", 21) + "┌" + AddPadding("", 20, '─') + "┬" + AddPadding("", 20, '─') + "┐" + Environment.NewLine +
                AddPadding("", 21) + "│" + AddPadding(c1.Value.name, 20) + "│" + AddPadding(c2.Value.name, 20) + "│" + Environment.NewLine +
                "┌" + AddPadding("", 20, '─') + "┼" + AddPadding("", 20, '─') + "┼" + AddPadding("", 20, '─') + "┤" + Environment.NewLine +
                "│" + AddPadding(Sentences.ClassStr(Context.Guild.Id), 20) + "│" + AddPadding(infosC1.firearmClass, 20) + "│" + AddPadding(infosC2.firearmClass, 20) + "│" + Environment.NewLine +
                "├" + AddPadding("", 20, '─') + "┼" + AddPadding("", 20, '─') + "┼" + AddPadding("", 20, '─') + "┤" + Environment.NewLine +
                "│" + AddPadding(Sentences.Rarity(Context.Guild.Id), 20) + "│" + AddPadding(infosC1.rarity.Count(delegate (char c) { return (c == '★'); }) + " stars", 20) + "│" + AddPadding(infosC2.rarity.Count(delegate (char c) { return (c == '★'); }) + " stars", 20) + "│" + Environment.NewLine +
                "└" + AddPadding("", 20, '─') + "┴" + AddPadding("", 20, '─') + "┴" + AddPadding("", 20, '─') + "┘" + Environment.NewLine +
                "```");
        }

        private string AddPadding(string str, int maxLenght, char fill = ' ')
        {
            if (str.Length > maxLenght)
                return (str);
            int currLenght = maxLenght - str.Length;
            string finalStr = "";
            for (int i = 0; i < Math.Floor(currLenght / 2.0f); i++)
                finalStr += fill;
            finalStr += str;
            for (int i = 0; i < Math.Ceiling(currLenght / 2.0f); i++)
                finalStr += fill;
            return (finalStr);
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