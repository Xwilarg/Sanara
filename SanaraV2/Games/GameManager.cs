using System.Collections.Generic;
using System.Threading;

namespace SanaraV2.Games
{
    public class GameManager
    {
        public GameManager()
        {
            _games = new List<AGame>();
            _gameThread = new Thread(new ThreadStart(gameLoop));
        }

        private void gameLoop()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                lock (_games)
                {
                    foreach (AGame game in _games)
                    { }
                }
                Thread.Sleep(100);
            }
        }

        private List<AGame> _games;
        private Thread _gameThread;
    }
}
