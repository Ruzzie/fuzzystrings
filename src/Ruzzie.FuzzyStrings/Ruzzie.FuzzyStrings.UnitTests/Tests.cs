using NUnit.Framework;

namespace Ruzzie.FuzzyStrings.UnitTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        [TestCase("test", "w", 0.059523809523809521d)]
        [TestCase("test", "W", 0.059523809523809521d)]
        [TestCase("test", "w ", 0.059523809523809521d)]
        [TestCase("test", "W ", 0.059523809523809521d)]
        [TestCase("test", " w", 0.059523809523809521d)]
        [TestCase("test", " W", 0.059523809523809521d)]
        [TestCase("test", " w ", 0.059523809523809521d)]
        [TestCase("test", " W ", 0.059523809523809521d)]
        [TestCase("Kjeldoran Elite Guard", "Kjeldoran Elite Guard", StringExtensions.ExactMatchProbability)]
        public void IssueTest(string input, string compareTo, double expected)
        {
            Assert.That(input.FuzzyMatch(compareTo), Is.EqualTo(expected));
        }

        [TestCase("Kjeldoran elite", "Kjeldoran Gargoyle", 0.56630225677506774d)]
        [TestCase("Kjeldoran Gargoyle", "Kjeldoran elite", 0.56630225677506774d)]
        [TestCase("Kjeldoran elite", "Kjeldoran Elite Guard", 0.71008452873323602d)]
        [TestCase("kjeldoran elite guard", "kjeldoran elite guard", StringExtensions.ExactMatchProbability)]
        [TestCase("guard elite kjeldoran", "kjeldoran elite guard", 0.70495849510110475d)]
        [TestCase("guard kjeldoran elite", "kjeldoran elite guard", 0.70495849510110475d)]
        [TestCase("kjeldoran elite guard master", "kjeldoran elite guard",  0.55814012072314667d)]
        [TestCase("Grizzled Angler", "Tangle Angler", 0.73445802805706839d)]
        [TestCase("", "",StringExtensions.ExactMatchProbability)]
        [TestCase("\t\n", "   ", StringExtensions.ExactMatchProbability)]
        public void TokenizeFuzzyMatch(string input, string compareTo, double expected)
        {
            Assert.That(input.FuzzyMatchTokens(compareTo, false), Is.EqualTo(expected));
        }

        [TestCase("Jensn","Jadams", 0.15384615384615385d )]
        [TestCase("Jensn","Adams", 0d )]
        [TestCase("Jensn", "Benson", 0.46153846153846156d)]
        [TestCase("Jensn", "Geralds", 0)]
        [TestCase("Jensn", "Johannson", 0.37500d)]
        [TestCase("Jensn", "Johnson", 0.42857142857142855d)]
        [TestCase("Jensn", "Jensen", 0.76923076923076927d)]
        [TestCase("aensn", "Jensen", 0.46153846153846156d)]
        [TestCase("Jensn", "Jordon", 0.30769230769230771d)]
        [TestCase("Jensn", "Madsen", 0.30769230769230771d)]
        [TestCase("Jensn", "Stratford", 0d )]
        [TestCase("Jensn", "Wilkins", 0.14285714285714285d)]
        [TestCase("2130 South Fort Union Blvd.", "2689 East Milkin Ave.", 0.16)]
        [TestCase("2130 South Fort Union Blvd.", "85 Morrison", 0.1)]
        [TestCase("2130 South Fort Union Blvd.", "2350 North Main", 0.272727272727273)]
        [TestCase("2130 South Fort Union Blvd.", "567 West Center Street", 0.0784313725490196)]
        [TestCase("2130 South Fort Union Blvd.", "2130 Fort Union Boulevard", 0.66666666666666663)]
        [TestCase("2130 South Fort Union Blvd.", "2310 S. Ft. Union Blvd.", 0.615384615384615)]
        [TestCase("2130 South Fort Union Blvd.", "98 West Fort Union", 0.425531914893617)]
        [TestCase("2130 South Fort Union Blvd.", "Rural Route 2 Box 29", 0.122448979591837)]
        [TestCase("2130 South Fort Union Blvd.", "PO Box 3487", 0.05)]
        [TestCase("2130 South Fort Union Blvd.", "3 Harvard Square", 0.0444444444444444)]
        public void DiceCoefficientTests(string input, string compareTo, double expected)
        {
            Assert.That(input.DiceCoefficient(compareTo), Is.EqualTo(expected).Within(0.0000000000000009));
            Assert.That( DiceCoefficientExtensions.DiceCoefficientAlternativeV2(input, compareTo), Is.EqualTo(expected).Within(0.0000000000000009));
        }

        [TestCase("Jensn", "Adams", 5)]
        [TestCase("Jensn", "Benson", 2)]
        [TestCase("Jensn", "Geralds", 6)]
        [TestCase("Jensn", "Johannson", 5)]
        [TestCase("Jensn", "Johnson", 3)]
        [TestCase("Jensn", "Jensen", 1)]
        [TestCase("Jensn", "Jordon", 4)]
        [TestCase("Jensn", "Madsen", 4)]
        [TestCase("Jensn", "Stratford", 9)]
        [TestCase("Jensn", "Wilkins", 6)]
        [TestCase("2130 South Fort Union Blvd.", "2689 East Milkin Ave.", 18)]
        [TestCase("2130 South Fort Union Blvd.", "85 Morrison", 22)]
        [TestCase("2130 South Fort Union Blvd.", "2350 North Main", 18)]
        [TestCase("2130 South Fort Union Blvd.", "567 West Center Street", 23)]
        [TestCase("2130 South Fort Union Blvd.", "2130 Fort Union Boulevard", 12)]
        [TestCase("2130 South Fort Union Blvd.", "98 West Fort Union", 14)]
        [TestCase("2130 South Fort Union Blvd.", "Rural Route 2 Box 29", 20)]
        [TestCase("2130 South Fort Union Blvd.", "PO Box 3487", 23)]
        [TestCase("2130 South Fort Union Blvd.", "3 Harvard Square", 23)]
        public void LevenshteinTests(string input, string compareTo, int expected)
        {
            Assert.That(input.LevenshteinDistance(compareTo), Is.EqualTo(expected));
            Assert.That(input.LevenshteinDistanceUncachedAlternativeV2(compareTo), Is.EqualTo(expected),"LevenshteinDistanceUncachedAlternativeV2");
        }

        [TestCase("Jensn", "Adams", 5)]
        [TestCase("Jensn", "Benson", 2)]
        [TestCase("Jensn", "Geralds", 6)]
        [TestCase("Jensn", "Johannson", 5)]
        [TestCase("Jensn", "Johnson", 3)]
        [TestCase("Jensn", "Jensen", 1)]
        [TestCase("Jensn", "Jordon", 4)]
        [TestCase("Jensn", "Madsen", 4)]
        [TestCase("Jensn", "Stratford", 9)]
        [TestCase("Jensn", "Wilkins", 6)]
        [TestCase("2130 South Fort Union Blvd.", "2689 East Milkin Ave.", 18)]
        [TestCase("2130 South Fort Union Blvd.", "85 Morrison", 22)]
        [TestCase("2130 South Fort Union Blvd.", "2350 North Main", 18)]
        [TestCase("2130 South Fort Union Blvd.", "567 West Center Street", 23)]
        [TestCase("2130 South Fort Union Blvd.", "2130 Fort Union Boulevard", 12)]
        [TestCase("2130 South Fort Union Blvd.", "2310 S. Ft. Union Blvd.", 9)] //This case returns 8 in the old algorithm, seems like a bug in the old algorithm.
        [TestCase("2130 South Fort Union Blvd.", "98 West Fort Union", 14)]
        [TestCase("2130 South Fort Union Blvd.", "Rural Route 2 Box 29", 20)]
        [TestCase("2130 South Fort Union Blvd.", "PO Box 3487", 23)]
        [TestCase("2130 South Fort Union Blvd.", "3 Harvard Square", 23)]
        public void LevenshteinDistanceUncachedAlternativeV2Tests(string input, string compareTo, int expected)
        {
            Assert.That(input.LevenshteinDistanceUncachedAlternativeV2(compareTo), Is.EqualTo(expected),"LevenshteinDistanceUncachedAlternativeV2");
        }

        [TestCase("beast of burden's power and toughness are each equal to the number of creatures on the battlefield", "Beast of Burden","", "'s power and toughness are each equal to the number of creatures on the battlefield")]
        [TestCase("battlefield", "B","C", "Cattlefield")]
        public void ReplaceStringTests(string input, string replace, string replaceWith, string expected)
        {
            Assert.That(input.ToLowerInvariant().Replace(replace.ToLowerInvariant(),replaceWith),Is.EqualTo(expected));
        }

        [TestCase("{B}{G}", "B", true)]
        [TestCase("{G}", "G", true)]
        [TestCase("{G}", "B", false)]
        [TestCase("", "B", false)]
        [TestCase("", "", true)]
        public void AnyStringTests(string input, string stringToFind, bool expectedResult)
        {
            Assert.That(input.AnyString(stringToFind), Is.EqualTo(expectedResult));
        }

        [TestCase("{B}{G}", "B", 1, true)]
        [TestCase("{G}", "G", 1, true)]
        [TestCase("{G}", "B", 1, false)]
        [TestCase("", "B", 0, false)]
        [TestCase("", "", 0, true)]
        [TestCase("A string original", "original", 0, false)] //NOTE: this differs with the original implementation (which I consider a bug)
        [TestCase("very much", "very much longer and longer", 0, false)]
        [TestCase("very much longer", "very much longer and longer", 5, false)]
        public void StringAtOneParamTests(string input, string stringAt, int atIndex, bool expectedResult)
        {
            Assert.That(DoubleMetaphoneExtensions.StringAt(input, atIndex, stringAt), Is.EqualTo(expectedResult), "StringAt");
            Assert.That(DoubleMetaphoneExtensions.StringAtOld(input, atIndex, stringAt, stringAt), Is.EqualTo(expectedResult), "StringAtBoth");
        }

        [TestCase("{B}{G}", "B", "", 1, true)]
        [TestCase("{G}", "G", "", 1, true)]
        [TestCase("{G}", "B", "", 1, false)]
        [TestCase("", "B", "", 0, true)]
        [TestCase("", "", "", 0, true)]
        [TestCase("very much", "very much longer and longer", "", 0, false)]
        [TestCase("very much longer", "very much longer and longer", "", 5, false)]
        [TestCase("POBOX", "B", "P", 1, false)]
        [TestCase("POBOX", "B", "P", 2, true)]
        [TestCase("POBOX", "WR", "PS", 0, false)]
        [TestCase("PSORELLO", "WR", "PS", 0, true)]
        public void StringAtTwoParamTests(string input, string stringAt, string stringAtB, int atIndex, bool expectedResult)
        {
            Assert.That(DoubleMetaphoneExtensions.StringAtOld(input, atIndex, stringAt, stringAtB), Is.EqualTo(expectedResult), "StringAltAtLeft");
            Assert.That(DoubleMetaphoneExtensions.StringAtOld(input, atIndex, stringAtB, stringAt), Is.EqualTo(expectedResult), "StringAltAtRight");

            Assert.That(DoubleMetaphoneExtensions.StringAt(input, atIndex, stringAt, stringAtB), Is.EqualTo(expectedResult), "StringAtLeft");
            Assert.That(DoubleMetaphoneExtensions.StringAt(input, atIndex, stringAtB, stringAt), Is.EqualTo(expectedResult), "StringAtRight");
        }

        [TestCase("Jensn", "ANSN")]
        [TestCase("Adams", "ATMS")]
        [TestCase("Geralds", "JRLT")]
        [TestCase("Johannson", "AHNS")]
        [TestCase("Johnson", "ANSN")]
        [TestCase("Jensen", "ANSN")]
        [TestCase("Jordon", "ARTN")]
        [TestCase("Madsen", "MTSN")]
        [TestCase("Stratford", "STRT")]
        [TestCase("Wilkins", "FLKN")]
        [TestCase("2689 East Milkin Ave.", "STML")]
        [TestCase("85 Morrison", "MRSN")]
        [TestCase("2350 North Main", "NRTM")]
        [TestCase("567 West Center Street", "STSN")]
        [TestCase("2130 Fort Union Boulevard", "FRTN")]
        [TestCase("2310 S. Ft. Union Blvd.", "SFTN")]
        [TestCase("98 West Fort Union", "STFR")]
        [TestCase("Rural Route 2 Box 29", "RRLR")]
        [TestCase("PO Box 3487", "PPKS")]
        [TestCase("3 Harvard Square", "RFRT")]
        [TestCase("Grizzled Angler", "KRSL")]
        [TestCase("Gallegos", "KKS")]
        [TestCase("Galegas", "KLKS")]
        [TestCase("cabrillo", "KPR")]
        [TestCase("Witzsche", "FFXX")]
        [TestCase("Wiklamab", "FKLM")]
        [TestCase("wiklamab", "FKLM")]
        [TestCase("Acze", "AX")]
        [TestCase("Gnollero", "NLR")]
        [TestCase("Psollero", "SLR")]
        [TestCase("Honorificabilitudinitatibus", "HNRF")]
        [TestCase("SCH", "S")]
        [TestCase("breaux", "PR")]
        [TestCase("BOX", "PKS")]
        [TestCase("PO B", "PP")]
        public void ToDoubleMetaPhoneTests(string input, string expected)
        {
            Assert.That(DoubleMetaphoneExtensions.ToDoubleMetaphoneStr(input,false), Is.EqualTo(expected));
        }

        [TestCase("Spotify","Spotfy","Sputfi", "Spotifi")]
        [TestCase( "United Air Lines", "United Aire Lines", "Unitid Air Line")]
        public void ToDoubleMetaPhoneSimilarTests(params string[] resultsInSame)
        {
            var previous = resultsInSame[0].ToDoubleMetaphone();
            for (int i = 1; i < resultsInSame.Length; i++)
            {
                var current = resultsInSame[i].ToDoubleMetaphone();
                Assert.That(current, Is.EqualTo(previous));
                previous = current;
            }
        }

        [TestCase("Jensn", "Adams", 0.04000d,"s")]
        [TestCase("Adams", "Jensn", 0.04000d,"s")]
        [TestCase("Jensn", "Benson", 0.33333d, "ensn")]
        [TestCase("Jensn", "Geralds", 0.05714d, "es")]
        [TestCase("Jensn", "Johannson", 0.08889d,"jnsn")]
        [TestCase("Jensn", "Johnson", 0.17143d, "jnsn")]
        [TestCase("Jensn", "Jensen", 0.56667d, "jensn")]
        [TestCase("Jensn", "Jordon", 0.06667d, "jn")]
        [TestCase("Jensn", "Madsen", 0.13333d,"en")]
        [TestCase("Jensn", "Stratford", 0.02222d, "s")]
        [TestCase("Jensn", "Wilkins", 0.11429d, "ns")]
        [TestCase("2130 South Fort Union Blvd.", "2689 East Milkin Ave.", 0.0211640211640212, "2 st in v.")]
        [TestCase("2689 East Milkin Ave.", "2130 South Fort Union Blvd.", 0.0211640211640212, "2 st in v.")]
        [TestCase("2130 South Fort Union Blvd.", "85 Morrison", 0.0202020202020202, " son")]
        [TestCase("2130 South Fort Union Blvd.", "2350 North Main", 0.0444444444444444, "230 oth in")]
        [TestCase("2130 South Fort Union Blvd.", "567 West Center Street", 0.0101010101010101, " st t ")]
        [TestCase("2130 South Fort Union Blvd.", "2130 Fort Union Boulevard", 0.254814814814815, "2130 fort union blvd")]
        [TestCase("2130 South Fort Union Blvd.", "2310 S. Ft. Union Blvd.", 0.257648953301127, "230 s ft union blvd.")]
        [TestCase("2130 South Fort Union Blvd.", "98 West Fort Union", 0.255144032921811, " st fort union")]
        [TestCase("2130 South Fort Union Blvd.", "Rural Route 2 Box 29", 0.0259259259259259, " out  o ")]
        [TestCase("2130 South Fort Union Blvd.", "PO Box 3487", 0.0134680134680135, "o o ")]
        [TestCase("2130 South Fort Union Blvd.", "3 Harvard Square", 0.0138888888888889, "3 hrvd")]
        public void LongestCommonSubSequenceTests(string input, string compareTo, double expectedDouble, string expectedSequence)
        {
            LongestCommonSubsequenceResult result = input.LongestCommonSubsequence(compareTo);
            Assert.That(result.Coefficient, Is.EqualTo(expectedDouble).Within(0.000009));
            Assert.That(input.LongestCommonSubsequence(compareTo,false,false).Coefficient, Is.EqualTo(expectedDouble).Within(0.000009));
            Assert.That(result.LongestSubsequence.ToUpperInvariant(), Is.EqualTo(expectedSequence.ToUpperInvariant()));
        }

        [TestCase("Jensn", "Adams", 0.04000d,"s")]
        [TestCase("Adams", "Jensn", 0.04000d,"s")]
        [TestCase("Jensn", "Benson", 0.33333d, "ensn")]
        [TestCase("Jensn", "Geralds", 0.05714d, "es")]
        [TestCase("Jensn", "Johannson", 0.08889d,"jnsn")]
        [TestCase("Jensn", "Johnson", 0.17143d, "jnsn")]
        [TestCase("Jensn", "Jensen", 0.56667d, "jensn")]
        [TestCase("Jensn", "Jordon", 0.06667d, "jn")]
        [TestCase("Jensn", "Madsen", 0.13333d,"en")]
        [TestCase("Jensn", "Stratford", 0.02222d, "s")]
        [TestCase("Jensn", "Wilkins", 0.11429d, "ns")]
        [TestCase("2130 South Fort Union Blvd.", "2689 East Milkin Ave.", 0.0211640211640212, "2 st in v.")]
        [TestCase("2689 East Milkin Ave.", "2130 South Fort Union Blvd.", 0.0211640211640212, "2 st in v.")]
        [TestCase("2130 South Fort Union Blvd.", "85 Morrison", 0.0202020202020202, " son")]
        [TestCase("2130 South Fort Union Blvd.", "2350 North Main", 0.0444444444444444, "230 oth in")]
        [TestCase("2130 South Fort Union Blvd.", "567 West Center Street", 0.0101010101010101, " st t ")]
        [TestCase("2130 South Fort Union Blvd.", "2130 Fort Union Boulevard", 0.254814814814815, "2130 fort union blvd")]
        [TestCase("2130 South Fort Union Blvd.", "2310 S. Ft. Union Blvd.", 0.257648953301127, "230 s ft union blvd.")]
        [TestCase("2130 South Fort Union Blvd.", "98 West Fort Union", 0.255144032921811, " st fort union")]
        [TestCase("2130 South Fort Union Blvd.", "Rural Route 2 Box 29", 0.0259259259259259, " out  o ")]
        [TestCase("2130 South Fort Union Blvd.", "PO Box 3487", 0.0134680134680135, "o o ")]
        [TestCase("2130 South Fort Union Blvd.", "3 Harvard Square", 0.0138888888888889, "3 hrvd")]
        public void LongestCommonSubSequenceUncachedNoBackTrackingTests(string input, string compareTo, double expectedDouble, string expectedSequence)
        {
            LongestCommonSubsequenceResult result = input.LongestCommonSubsequenceUncached(compareTo, false, false);
            Assert.That(result.Coefficient, Is.EqualTo(expectedDouble).Within(0.000009));
            Assert.That(input.LongestCommonSubsequence(compareTo,false,true).Coefficient, Is.EqualTo(expectedDouble).Within(0.000009));
            Assert.That(input.LongestCommonSubsequenceWithoutSubsequenceAlternative(compareTo,false).Coefficient, Is.EqualTo(expectedDouble).Within(0.000009));
        }

        [TestCase("Jensn", "Adams", false)]
        [TestCase("Jensn", "Benson", false)]
        [TestCase("Jensn", "Geralds", false)]
        [TestCase("Jensn", "Johannson", false)]
        [TestCase("Jensn", "Johnson", false)]
        [TestCase("Jensn", "Jensen", true)]
        [TestCase("Jensn", "Jordon", false)]
        [TestCase("Jensn", "Madsen", false)]
        [TestCase("Jensn", "Stratford", false)]
        [TestCase("Jensn", "Wilkins", false)]
        [TestCase("2130 South Fort Union Blvd.", "2689 East Milkin Ave.", false)]
        [TestCase("2130 South Fort Union Blvd.", "85 Morrison", false)]
        [TestCase("2130 South Fort Union Blvd.", "2350 North Main", false)]
        [TestCase("2130 South Fort Union Blvd.", "567 West Center Street", false)]
        [TestCase("2130 South Fort Union Blvd.", "2130 Fort Union Boulevard", true)]
        [TestCase("2130 South Fort Union Blvd.", "2310 S. Ft. Union Blvd.", true)]
        [TestCase("2130 South Fort Union Blvd.", "98 West Fort Union", false)]
        [TestCase("2130 South Fort Union Blvd.", "Rural Route 2 Box 29", false)]
        [TestCase("2130 South Fort Union Blvd.", "PO Box 3487", false)]
        [TestCase("2130 South Fort Union Blvd.", "3 Harvard Square", false)]
        public void FuzzyEqualsTests(string input, string compareTo, bool expected)
        {
            Assert.That(input.FuzzyEquals(compareTo, caseSensitive: false), Is.EqualTo(expected));
        }

        [TestCase("Flying (This creature can't be blocked", "Flying (This creature can't be blocked", 0.99999899999999997d)]//equal strings
        [TestCase("Jensn", "Adams", 0.13202380952381d)]
        [TestCase("Jensn", "Benson", 0.499854312354312d)]
        [TestCase("Jensn", "Geralds", 0.0623626373626374)]
        [TestCase("Jensn", "Johannson", 0.226549145299145)]
        [TestCase("Jensn", "Johnson", 0.478125)]
        [TestCase("Jensn", "Jensen", 0.792307692307692)]
        [TestCase("Jensn", "Jordon", 0.278113553113553)]
        [TestCase("Jensn", "Madsen", 0.29478021978022)]
        [TestCase("Jensn", "Stratford", 0.0360433604336043)]
        [TestCase("Jensn", "Wilkins", 0.167108294930876)]
        [TestCase("2130 South Fort Union Blvd.", "2689 East Milkin Ave.", 0.113785006660007)]
        [TestCase("2130 South Fort Union Blvd.", "85 Morrison", 0.0744070069715231)]
        [TestCase("2130 South Fort Union Blvd.", "2350 North Main", 0.179121212121212)]
        [TestCase("2130 South Fort Union Blvd.", "567 West Center Street", 0.0663653846153846)]
        [TestCase("2130 South Fort Union Blvd.", "2130 Fort Union Boulevard", 0.983428418803419)]
        [TestCase("2130 South Fort Union Blvd.", "2310 S. Ft. Union Blvd.", 0.915673701298701)]
        [TestCase("2130 South Fort Union Blvd.", "98 West Fort Union", 0.73525)]
        [TestCase("2130 South Fort Union Blvd.", "Rural Route 2 Box 29", 0.172657287157287)]
        [TestCase("2130 South Fort Union Blvd.", "PO Box 3487", 0.0783005050505051)]
        [TestCase("2130 South Fort Union Blvd.", "3 Harvard Square", 0.0769340659340659)]
        [TestCase("henk", "henk verhoeven", 0.19593837535014005d)]
        [TestCase("henk", "henk", 0.99999899999999997)]
        [TestCase("henk", "appel", 0.072023809523809518d)]
        [TestCase("Flying (This creature can't be blocked except by creatures with flying or reach.) Vigilance (Attacking doesn't cause this creature to tap.)",
            "Firebolt deals 2 damage to target creature or player. Flashback {4}{R} (You may cast this card from your graveyard for its flashback cost. Then exile it.)",
             0.45195413898026665)]
        [TestCase("Sacrifice a Goblin creature: Airdrop Condor deals damage equal to the sacrificed creature's power to target creature or player.",
            "Put a white Avatar creature token onto the battlefield. It has \"This creature's power and toughness are each equal to your life total.\"",
             0.72955237915808924d)]
        [TestCase("Lightning Bolt deals 3 damage to target creature or player.",
            "Firebolt deals 2 damage to target creature or player. Flashback {4}{R} (You may cast this card from your graveyard for its flashback cost. Then exile it.)",
             0.99999899999999997)]
        [TestCase("Firebolt deals 2 damage to target creature or player. Flashback {4}{R} (You may cast this card from your graveyard for its flashback cost. Then exile it.)", "Lightning Bolt deals 3 damage to target creature or player.",

            0.60841197203094644d)]
        public void FuzzyMatchTests(string input, string compareTo, double expected)
        {
            Assert.That(input.FuzzyMatch(compareTo, false), Is.EqualTo(expected).Within(0.1159));
        }


        [TestCase("FLYING (THIS CREATURE CAN'T BE BLOCKED", "FLYING (THIS CREATURE CAN'T BE BLOCKED", 0.99999899999999997d)]//equal strings
        [TestCase("JENSN", "ADAMS", 0.13202380952381d)]
        [TestCase("JENSN", "BENSON", 0.499854312354312d)]
        [TestCase("JENSN", "GERALDS", 0.0623626373626374)]
        [TestCase("JENSN", "JOHANNSON", 0.226549145299145)]
        [TestCase("JENSN", "JOHNSON", 0.478125)]
        [TestCase("JENSN", "JENSEN", 0.792307692307692)]
        [TestCase("JENSN", "JORDON", 0.278113553113553)]
        [TestCase("JENSN", "MADSEN", 0.29478021978022)]
        [TestCase("JENSN", "STRATFORD", 0.0360433604336043)]
        [TestCase("JENSN", "WILKINS", 0.167108294930876)]

        [TestCase("FLYING (THIS CREATURE CAN'T BE BLOCKED EXCEPT BY CREATURES WITH FLYING OR REACH.) VIGILANCE (ATTACKING DOESN'T CAUSE THIS CREATURE TO TAP.)",
            "FIREBOLT DEALS 2 DAMAGE TO TARGET CREATURE OR PLAYER. FLASHBACK {4}{R} (YOU MAY CAST THIS CARD FROM YOUR GRAVEYARD FOR ITS FLASHBACK COST. THEN EXILE IT.)",
             0.45195413898026665)]
        [TestCase("SACRIFICE A GOBLIN CREATURE: AIRDROP CONDOR DEALS DAMAGE EQUAL TO THE SACRIFICED CREATURE'S POWER TO TARGET CREATURE OR PLAYER.",
            "PUT A WHITE AVATAR CREATURE TOKEN ONTO THE BATTLEFIELD. IT HAS \"THIS CREATURE'S POWER AND TOUGHNESS ARE EACH EQUAL TO YOUR LIFE TOTAL.\"",
             0.72955237915808924d)]
        [TestCase("LIGHTNING BOLT DEALS 3 DAMAGE TO TARGET CREATURE OR PLAYER.",
            "FIREBOLT DEALS 2 DAMAGE TO TARGET CREATURE OR PLAYER. FLASHBACK {4}{R} (YOU MAY CAST THIS CARD FROM YOUR GRAVEYARD FOR ITS FLASHBACK COST. THEN EXILE IT.)",
             0.99999899999999997)]
        [TestCase("FIREBOLT DEALS 2 DAMAGE TO TARGET CREATURE OR PLAYER. FLASHBACK {4}{R} (YOU MAY CAST THIS CARD FROM YOUR GRAVEYARD FOR ITS FLASHBACK COST. THEN EXILE IT.)", "LIGHTNING BOLT DEALS 3 DAMAGE TO TARGET CREATURE OR PLAYER.",

            0.60841197203094644d)]
        public void FuzzyMatchAndFuzzyMatchAlreadyUpperCasedShouldHaveSameResultOnSameInput(string input, string compareTo, double expected)
        {
            double standardMatchCaseInsensitive = input.FuzzyMatch(compareTo, false);
            double matchAlreadyUpperCased = input.FuzzyMatchAlreadyUpperCasedStrings(compareTo);

            Assert.That(matchAlreadyUpperCased,
                Is.EqualTo(standardMatchCaseInsensitive).And.EqualTo(expected).Within(0.1159));
        }

        [Test]
        [TestCase("Sacrifice a Goblin creature: Airdrop Condor deals damage equal to the sacrificed creature's power to target creature or player.",
            "Put a white Avatar creature token onto the battlefield. It has \"This creature's power and toughness are each equal to your life total.\"",
              0.011210762331838564d)]
        [TestCase("a creature", "creature a", 0.23809523809523808d)]
        [TestCase("creature", "creature", 5d)]
        [TestCase("deals 3 damage to target creature or player.",
            "deals 2 damage to target creature or player.",
             0.83333333333333337)]
        public void CalculateLevenCoefficient(string input, string compareTo, double expected)
        {
            Assert.That(StringExtensions.CalculateLevenshteinDistanceCoefficientForCompositeCoefficient(input,compareTo),Is.EqualTo(expected));
        }

        [Theory]
        [TestCase("kitten", "sitting", 3)]
        [TestCase("78135", "75130", 2)]
        [TestCase("78135x", "75130x", 2)]
        public void LevenshteinDistancePreciseTests(string input, string match, int expectedResult)
        {
            var result = input.LevenshteinDistance(match);
            var msg = $"LevenshteinDistance of \"{match}\" against \"{input}\" was {result}, expecting {expectedResult}.";

            Assert.That(expectedResult, Is.EqualTo(result).Within(double.Epsilon), msg);
        }

        [Theory]
        [TestCase("kitten", "sitting", false)]
        [TestCase("kitten", "", false)]
        [TestCase("kitten ", " ", true)]
        [TestCase("", " ", false)]
        [TestCase("", "", false)]
        [TestCase("a creature", "creature a", false)]
        [TestCase("creature", "creature", true)]
        [TestCase("creature", "creatures", false)]
        [TestCase("creature", "crea",true)]
        [TestCase("DOCTOR", "CREATU",false)]
        [TestCase("DOCTOR", "DOCTOR",true)]
        [TestCase("deals 3 damage to target creature or player.",
            "to target creature or player.",
            true)]
        [Ignore("No alternative contains algorithm. enable to test")]
        public void StringContainsTest(string a, string b, bool expectedResult)
        {
            //TODO: Change this to alternative Algorithm when needed
            Assert.That(a.Contains(b), Is.EqualTo(expectedResult));
        }


        [TestCase(1,  2, 3,  1)]
        [TestCase(3,  2, 1,  1)]
        [TestCase(3,  1, 2,  1)]
        [TestCase(3,  3, 2,  2)]
        [TestCase(2,  3, 3,  2)]
        [TestCase(3,  2, 3,  2)]
        [TestCase(0,  0, 0,  0)]
        [TestCase(0,  0, -1, -1)]
        [TestCase(0,  0, -1, -1)]
        [TestCase(-1, 0, -1, -1)]
        [TestCase(1,  0, -1, -1)]
        [TestCase(1,  1, 1,  1)]
        public void LevenshteinDistanceExtensions_FindMinimumOptimized(int a, int b, int c, int expected)
        {
            Assert.That(LevenshteinDistanceExtensions.Min(a, b, c), Is.EqualTo(expected));
        }
    }
}
