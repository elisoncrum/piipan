using Xunit;

namespace Piipan.Match.Orchestrator.Tests
{
    public class LookupTests
    {

        [Fact]
        public void LookupIdConformsToLength()
        {
            var id = LookupId.Generate();

            Assert.Equal(7, id.Length);
        }

        [Fact]
        public void LookupIdConformsToAlphabet()
        {
            var disallowed = "01AEIOUabcdefghijklmnopqrstuvwxyz";
            var id = LookupId.Generate();

            foreach (char c in disallowed)
            {
                Assert.False(id.Contains(c));
            }
        }
    }
}
