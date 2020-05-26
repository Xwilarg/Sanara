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

namespace SanaraV2.Features.NSFW
{
    public static class Error
    {
        public enum Subscribe
        {
            None,
            Help,
            InvalidChannel,
            ChanNotSafe
        }

        public enum Download
        {
            None,
            Help,
            ChanNotSafe,
            NotFound
        }

        public enum SourceBooru
        {
            None,
            Help,
            NotFound,
            NotAnUrl
        }

        public enum Booru
        {
            None,
            ChanNotNSFW,
            NotFound
        }

        public enum BooruTags
        {
            None,
            Help,
            NotFound
        }

        public enum Doujinshi
        {
            None,
            ChanNotNSFW,
            NotFound
        }
    }
}
