/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.

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
