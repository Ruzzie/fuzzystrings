using System;
using NUnit.Framework;

namespace Ruzzie.FuzzyStrings.UnitTests
{
    [TestFixture]
    public class StripTests
    {
        [TestCase("Doctor Who!", "Doctor Who")]
        [TestCase(" Flashback {4}{R}", " Flashback 4R")]
        [TestCase(" Æther Vial", " ther Vial")]
        [TestCase(" Æther Vial-", " ther Vial-")]
        public void SmokeTest(string input, string expected)
        {
            Assert.That(StringExtensions.StripWithRegex(input), Is.EqualTo(expected));
            Assert.That(StringExtensions.StripAlternative(input), Is.EqualTo(StringExtensions.StripWithRegex(input)));
            Assert.That(StringExtensions.StripAlternativeV2(input), Is.EqualTo(StringExtensions.StripAlternative(input)));
        }

        [Test]
        public void AltStripTest()
        {
            for (int i = 0; i < 128; i++)
            {
                char c = (char) i;

                if (c == '-' || c == ' ')
                {
                    Console.WriteLine("Char " + i + " :'" + c + "'");
                }

                //65 && 90 : A-Z
                //48 && 57: 0-9
                //97 && 122: a-z
                //32: ' '
                //45: '-'
            }
        }

        [TestCase(1,2,3,1)]
        [TestCase(3,2,1,1)]
        [TestCase(3,1,2,1)]
        [TestCase(3,3,2,2)]
        [TestCase(2,3,3,2)]
        [TestCase(3,2,3,2)]
        [TestCase(0,0,0,0)]
        [TestCase(0,0,-1,-1)]
        [TestCase(0,0,-1,-1)]
        [TestCase(-1,0,-1,-1)]
        [TestCase(1,0,-1,-1)]
        [TestCase(1,1,1,1)]
        public void FindMinumumOptimized(int a, int b, int c, int expected)
        {            
            Assert.That(LevenshteinDistanceExtensions.FindMinimumOptimized(a,b,c), Is.EqualTo(expected));
            Assert.That(LevenshteinDistanceExtensions.FindMinimum(a,b,c), Is.EqualTo(expected));
        }
    }
}