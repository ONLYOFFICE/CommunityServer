using System;

namespace Rackspace
{
    public static class TestData
    {
        public static string GenerateName()
        {
            return $"ci-test-{Guid.NewGuid()}";
        }
    }
}
