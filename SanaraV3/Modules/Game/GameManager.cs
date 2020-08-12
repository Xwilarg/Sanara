using System.Threading;

namespace SanaraV3.Modules.Game
{
    public sealed class GameManager
    {
        public GameManager()
        {
            thread = new Thread(new ThreadStart(Loop));
            thread.Start();
        }

        private void Loop()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                foreach (var game in StaticObjects.Games)
                    game.CheckTimerAsync().GetAwaiter().GetResult();
                StaticObjects.Games.RemoveAll(x => x.AsLost()); // Remove all the game that were lost
                Thread.Sleep(200);
            }
        }

        private readonly Thread thread;
    }
}
