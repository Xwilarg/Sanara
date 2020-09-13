using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SanaraV3.Game
{
    public sealed class GameManager
    {
        public GameManager()
        {
            thread = new Thread(new ThreadStart(Loop));
        }

        public void Init()
        {
            thread.Start();
        }

        private void Loop()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                foreach (var game in StaticObjects.Games)
                {
                    _ = Task.Run(() => { game.CheckAnswersAsync().GetAwaiter().GetResult(); });
                    game.CheckTimerAsync().GetAwaiter().GetResult();
                }
                foreach (var g in StaticObjects.Games.Where(x => x.AsLost()))
                    g.Dispose();
                StaticObjects.Games.RemoveAll(x => x.AsLost()); // Remove all the game that were lost
                Thread.Sleep(200);
            }
        }

        private readonly Thread thread;
    }
}
