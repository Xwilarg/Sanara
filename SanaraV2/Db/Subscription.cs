using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanaraV2.Db
{
    public partial class Db
    {
        private void InitSubscription()
        {
            current = new Dictionary<string, int>();
        }

        public async Task InitSubscription(string subscription)
        {
            if (await R.Db(dbName).Table("Subscriptions").GetAll(subscription).Count().Eq(0).RunAsync<bool>(conn))
            {
                await R.Db(dbName).Table("Subscriptions").Insert(R.HashMap("id", subscription)
                    .With("value", 0)
                    ).RunAsync(conn);
                AddCurrent(subscription, 0);
            }
            else
            {
                dynamic json = await R.Db(dbName).Table("Subscriptions").Get(subscription).RunAsync(conn);
                AddCurrent(subscription, (int)json.value);
            }
        }

        private void AddCurrent(string subscription, int value)
        {
            if (!current.ContainsKey(subscription))
                current.Add(subscription, value);
            else
                current = new Dictionary<string, int>(){ { subscription, value } };
        }

        public async Task SetCurrent(string subscription, int value)
        {
            await R.Db(dbName).Table("Subscriptions").Update(R.HashMap("id", subscription)
                .With("value", value)
                ).RunAsync(conn);
            current[subscription] = value;
        }

        public int GetCurrent(string subscription)
            => current[subscription];

        private Dictionary<string, int> current;
    }
}
