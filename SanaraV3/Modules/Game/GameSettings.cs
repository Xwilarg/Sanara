namespace SanaraV3.Modules.Game
{
    public struct GameSettings
    {
        public GameSettings(bool isMultiplayer)
        {
            IsMultiplayer = isMultiplayer;
        }

        public bool IsMultiplayer;
    }
}
