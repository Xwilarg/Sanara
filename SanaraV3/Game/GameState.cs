namespace SanaraV3.Game
{
    public enum GameState
    {
        PREPARE, // Waiting for multiplayer
        READY, // Waiting for first post to be send
        POSTING, // An image is being posted
        RUNNING, // Game is running
        LOST // Game ended
    }
}
