using System;
using NUnit.Framework;

namespace Ruzzie.FuzzyStrings.UnitTests
{
    [TestFixture]
    public class InternalVariablesTests
    {
        [Test]
        public void InternalVariablesMaxCacheSizeInMbTests()
        {
            Console.WriteLine("MaxCacheSizeInMb: " + InternalVariables.MaxCacheSizeInMb);
            Assert.That(InternalVariables.MaxCacheSizeInMb
                      , Is.GreaterThanOrEqualTo(InternalVariables.DefaultCacheItemSizeInMb * 2));
        }

        [Test]
        public void InternalVariablesDefaultCacheSizeInMbTests()
        {
            Console.WriteLine("DefaultCacheItemSizeInMb: " + InternalVariables.DefaultCacheItemSizeInMb);
            Assert.That(InternalVariables.MaxCacheSizeInMb, Is.GreaterThanOrEqualTo(1));
        }

        [TestCase(4096,          524287)]
        [TestCase(1,             3692)]
        [TestCase(0,             1024)]
        [TestCase(8,             29537)]
        [TestCase(2,             7384)]
        [TestCase(long.MinValue, 1024)]
        [TestCase(long.MaxValue, 524287)]
        public void CalculateMaxNumberOfStringsTests(long cacheSizeInMb, int expectedNumberOfStrings)
        {
            Assert.That(InternalVariables.CalculateMaxNumberOfStrings(cacheSizeInMb
                                                                    , InternalVariables.AverageStringSizeInBytes)
                      , Is.EqualTo(expectedNumberOfStrings)
                       );
        }
    }
}