using NUnit.Framework;
using Sanara;
using Sanara.Game.Preload;
using Sanara.Game.Preload.Impl;
using Sanara.Game.Preload.Result;

namespace Sanara.UnitTests.Tests.Game
{
    [TestFixture]
    public sealed class Game
    {/*
        private async Task CheckImageAsync(QuizzPreloadResult current)
        {
            var h = await StaticObjects.HttpClient.GetStringAsync(current.ImageUrl);
            var file = await StaticObjects.HttpClient.GetStreamAsync(current.ImageUrl);
            using MemoryStream ms = new();
            await file.CopyToAsync(ms);

            byte[] bytes = ms.ToArray();

            var png = new byte[] { 137, 80, 78, 71 };
            var jpeg = new byte[] { 255, 216, 255, 224 };

            Assert.IsTrue(
                bytes.Take(4).SequenceEqual(png) ||
                bytes.Take(4).SequenceEqual(jpeg),
                current.Answers[0] + " doesn't have a valid image (" + current.ImageUrl + ")");
        }

        [TestCase(typeof(KancollePreload))]
        //[TestCase(typeof(FateGOPreload))]
        //[TestCase(typeof(AzurLanePreload))]
        [TestCase(typeof(PokemonPreload))]
        [TestCase(typeof(ArknightsPreload))]
        [TestCase(typeof(GirlsFrontlinePreload))]
        public async Task RandomGameImageTestAsync(Type t)
        {
            var preload = (IPreload)Activator.CreateInstance(t);
            var names = preload.Load().Cast<QuizzPreloadResult>().ToArray();

            for (int i = 0; i < 10; i++)
            {
                var current = names[StaticObjects.Random.Next(0, names.Length)];

                await CheckImageAsync(current);
            }
        }*/
    }
}
