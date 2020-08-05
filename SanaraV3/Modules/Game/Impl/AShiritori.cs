using Discord;

namespace SanaraV3.Modules.Game.Impl
{
    public abstract class AShiritori : AGame
    {
        public AShiritori(IMessageChannel textChan) : base(textChan, StaticObjects.ModeText)
        { }

        protected override string GetPostInternal()
        {
            return null;
        }
    }
}
