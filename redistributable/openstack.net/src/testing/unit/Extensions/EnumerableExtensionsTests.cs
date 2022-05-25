using Xunit;

namespace System.Collections.Generic
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void ToCollection_ReturnsANewCollection_WhenPassedInNull()
        {
            IEnumerable<int> items = null;
            IList<int> result = items.ToNonNullList();
            Assert.NotNull(result);
        }
    }
}
