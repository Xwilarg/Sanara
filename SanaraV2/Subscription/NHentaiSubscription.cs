using Discord;
using NHentaiSharp.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2.Subscription
{
    public class NHentaiSubscription : ASubscription
    {
        public NHentaiSubscription()
        {
            var feed = GetFeed().GetAwaiter().GetResult();
            if (feed.Length > 0)
                Current = feed[0].Item1;
            else
                Current = 0;
        }

        public override async Task<(int, EmbedBuilder)[]> GetFeed()
        {
            var datas = await SearchClient.SearchAsync();
            return datas.elements.Select(x => ((int)x.id, new EmbedBuilder
            {
                Color = new Color(255, 20, 147),
                Title = x.prettyTitle,
                Description = string.Join(", ", x.tags.Select(y => y.name)),
                Url = x.url.ToString(),
                ImageUrl = x.pages[0].imageUrl.ToString(),
                Footer = new EmbedFooterBuilder()
                {
                    Text = Modules.NSFW.Sentences.ClickFull(0) + "\n\n" + Modules.NSFW.Sentences.DownloadDoujinshiInfo(0, x.id.ToString())
                }
            })).Reverse().ToArray();
        }

        public struct NHentaiData
        {
            public long id;
            public string name;
            public string[] tags;
            public Uri urlDoujinshi;
            public Uri urlImage;
        }

        public string[] goreTags = new[] // Visual brutality
        {
            "guro", "torture", "necrophilia", "skinsuit", "asphyxiation", "snuff"
        };

        public string[] badbehaviour = new[] // Disrespect towards some characters involved
        {
            "rape", "prostitution", "drugs", "cheating", "humiliation", "slave", "possession", "mind control", "body swap", "netorare"
        };

        public string[] bodyfluids = new[] // Body fluids (others that semen and blood)
        {
            "scat", "vomit", "low scat"
        };

        public string[] unusualEntrances = new[] // Entering inside the body by holes that aren't meant for that
        {
            "vore", "absorption", "brain fuck", "nipple fuck", "urethra insertion"
        };

        public string[] tos = new[] // Tags that are against Discord's Terms of Service (characters that are too young)
        {
            "shota", "lolicon", "oppai loli", "low lolicon", "low shotacon"
        };

        public string[] othersFetichisms = new[] // Others fetichisms that may seams strange from the outside
        {
            "birth", "bbm", "ssbbw", "inflation", "smell", "futanari", "omorashi", "bestiality", "body modification", "urination", "piss drinking"
        };

        public string[] yaoi = new[] // I'm just making the baseless assumption that you are an heterosexual male, if that's not the case sorry :( - (You can enable it back anyway)
        {
            "yaoi"
        };
    }
}
