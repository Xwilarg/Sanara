namespace SanaraV3.Games
{
    public struct GameSettings
    {
        public GameSettings(bool isMultiplayer)
        {
            IsMultiplayer = isMultiplayer;
        }

        public bool IsMultiplayer { get; }
    }
}
