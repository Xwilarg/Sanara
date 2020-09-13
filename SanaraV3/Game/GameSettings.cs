namespace SanaraV3.Game
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
