﻿using DuoVia.FuzzyStrings;
using NUnit.Framework;

namespace fuzzytest
{
    [TestFixture]
    public class Tests
    {      

        [Test]
        [TestCase("test","w",0.078125d)]
        [TestCase("test","W",0.078125d)]
        [TestCase("test","w ",0.078125d)]
        [TestCase("test","W ",0.078125d)]
        [TestCase("test"," w",0.078125d)]
        [TestCase("test"," W",0.078125d)]
        [TestCase("test"," w ",0.078125d)]
        [TestCase("test"," W ",0.078125d)]
        public void IssueTest(string input, string compareTo, double expected)
        {
            Assert.That(input.FuzzyMatch(compareTo),Is.EqualTo(expected));          
        }

        [TestCase("Jensn","Adams", 0d )]
        [TestCase("Jensn", "Benson", 0.46153846153846156d)]
        [TestCase("Jensn", "Geralds", 0)]
        [TestCase("Jensn", "Johannson", 0.37500d)]
        [TestCase("Jensn", "Johnson", 0.42857142857142855d)]
        [TestCase("Jensn", "Jensen", 0.76923076923076927d)]
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
        }

        [TestCase("Jensn", "Adams", 4)]
        [TestCase("Jensn", "Benson", 2)]
        [TestCase("Jensn", "Geralds", 5)]
        [TestCase("Jensn", "Johannson", 5)]
        [TestCase("Jensn", "Johnson", 3)]
        [TestCase("Jensn", "Jensen", 1)]
        [TestCase("Jensn", "Jordon", 4)]
        [TestCase("Jensn", "Madsen", 4)]
        [TestCase("Jensn", "Stratford", 8)]
        [TestCase("Jensn", "Wilkins", 6)]
        [TestCase("2130 South Fort Union Blvd.", "2689 East Milkin Ave.", 18)]
        [TestCase("2130 South Fort Union Blvd.", "85 Morrison", 22)]
        [TestCase("2130 South Fort Union Blvd.", "2350 North Main", 18)]
        [TestCase("2130 South Fort Union Blvd.", "567 West Center Street", 22)]
        [TestCase("2130 South Fort Union Blvd.", "2130 Fort Union Boulevard", 11)]
        [TestCase("2130 South Fort Union Blvd.", "2310 S. Ft. Union Blvd.", 8)]
        [TestCase("2130 South Fort Union Blvd.", "98 West Fort Union", 14)]
        [TestCase("2130 South Fort Union Blvd.", "Rural Route 2 Box 29", 19)]
        [TestCase("2130 South Fort Union Blvd.", "PO Box 3487", 22)]
        [TestCase("2130 South Fort Union Blvd.", "3 Harvard Square", 22)]
        public void LevenshteinTests(string input, string compareTo, int expected)
        {
            Assert.That(input.LevenshteinDistance(compareTo), Is.EqualTo(expected));
        }

        [TestCase("Jensn", "ANSN")]
        [TestCase("Adams", "ATMS")]
        [TestCase("Geralds", "JRLT")]
        [TestCase("Johannson", "AHNS")]
        [TestCase("Johnson", "ANSN")]
        [TestCase("Jensen", "ANSN")]
        [TestCase("Jordon", "ARTN")]
        [TestCase("Madsen", "MTSN")]
        [TestCase("Stratford", "STTR")]
        [TestCase("Wilkins", "FLKN")]
        [TestCase("2689 East Milkin Ave.", "STML")]
        [TestCase("85 Morrison", "MRSN")]
        [TestCase("2350 North Main", "NRTM")]
        [TestCase("567 West Center Street", "SSNT")]
        [TestCase("2130 Fort Union Boulevard", "FRTN")]
        [TestCase("2310 S. Ft. Union Blvd.", "SFTN")]
        [TestCase("98 West Fort Union", "STFR")]
        [TestCase("Rural Route 2 Box 29", "RRLR")]
        [TestCase("PO Box 3487", "PPKS")]
        [TestCase("3 Harvard Square", "RFRT")]
        public void ToDoubleMetaPhoneTests(string input, string expected)
        {
            Assert.That(input.ToDoubleMetaphone(), Is.EqualTo(expected));
        }

        [TestCase("Jensn", "Adams", 0.04000d,"s")]
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
            Assert.That(input.LongestCommonSubsequence(compareTo).Item2, Is.EqualTo(expectedDouble).Within(0.000009));
            Assert.That(input.LongestCommonSubsequence(compareTo).Item1, Is.EqualTo(expectedSequence));
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
            Assert.That(input.FuzzyEquals(compareTo),Is.EqualTo(expected));
        }

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
            Assert.That(input.FuzzyMatch(compareTo), Is.EqualTo(expected).Within(0.0000000000000009));
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
            Assert.That(StringExtensions.CalculateLevenCoefficientForCompositeCoefficient(input,compareTo),Is.EqualTo(expected));
        }     
    }
}