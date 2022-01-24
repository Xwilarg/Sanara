/*

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadDoujinHelp()
        {
            _submoduleHelp.Add("Doujin", "Get self published work");
            _help.Add(("Nsfw", new Help("Doujin", "Doujinshi", new[] { new Argument(ArgumentType.OPTIONAL, "tags/id") }, "Get a random doujinshi. You can either provide some tags or directly give its id.", new[] { "Doujin", "Nhentai" }, Restriction.Nsfw, "Doujinshi kancolle yuri")));
            _help.Add(("Nsfw", new Help("Doujin", "Dlsite", new[] { new Argument(ArgumentType.MANDATORY, "query") }, "Get the most popular work from dlsite given a query.", Array.Empty<string>(), Restriction.Nsfw, "Dlsite 艦隊")));
            _help.Add(("Nsfw", new Help("Doujin", "Download doujinshi", new[] { new Argument(ArgumentType.MANDATORY, "id") }, "Download a doujinshi given its id.", new[] { "Download doujin" }, Restriction.Nsfw, "Download doujin 321633")));
            _help.Add(("Nsfw", new Help("Doujin", "Subscribe doujinshi", new[] { new Argument(ArgumentType.MANDATORY, "text channel"), new Argument(ArgumentType.OPTIONAL, "tags") }, "Get information on all new doujinshi in a channel.", new[] { "Subscribe doujin", "Subscribe nhentai" }, Restriction.Nsfw | Restriction.AdminOnly, "Subscribe doujinshi +\"ke-ta\"")));
            _help.Add(("Nsfw", new Help("Doujin", "Unsubscribe doujinshi", new Argument[0], "Remove a doujinshi subscription.", new[] { "Unsubscribe doujin", "Unsubscribe nhentai" }, Restriction.Nsfw | Restriction.AdminOnly, null)));
            _help.Add(("Nsfw", new Help("Doujin", "Doujinshi popularity", new[] { new Argument(ArgumentType.OPTIONAL, "tags") }, "Get the most popular doujinshi given some tags. If none are provided, give the trending ones.", new[] { "Doujinshi popularity", "Doujinshi p", "Doujin p", "Doujin popularity", "Nhentai p", "Nhentai popularity" }, Restriction.Nsfw | Restriction.AdminOnly, null)));
        }
    }
}

namespace SanaraV3.Module.Nsfw
{
    public sealed class DoujinModule : ModuleBase
    {

        [Command("Doujinshi popularity", RunMode = RunMode.Async), Priority(2), RequireNsfw, Alias("Doujinshi p", "Doujin popularity", "Doujin p", "Nhentai popularity", "Nhentai p")]
        public async Task PopularityAsync()
        {
            string html = await StaticObjects.HttpClient.GetStringAsync("https://nhentai.net/");
            html = html.Split(new[] { "<div class=\"container index-container index-popular\">" }, StringSplitOptions.None)[1]
                .Split(new[] { "<div class=\"container index-container\">" }, StringSplitOptions.None)[0];
            var elems = new List<Diaporama.Impl.Doujinshi>();
            foreach (var match in Regex.Matches(html, "<a href=\"\\/g\\/([0-9]+)\\/\"").Cast<Match>())
            {
                var doujinshi = await SearchClient.SearchByIdAsync(int.Parse(match.Groups[1].Value));
                elems.Add(new Diaporama.Impl.Doujinshi(doujinshi.url.AbsoluteUri, doujinshi.pages[0].imageUrl.AbsoluteUri, doujinshi.prettyTitle, doujinshi.tags.Select(x => x.name).ToArray(), doujinshi.id));
            }
            var msg = await ReplyAsync(embed: Diaporama.ReactionManager.Post(elems[0], 1, elems.Count));
            StaticObjects.Diaporamas.Add(msg.Id, new Diaporama.Diaporama(elems.ToArray()));
            await msg.AddReactionsAsync(new[] { new Emoji("⏪"), new Emoji("◀️"), new Emoji("▶️"), new Emoji("⏩") });
        }

        [Command("Doujinshi popularity", RunMode = RunMode.Async), Priority(2), RequireNsfw, Alias("Doujinshi p", "Doujin popularity", "Doujin p", "Nhentai popularity", "Nhentai p")]
        public async Task PopularityAsync([Remainder]string tags)
        {
            NHentaiSharp.Search.SearchResult result;
            try
            {
                result = await SearchClient.SearchWithTagsAsync(tags.Split(' '), 1, PopularitySort.AllTime);
            }
            catch (InvalidArgumentException)
            {
                throw new CommandFailed("There is no doujin with these tags");
            }
            var elems = new List<Diaporama.Impl.Doujinshi>();
            foreach (var doujinshi in result.elements.Take(5))
            {
                elems.Add(new Diaporama.Impl.Doujinshi(doujinshi.url.AbsoluteUri, doujinshi.pages[0].imageUrl.AbsoluteUri, doujinshi.prettyTitle, doujinshi.tags.Select(x => x.name).ToArray(), doujinshi.id));
            }
            var msg = await ReplyAsync(embed: Diaporama.ReactionManager.Post(elems[0], 1, elems.Count));
            StaticObjects.Diaporamas.Add(msg.Id, new Diaporama.Diaporama(elems.ToArray()));
            await msg.AddReactionsAsync(new[] { new Emoji("⏪"), new Emoji("◀️"), new Emoji("▶️"), new Emoji("⏩") });
        }

        [Command("Dlsite", RunMode = RunMode.Async), RequireNsfw]
        public async Task Dlsite([Remainder]string query = "")
        {
            var html = await StaticObjects.HttpClient.GetStringAsync("https://www.dlsite.com/maniax/fsr/=/language/jp/sex_category%5B0%5D/male/ana_flg/all/order%5B0%5D/trend/genre_and_or/or/options_and_or/or/per_page/100/show_type/1/from/fsr.again/keyword/" + query);

            // Parse HTML to only keep search results
            html = html.Split(new[] { "id=\"search_result_list\"" }, StringSplitOptions.None)[1];
            html = html.Split(new[] { "class=\"result_contents\"" }, StringSplitOptions.None)[0];

            var elems = new List<Diaporama.Impl.Dlsite>();
            foreach (var elem in html.Split("work_1col_thumb", StringSplitOptions.None).Skip(1))
            {
                var url = Regex.Match(elem, "href=\"([^\"]+)\"").Groups[1].Value;
                var preview = "http:" + Regex.Match(elem, "src=\"([^\"]+)\"").Groups[1].Value;
                var name = Regex.Match(elem, "alt=\"([^\"]+)\"").Groups[1].Value;
                var id = long.Parse(Regex.Match(elem, "[A-Z]{2}([^\\.]+)\\.html").Groups[1].Value);
                var rating = Regex.Match(elem, "star_rating star_([0-9]{2})");
                var nbDownload = Regex.Match(elem, "<span class=\"_dl_count_[A-Z]{2}[0-9]+\">([0-9,]+)").Groups[1].Value.Replace(',', ' ');
                var price = Regex.Match(elem, "<span class=\"work_price[^\"]*\">([0-9,]+)").Groups[1].Value.Replace(',', ' ');
                var description = HttpUtility.HtmlDecode(Regex.Match(elem, "<dd class=\"work_text\">([^<]+)+").Groups[1].Value);
                var type = Regex.Match(elem, "work_type[^\"]+\">([^<]+)").Groups[1].Value;
                var subElem = elem.Contains("search_tag") ?
                    elem.Split(new[] { "search_tag" }, StringSplitOptions.None)[1].Split(new[] { "</dd>" }, StringSplitOptions.None)[0]
                    : "";
                var tags = Regex.Matches(subElem, "<a href=\"[^\"]+\">([^<]+)").Cast<Match>().Select(x => x.Groups[1].Value).ToArray();
                elems.Add(new(url, preview, name, id, rating.Length > 1 ? int.Parse(rating.Groups[1].Value) / 10f : null, nbDownload, price, description, tags, type));
            }
            var msg = await ReplyAsync(embed: Diaporama.ReactionManager.Post(elems[0], 1, elems.Count));
            StaticObjects.Diaporamas.Add(msg.Id, new Diaporama.Diaporama(elems.ToArray()));
            await msg.AddReactionsAsync(new[] { new Emoji("⏪"), new Emoji("◀️"), new Emoji("▶️"), new Emoji("⏩") });
        }
    }
}
*/