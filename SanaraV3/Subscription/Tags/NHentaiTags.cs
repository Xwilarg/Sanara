using System.Collections.Generic;

namespace SanaraV3.Subscription.Tags
{
    public sealed class NHentaiTags : ASubscriptionTags
    {
        public override Dictionary<string, string[]> GetDefaultBlacklist()
        {
            return new Dictionary<string, string[]>
            {
                {
                    "gore", new[] // Visual brutality
                    {
                        "guro", "torture", "necrophilia", "skinsuit", "asphyxiation", "snuff"
                    }
                },
                {
                    "badbehaviour", new[] // Disrespect towards some characters involved
                    {
                        "rape", "prostitution", "drugs", "cheating", "humiliation", "slave", "possession", "mind control", "body swap", "netorare", "blackmail"
                    }
                },
                {
                    "bodyfluids", new[] // Body fluids (others that semen and blood)
                    {
                        "scat", "vomit", "low scat", "omorashi", "urination", "piss drinking"
                    }
                },
                {
                    "unusualEntrances", new[] // Entering inside the body by holes that aren't meant for that
                    {
                        "vore", "absorption", "brain fuck", "nipple fuck", "urethra insertion"
                    }
                },
                {
                    "tos", new[] // Tags that are against Discord's Terms of Service (characters that are too young)
                    {
                        "shotacon", "lolicon", "oppai loli", "low lolicon", "low shotacon"
                    }
                },
                {
                    "othersFetichisms", new[] // Others fetichisms that may seams strange from the outside
                    {
                        "birth", "bbm", "ssbbw", "inflation", "smell", "futanari", "bestiality", "body modification", "amputee", "giantess", "bbw"
                    }
                },
                {
                    "yaoi", new[] // I'm just making the baseless assumption that you are an heterosexual male, if that's not the case sorry :( - (You can enable it back anyway)
                    {
                        "yaoi"
                    }
                }
            };
        }
    }
}
