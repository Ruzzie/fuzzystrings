using DuoVia.FuzzyStrings;
using NUnit.Framework;

namespace fuzzytest
{
    [TestFixture]
    internal class RingLikeCacheWithFixedSizeTest
    {
        [Test]
        public void Smokey()
        {
            RingLikeCacheWithFixedSize<string, string> cache = new RingLikeCacheWithFixedSize<string, string>(5);

            string valueOne = cache.GetOrAdd("1", key => "1Value");
            string valueTwo = cache.GetOrAdd("2", key => "2Value");
            string valueThree = cache.GetOrAdd("3", key => "3Value");
            string valueFour = cache.GetOrAdd("4", key => "4Value");
            string valueFive = cache.GetOrAdd("5", key => "5Value");

            string valueSix = cache.GetOrAdd("6", key => "6Value");

            Assert.That(cache.GetOrAdd("1", key => "1ValueUpdated"), Is.EqualTo("1ValueUpdated"));
            Assert.That(cache.GetOrAdd("6", key => "DoNotUpdate"), Is.EqualTo("6Value"));
            Assert.That(cache.GetOrAdd("5", key => "DoNotUpdate"), Is.EqualTo("5Value"));
            Assert.That(cache.GetOrAdd("4", key => "DoNotUpdate"), Is.EqualTo("4Value"));
            Assert.That(cache.GetOrAdd("3", key => "Update3"), Is.EqualTo("Update3"));
            Assert.That(cache.GetOrAdd("2", key => "Update2"), Is.EqualTo("Update2"));
        }
    }
}