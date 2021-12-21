using Discord;

namespace Sanara.Subscription
{
    public class SubscriptionGuild
    {
        public SubscriptionGuild(ITextChannel textChan, ASubscriptionTags tags)
        {
            TextChan = textChan;
            Tags = tags;
        }

        public ITextChannel TextChan { set; get; }
        public ASubscriptionTags Tags { set; get; }
    }
}
