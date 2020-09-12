using SanaraV3.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace SanaraV3.Subscription
{
    public abstract class ASubscriptionTags
    {
        /// <param name="addDefaultTags">Should add the default blacklist?</param>
        protected ASubscriptionTags(string[] tags, bool addDefaultTags)
        {
            List<string> whitelist = new List<string>();
            List<string> blacklist = new List<string>();
            List<string> tagsList = tags.ToList();

            if (addDefaultTags) // We don't need to add the default blacklist if we load tags from the db
            {
                if (tagsList.Contains("full")) // User don't want to use the default blacklist
                    tagsList.Remove("full");
                else
                {
                    foreach (var list in GetDefaultBlacklist())
                        blacklist.AddRange(list.Value);
                }
            }
            foreach (string s in tagsList)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;
                char indicator = s[0]; // First character contains the type of the tag
                if (indicator == '+' || indicator == '-' || indicator == '*')
                {
                    string tag = string.Join("", s.Skip(1)); // Actual tag without the indicator

                    List<string> toAdd = new List<string>();
                    if (GetDefaultBlacklist().ContainsKey(tag))
                        toAdd.AddRange(GetDefaultBlacklist()[tag]);
                    else
                        toAdd.Add(tag);

                    if (indicator == '*') // * remove the tag from blacklist and whitelist
                    {
                        foreach (var t in toAdd)
                        {
                            if (whitelist.Contains(t)) whitelist.Remove(t);
                            if (blacklist.Contains(t)) blacklist.Remove(t);
                        }
                    }
                    else
                    {
                        var list = indicator == '+' ? whitelist : blacklist;
                        var other = indicator == '+' ? blacklist : whitelist;

                        foreach (string t in toAdd)
                        {
                            if (other.Contains(t)) other.Remove(t); // A tag can't be in the whitelist and blacklist at the same time
                            list.Add(t);
                        }
                    }
                }
                else
                    throw new CommandFailed("Your tag must begin with +, - or *");
            }
            _whitelist = whitelist.ToArray();
            _blacklist = blacklist.ToArray();
        }

        public abstract Dictionary<string, string[]> GetDefaultBlacklist();

        public string GetWhitelistTags()
            => _whitelist.Length > 0 ? $"`{string.Join(", ", _whitelist)}`" : "None";

        public string GetBlacklistTags()
            => _blacklist.Length > 0 ? $"`{string.Join(", ", _blacklist)}`" : "None";

        public string[] ToStringArray()
        {
            List<string> lists = new List<string>();
            lists.AddRange(_whitelist.Select(x => "+" + x));
            lists.AddRange(_blacklist.Select(x => "-" + x));
            return lists.ToArray();
        }

        /// <summary>
        /// The tag must not be blacklisted
        /// If there is a whitelist set, it must be inside
        /// </summary>
        public bool IsTagValid(string[] tags)
        {
            foreach (string tag in tags)
            {
                if (_blacklist.Contains(tag))
                    return false;
                if (_whitelist.Contains(tag))
                    return true;
            }
            return _whitelist.Length == 0; // Tag not found in the whitelist, is valid only if the whitelist is not set
        }

        private string[] _whitelist, _blacklist;
    }
}
