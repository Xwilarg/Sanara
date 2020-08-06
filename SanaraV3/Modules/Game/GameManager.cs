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
                StaticObjects.Games.RemoveAll(x => x.IsLost()); // Remove all the game that were lost
                Thread.Sleep(200);
            }
        }

        private readonly Thread thread;
    }
}
