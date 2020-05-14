using Discord;
using Discord.Commands;
using SanaraV2.Modules.Base;
using System;
using System.Threading.Tasks;

namespace SanaraV2.Modules.GamesInfo
{
    [Group("Arknights"), Alias("Ak")]
    public class Arknights : ModuleBase
    {
        [Command("", RunMode = RunMode.Async), Priority(-1)]
        public async Task CharacDefault(params string[] shipNameArr) => await Charac(shipNameArr);

        [Command("Charac", RunMode = RunMode.Async), Alias("Character")]
        public async Task Charac(params string[] name)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Arknights);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Arknights);
            var result = await Features.GamesInfo.Arknights.SearchCharac(name);
            switch (result.error)
            {
                case Features.GamesInfo.Error.Charac.Help:
                    await ReplyAsync(Sentences.ArknightsHelp(Context.Guild.Id));
                    break;

                case Features.GamesInfo.Error.Charac.NotFound:
                    await ReplyAsync(Sentences.OperatorDontExist(Context.Guild.Id));
                    break;

                case Features.GamesInfo.Error.Charac.None:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = result.answer.name,
                        ImageUrl = result.answer.imgUrl,
                        Color = Color.Blue
                    }.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
