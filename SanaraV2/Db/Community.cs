using Newtonsoft.Json.Linq;
using SanaraV2.Community;
using System.Threading.Tasks;

namespace SanaraV2.Db
{
    public partial class Db
    {
        private async Task PreloadProfiles()
        {
            var json = await R.Db(dbName).Table("Profiles").RunAsync(conn);
            foreach (JObject elem in json)
            {
                string id = elem["id"].Value<string>();
                Program.p.cm.AddProfile(ulong.Parse(id), new Profile(id, elem));
            }
        }

        public void AddProfile(Profile p)
        {
            R.Db(dbName).Table("Profiles").Insert(p.GetProfileToDb(R)).Run(conn);
        }
    }
}
