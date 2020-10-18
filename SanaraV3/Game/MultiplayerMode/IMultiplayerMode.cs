using Discord;
using System.Collections.Generic;

namespace SanaraV3.Game.MultiplayerMode
{
    public interface IMultiplayerMode
    {
        public void Init(List<IUser> users);

        public string PrePost(); // Called before a post is sent

        public void PreAnswerCheck(IUser user); // Called before checking an user answer

        public void AnswerIsCorrect(IUser user); // Called when an user find a good answer

        public bool CanLooseAuto(); // Can the normal game flow decide the end of the game

        public bool Loose(); // Called when an user loose, returns if the game should stop

        public string GetOutroLoose();

        public string GetWinner();

        public string GetRules();
    }
}
