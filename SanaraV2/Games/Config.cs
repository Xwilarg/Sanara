namespace SanaraV2.Games
{
    public struct Config
    {
        public Config(int refTime, Difficulty difficulty, string gameName)
        {
            this.refTime = refTime;
            this.difficulty = difficulty;
            this.gameName = gameName;
        }

        public int refTime; // Time before the counter end and the player loose
        public Difficulty difficulty;
        public string gameName; // Used to store the score in the db
    }

    public enum Difficulty // Easy mode give twice more time
    {
        Normal = 1,
        Easy
    }
}
