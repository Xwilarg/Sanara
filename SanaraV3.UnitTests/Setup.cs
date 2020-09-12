using NUnit.Framework;
using System.Threading.Tasks;

namespace SanaraV3.UnitTests
{
    [SetUpFixture]
    public class Setup
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            await StaticObjects.InitializeAsync(new Credentials());
        }
    }
}
