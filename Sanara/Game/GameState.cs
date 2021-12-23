namespace Sanara.Game
{
    public enum GameState
    {
        Prepare, // Waiting for multiplayer
        Ready, // Waiting for first post to be send
        Posting, // An image is being posted
        Running, // Game is running
        Lost // Game ended
    }
}
