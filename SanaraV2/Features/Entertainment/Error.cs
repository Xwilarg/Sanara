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
namespace SanaraV2.Features.Entertainment
{
    public static class Error
    {

        public enum Score
        {
            None,
            NoScore
        }

        public enum Vn
        {
            None,
            Help,
            NotFound
        }

        public enum AnimeManga
        {
            None,
            Help,
            NotFound
        }

        public enum Xkcd
        {
            None,
            InvalidNumber,
            NotFound
        }

        public enum YouTube
        {
            None,
            NotFound,
            InvalidApiKey,
            Help
        }

        public enum Game
        {
            None,
            AlreadyRunning,
            WrongName,
            WrongDifficulty,
            NotNsfw,
            FullNotAvailable
        }
    }
}
