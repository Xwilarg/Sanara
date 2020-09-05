using System;
using Xunit;

namespace SanaraV3.UnitTests
{
    public sealed class SkipIfNoEnvTheory : TheoryAttribute
    {
        public SkipIfNoEnvTheory()
        {
            Timeout = 30000;

            string env = Environment.GetEnvironmentVariable("YOUTUBE_KEY");
            if (env == null)
                Skip = "Environment variables not set";
        }
    }
}