using Discord;

namespace Sanara.Game.MultiplayerMode
{
    public interface IMultiplayerMode
    {
        public void Init(List<IUser> users);

        /// <summary>
        /// Called before a post is sent
        /// </summary>
        public string PrePost();

        /// <summary>
        /// Called before checking an user answer
        /// </summary>
        public void PreAnswerCheck(IUser user);

        /// <summary>
        /// Called when an user find a good answer
        /// </summary>
        public void AnswerIsCorrect(IUser user);

        /// <summary>
        /// Can the normal game flow decide the end of the game
        /// </summary>
        public bool CanLooseAuto();

        /// <summary>
        /// Called when an user loose, returns if the game should stop
        /// </summary>
        public bool Loose();

        public string GetOutroLoose();

        public string GetWinner();

        public string GetRules();
    }
}
