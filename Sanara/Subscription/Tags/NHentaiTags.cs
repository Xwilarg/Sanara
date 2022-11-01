namespace Sanara.Subscription.Tags
{
    public sealed class NHentaiTags : ASubscriptionTags
    {
        public NHentaiTags(string[] tags, bool addDefaultTags) : base(tags, addDefaultTags)
        { }

        public override Dictionary<string, string[]> GetDefaultBlacklist() // List of tags
        {
            return new()
            {
                {
                    "gore", new[] // Visual brutality
                    {
                        "guro", "torture", "necrophilia", "skinsuit", "asphyxiation", "snuff", "ryona"
                    }
                },
                {
                    "badbehaviour", new[] // Disrespect towards some characters involved
                    {
                        "rape", "prostitution", "drugs", "cheating", "humiliation", "slave", "possession", "mind control", "body swap", "netorare", "blackmail", "corruption"
                    }
                },
                {
                    "bodyfluids", new[] // Body fluids (others that semen and blood)
                    {
                        "scat", "vomit", "farting", "omorashi", "urination", "piss drinking"
                    }
                },
                {
                    "unusualEntrances", new[] // Entering inside the body by holes that aren't meant for that
                    {
                        "vore", "absorption", "brain fuck", "nipple fuck", "urethra insertion", "unusual insertions", "eye penetration"
                    }
                },
                {
                    "othersFetichisms", new[] // Others fetichisms that may seams strange from the outside
                    {
                        "birth", "bbm", "ssbbw", "inflation", "smell", "futanari", "bestiality", "body modification", "amputee", "giantess", "bbw", "stomach deformation", "incest"
                    }
                },
                {
                    "tos", new[] // Tags that are against Discord TOS
                    {
                        "lolicon", "shotacon"
                    }
                }
            };
        }
    }
}
