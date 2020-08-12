namespace SanaraV3.Modules.Game
{
    public enum GameState
    {
        PREPARE, // Waiting for multiplayer
        POSTING, // An image is being posted
        RUNNING, // Game is running
        LOST // Game ended
    }
}
