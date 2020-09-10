namespace SanaraV3.Games
{
    public enum GameState
    {
        PREPARE, // Waiting for multiplayer
        POSTING, // An image is being posted
        RUNNING, // Game is running
        LOST // Game ended
    }
}
