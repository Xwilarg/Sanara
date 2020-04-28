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
using System.Threading.Tasks;

namespace SanaraV2.Subscription
{
    public class SubscriptionManager
    {
        public static SubscriptionManager GetCurrentSubscription()
        {
            if (me == null) return new SubscriptionManager();
            else return me;
        }

        private SubscriptionManager()
        {
            anime = new AnimeSubscription();
            _ = Task.Run(async () =>
            {
                for (;;)
                {
                    await Task.Delay(120000); // 2 minutes
                    await anime.UpdateChannelAsync();
                }
            });
        }

        private AnimeSubscription anime;
        private static SubscriptionManager me = null;
    }
}
