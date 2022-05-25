using System;

namespace OpenStack
{
    public static class TestData
    {
        public static string GenerateName()
        {
            return string.Format("ci-test-{0}", Guid.NewGuid());
        }
    }
}
