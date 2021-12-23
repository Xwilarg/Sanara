using NUnit.Framework;

namespace Sanara.UnitTests
{
    [SetUpFixture]
    public class Setup
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            if (!Directory.Exists("Saves")) Directory.CreateDirectory("Saves");
            if (!Directory.Exists("Saves/Radio")) Directory.CreateDirectory("Saves/Radio");
            if (!Directory.Exists("Saves/Download")) Directory.CreateDirectory("Saves/Download");
            if (!Directory.Exists("Saves/Game")) Directory.CreateDirectory("Saves/Game");
            await StaticObjects.InitializeAsync(new Credentials());
        }
    }
}
