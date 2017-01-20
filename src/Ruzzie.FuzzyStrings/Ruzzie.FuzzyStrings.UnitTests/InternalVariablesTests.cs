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
            Assert.That(InternalVariables.MaxCacheSizeInMb, Is.GreaterThanOrEqualTo(InternalVariables.DefaultCacheItemSizeInMb * 2));
        }

        [Test]
        public void InternalVariablesDefaultCacheSizeInMbTests()
        {
            Console.WriteLine("DefaultCacheItemSizeInMb: " + InternalVariables.DefaultCacheItemSizeInMb);
            Assert.That(InternalVariables.MaxCacheSizeInMb, Is.GreaterThanOrEqualTo(1));
        }
    }
}