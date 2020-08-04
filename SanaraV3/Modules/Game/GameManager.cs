using System.Threading;

namespace SanaraV3.Modules.Game
{
    public sealed class GameManager
    {


        private void Loop()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                StaticObjects.Games.RemoveAll(x => x.IsLost()); // Remove all the game that were lost
                Thread.Sleep(200);
            }
        }

        private Thread thread;
    }
}
