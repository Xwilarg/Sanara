using NUnit.Framework;
using Sanara.UnitTests.Impl;
using System.Globalization;

namespace Sanara.UnitTests.Tests.Tool
{
    [TestFixture]
    public sealed class Science
    {/*
        [TestCase("3+3*2", "9")]
        [TestCase("(3+3)*2", "12")]
        [TestCase("25/2", "12.5")]
        public async Task CalcTest(string entry, string result)
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>((msg) =>
            {
                Assert.AreEqual(result, msg.Content);

                isDone = true;
                return Task.CompletedTask;
            });

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture; // Some
            var mod = new Module.Tool.ScienceModule();
            Common.AddContext(mod, callback);
            await mod.CalcAsync(entry);
            while (!isDone)
            { }
        }*/
    }
}
