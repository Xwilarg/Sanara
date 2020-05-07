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
            foreach (JProperty elem in json)
            {
                Program.p.cm.AddProfile(ulong.Parse(elem.Name), new Profile(elem.Name, elem.Value));
            }
        }
    }
}
