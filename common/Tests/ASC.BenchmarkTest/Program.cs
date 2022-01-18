using BenchmarkDotNet.Running;

namespace ASC.BenchmarkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<DbManagerTest>();
        }
    }
}
