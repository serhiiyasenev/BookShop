using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(100)]

namespace UnitTests
{
    public class BaseTest
    {
        public BaseTest()
        {

        }

    }
}