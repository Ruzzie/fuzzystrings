using NUnit.Framework;

namespace Ruzzie.FuzzyStrings.UnitTests
{
    [TestFixture]
    public class WhitespaceTokenizerTests
    {
        [TestCase("a\nb\tc d",4)]
        [TestCase("a\nb\tc",3)]
        [TestCase("a c",2)]
        [TestCase("ac",1)]
        [TestCase("",0)]
        public void TokenizeTest(string input, int expectedTokenCount)
        {
            IStringTokenizer stringTokenizer = new WhitespaceTokenizer();

            Assert.That(stringTokenizer.Tokenize(input).Length, Is.EqualTo(expectedTokenCount));
        }
    }
}