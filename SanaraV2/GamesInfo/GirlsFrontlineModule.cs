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
using System.Net;
using System.Threading.Tasks;

namespace SanaraV2.GamesInfo
{
    [Group("Girls Frontline"), Alias("Girlsfrontline", "Gf")]
    public class GirlsFrontlineModule : ModuleBase
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
            public TDollInfos(string name, List<string> jsonInfos)
            {
                this.name = name;
                firearmClass = FindElement(jsonInfos, "firearmclass =");
                rarity = FindElement(jsonInfos, "rarity =");
                manuTime = GetHour(FindElement(jsonInfos, "manuftimer ="));
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

            public string name;
            public string firearmClass;
            public string rarity;
            public DateTime? manuTime;
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
            TDollInfos jsonInfos = new TDollInfos(infos.Value.name, infos.Value.infos.Split('|').ToList());
            Color color;
            int rarityValue = jsonInfos.rarity.Count(delegate (char c) { return (c == '★'); });
            if (rarityValue == 2) color = new Color(255, 255, 255);
            else if (rarityValue == 3) color = new Color(173, 216, 230);
            else if (rarityValue == 4) color = new Color(144, 238, 144);
            else if (rarityValue == 5) color = new Color(255, 255, 0);
            else color = new Color(0, 0, 0);
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = jsonInfos.name,
                Color = color,
                ImageUrl = infos.Value.thumbnail
            };
            embed.AddField(Sentences.ClassStr(Context.Guild.Id), jsonInfos.firearmClass, true);
            embed.AddField(Sentences.Rarity(Context.Guild.Id), jsonInfos.rarity, true);
            embed.AddField(Sentences.ManufTime(Context.Guild.Id), (jsonInfos.manuTime == null) ? (Sentences.NoData(Context.Guild.Id)) : (jsonInfos.manuTime.Value.ToString("HH:mm:ss")), true);
            embed.AddField(Sentences.GrowthGrade(Context.Guild.Id), jsonInfos.growthGrade, true);
            embed.AddField(Sentences.OperationalEffectiveness(Context.Guild.Id), jsonInfos.operationalEffectiveness.min + " → " + jsonInfos.operationalEffectiveness.max, true);
            embed.AddField(Sentences.Health(Context.Guild.Id), jsonInfos.health.min + " → " + jsonInfos.health.max + " (" + jsonInfos.health.grade + ")", true);
            embed.AddField(Sentences.Evasion(Context.Guild.Id), jsonInfos.evasion.min + " → " + jsonInfos.evasion.max + " (" + jsonInfos.evasion.grade + ")", true);
            embed.AddField(Sentences.Speed(Context.Guild.Id), jsonInfos.speed.min + " (" + jsonInfos.speed.grade + ")", true);
            embed.AddField(Sentences.Damage(Context.Guild.Id), jsonInfos.damage.min + " → " + jsonInfos.damage.max + " (" + jsonInfos.damage.grade + ")", true);
            embed.AddField(Sentences.Accuracy(Context.Guild.Id), jsonInfos.accuracy.min + " → " + jsonInfos.accuracy.max + " (" + jsonInfos.accuracy.grade + ")", true);
            embed.AddField(Sentences.RateOfFire(Context.Guild.Id), jsonInfos.rateOfFire.min + " → " + jsonInfos.rateOfFire.max + " (" + jsonInfos.rateOfFire.grade + ")", true);
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
                await ReplyAsync(Sentences.GirlsFrontlineCompareHelp(Context.Guild.Id));
                return;
            }
            if (nameArr.Length > 5)
            {
                await ReplyAsync(Sentences.TooManyTDolls(Context.Guild.Id));
                return;
            }
            List<Wikia.CharacInfo> characs = new List<Wikia.CharacInfo>();
            foreach (string s in nameArr)
            {
                Wikia.CharacInfo? c = Wikia.GetCharacInfos(s, Wikia.WikiaType.GirlsFrontline);
                if (c == null)
                {
                    await ReplyAsync(Sentences.TDollDontExistSpecify(Context.Guild.Id, s));
                    return;
                }
                characs.Add(c.Value);
            }
            List<TDollInfos> infos = new List<TDollInfos>();
            infos.AddRange(characs.Select(delegate (Wikia.CharacInfo charac) { return (new TDollInfos(charac.name, charac.infos.Split('|').ToList())); }));
            string[] elems = new string[] {
                "```" + Environment.NewLine +
                AddPadding("", 31) + "┌" + String.Join("┬", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┐" + Environment.NewLine +
                AddPadding("", 31) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.name, 20)); })) + "│" + Environment.NewLine +
                "┌" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤" + Environment.NewLine +
                "│" + AddPadding(Sentences.ClassStr(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.firearmClass, 20)); })) + "│" + Environment.NewLine +
                "├" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤",
                "│" + AddPadding(Sentences.Rarity(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.rarity.Count(delegate (char c) { return (c == '★'); }) + " stars", 20)); })) + "│" + Environment.NewLine +
                "├" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤",
                "│" + AddPadding(Sentences.ManufTime(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding((info.manuTime == null) ? (Sentences.NoData(Context.Guild.Id)) : (info.manuTime.Value.ToString("HH:mm:ss")), 20)); })) + "│" + Environment.NewLine +
                "├" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤",
                "│" + AddPadding(Sentences.GrowthGrade(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.growthGrade.ToString(), 20)); })) + "│" + Environment.NewLine +
                "├" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤",
                "│" + AddPadding(Sentences.OperationalEffectiveness(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.operationalEffectiveness.min + " → " + info.operationalEffectiveness.max, 20)); })) + "│" + Environment.NewLine +
                "├" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤",
                "│" + AddPadding(Sentences.Health(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.health.min + " → " + info.health.max + " (" + info.health.grade + ")", 20)); })) + "│" + Environment.NewLine +
                "├" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤",
                "│" + AddPadding(Sentences.Evasion(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.evasion.min + " → " + info.evasion.max + " (" + info.evasion.grade + ")", 20)); })) + "│" + Environment.NewLine +
                "├" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤",
                "│" + AddPadding(Sentences.Speed(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.speed.min + " (" + info.speed.grade + ")", 20)); })) + "│" + Environment.NewLine +
                "├" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤",
                "│" + AddPadding(Sentences.Damage(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.damage.min + " → " + info.damage.max + " (" + info.damage.grade + ")", 20)); })) + "│" + Environment.NewLine +
                "├" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤",
                "│" + AddPadding(Sentences.Accuracy(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.accuracy.min + " → " + info.accuracy.max + " (" + info.accuracy.grade + ")", 20)); })) + "│" + Environment.NewLine +
                "├" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤",
                "│" + AddPadding(Sentences.RateOfFire(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.rateOfFire.min + " → " + info.rateOfFire.max + " (" + info.rateOfFire.grade + ")", 20)); })) + "│" + Environment.NewLine +
                "├" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤",
                "│" + AddPadding(Sentences.AmmoConsumption(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.ammoConsumption.min + " → " + info.ammoConsumption.max, 20)); })) + "│" + Environment.NewLine +
                "├" + AddPadding("", 30, '─') + "┼" + String.Join("┼", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┤",
                "│" + AddPadding(Sentences.RationConsumption(Context.Guild.Id), 30) + "│" + String.Join("│", infos.Select(delegate (TDollInfos info) { return (AddPadding(info.rationConsumption.min + " → " + info.rationConsumption.max, 20)); })) + "│" + Environment.NewLine +
                "└" + AddPadding("", 30, '─') + "┴" + String.Join("┴", infos.Select(delegate (TDollInfos info) { return (AddPadding("", 20, '─')); })) + "┘"
            };
            string finalStr = "";
            foreach (string s in elems)
            {
                finalStr += s + Environment.NewLine;
                if (finalStr.Length > 1500)
                {
                    await ReplyAsync(finalStr + Environment.NewLine + "```");
                    finalStr = "```";
                }
            }
            await ReplyAsync(finalStr + Environment.NewLine + "```");
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

        private static DateTime? GetHour(string str)
        {
            DateTime dt;
            if (DateTime.TryParseExact(str, "HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
                return (dt);
            if (DateTime.TryParseExact(str, "H:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
                return (dt);
            return (null);
        }

        [Command("Hours", RunMode = RunMode.Async), Summary("Compare two T-Dolls")]
        public async Task Hours(params string[] nameArr)
        {
            DateTime? objDt = null;
            if (nameArr.Length > 0)
            {
                objDt = GetHour(Utilities.AddArgs(nameArr));
                if (objDt == null)
                {
                    await ReplyAsync(Sentences.InvalidHourFormat(Context.Guild.Id));
                    return;
                }
            }
            using (WebClient wc = new WebClient())
            {
                string[] infos = wc.DownloadString("http://girlsfrontline.wikia.com/wiki/Tactical_Doll_List?action=raw").Split('|');
                Dictionary<string, DateTime> constructionTime = new Dictionary<string, DateTime>();
                IUserMessage msg = await ReplyAsync(Sentences.ParsingInProgress(Context.Guild.Id) + " 0%");
                int nbMax = infos.Count(delegate (string s) { return (s.StartsWith("[[")); });
                int lastPourcent = 0;
                int counter = 0;
                foreach (string s in infos)
                {
                    if (s.StartsWith("[[")) // TODO: Colt Revolver redirect to SAA
                    {
                        string name = s.Replace("\n", "");
                        name = name.Substring(2, name.Length - 4);
                        Wikia.CharacInfo? charac = Wikia.GetCharacInfos(name, Wikia.WikiaType.GirlsFrontline);
                        if (charac != null)
                        {
                            string duration = FindElement(charac.Value.infos.Split('|').ToList(), "manuftimer =");
                            if (duration != null)
                            {
                                DateTime? hour = GetHour(duration);
                                if (hour.HasValue)
                                    constructionTime.Add(charac.Value.name, hour.Value);
                            }
                            counter++;
                            int curr = (counter * 100 / nbMax) / 10;
                            if (curr != lastPourcent)
                            {
                                lastPourcent = curr;
                                await msg.ModifyAsync(x => x.Content = Sentences.ParsingInProgress(Context.Guild.Id) + " " + (curr * 10) + "%");
                            }
                        }
                    }
                }
                await msg.ModifyAsync(x => x.Content = Sentences.ParsingInProgress(Context.Guild.Id) + " 100%");
                    string finalStr = "";
                if (objDt != null)
                {
                    Dictionary<string, DateTime> tmp = new Dictionary<string, DateTime>();
                    foreach (var k in constructionTime)
                    {
                        if (k.Value == objDt.Value)
                            tmp.Add(k.Key, k.Value);
                    }
                    if (tmp.Count == 0)
                    {
                        await ReplyAsync(Sentences.NoTDollHour(Context.Guild.Id));
                        return;
                    }
                    else
                        constructionTime = tmp;
                }
                while (constructionTime.Count > 0)
                {
                    KeyValuePair<string, DateTime>? best = null;
                    foreach (var k in constructionTime)
                    {
                        if (best == null || k.Value.CompareTo(best.Value.Value) == -1)
                            best = k;
                    }
                    finalStr += best.Value.Value.ToString("HH:mm:ss") + ": " + best.Value.Key + Environment.NewLine;
                    constructionTime.Remove(best.Value.Key);
                }
                await ReplyAsync(finalStr);
            }
        }
    }
}