using Xunit;
using SanaraV2;

namespace Sanara_UnitTests
{
    public class Program
    {
        [Fact]
        public void CeckEmptyAddArgs()
        {
            Assert.True(Utilities.addArgs(new string[] { }) == null);
        }
    }
}
