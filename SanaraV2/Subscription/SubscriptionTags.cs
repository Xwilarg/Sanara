using System.Collections.Generic;
using System.Linq;

namespace SanaraV2.Subscription
{
    public class SubscriptionTags
    {
        private string[] _whitelist;
        private string[] _blacklist;

        public static SubscriptionTags ParseSubscriptionTags(string[] tags)
        {
            if (tags.Length == 0)
                return new SubscriptionTags();
            List<string> whitelist = new List<string>();
            List<string> blacklist = new List<string>();
            List<string> tagsList = tags.ToList();
            if (tagsList[0].ToLower() == "full")
                tagsList.RemoveAt(0);
            else
            {
                foreach (var elem in _defaultBlacklist)
                {
                    foreach (string tag in elem.Value)
                    {
                        blacklist.Add(tag);
                    }
                }
            }
            foreach (string s in tagsList)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;
                if (s[0] == '+' || s[0] == '-')
                {
                    var arr = s[0] == '+' ? whitelist : blacklist;
                    var otherArr = s[0] == '+' ? blacklist : whitelist;
                    string tag = string.Join("", s.Skip(1)).ToLower();
                    List<string> toAdd = new List<string>();
                    if (_defaultBlacklist.ContainsKey(tag))
                    {
                        toAdd.AddRange(_defaultBlacklist[tag]);
                    }
                    else
                        toAdd.Add(tag);
                    foreach (string t in toAdd)
                    {
                        if (otherArr.Contains(t))
                            otherArr.Remove(t);
                        arr.Add(t);
                    }
                }
            }
            return new SubscriptionTags
            {
                _whitelist = whitelist.ToArray(),
                _blacklist = blacklist.ToArray()
            };
        }

        public string[] ToStringArray()
        {
            List<string> lists = new List<string>();
            foreach (string s in _whitelist)
                lists.Add("+" + s);
            foreach (string s in _blacklist)
                lists.Add("-" + s);
            return lists.ToArray();
        }

        private static Dictionary<string, string[]> _defaultBlacklist = new Dictionary<string, string[]>
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
                    "rape", "prostitution", "drugs", "cheating", "humiliation", "slave", "possession", "mind control", "body swap", "netorare"
                }
            },
            {
                "bodyfluids", new[] // Body fluids (others that semen and blood)
                {
                    "scat", "vomit", "low scat"
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
                    "shota", "lolicon", "oppai loli", "low lolicon", "low shotacon"
                }
            },
            {
                "othersFetichisms", new[] // Others fetichisms that may seams strange from the outside
                {
                    "birth", "bbm", "ssbbw", "inflation", "smell", "futanari", "omorashi", "bestiality", "body modification", "urination", "piss drinking", "amputee", "giantess"
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
