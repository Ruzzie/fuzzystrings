using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DuoVia.FuzzyStrings
{
    public static class StringExtensions
    {
        private const string Space = " ";
        private static readonly Regex StripRegex = new Regex(@"[^a-zA-Z0-9 -]*");

        public static bool FuzzyEquals(this string strA, string strB, double requiredProbabilityScore = 0.75)
        {
            return strA.FuzzyMatch(strB) > requiredProbabilityScore;
        }

        public static double FuzzyMatch(this string strA, string strB)
        {
            string localA = Strip(strA.Trim().ToLower());
            string localB = Strip(strB.Trim().ToLower());
            if (localA.ContainsCustom(Space) && localB.ContainsCustom(Space))
            {
                var partsA = localA.Split(' ');
                var partsB = localB.Split(' ');
                var weightedHighCoefficients = new double[partsA.Length];
                for (int i = 0; i < partsA.Length; i++)
                {
                    double high = 0.0;
                    int indexDistance = 0;
                    for (int x = 0; x < partsB.Length; x++)
                    {
                        var coef = CompositeCoefficient(partsA[i], partsB[x]);
                        if (coef > high)
                        {
                            high = coef;
                            indexDistance = Math.Abs(i - x);
                        }
                    }
                    double distanceWeight = indexDistance == 0 ? 1.0 : 1.0 - (indexDistance/(partsA.Length*1.0));
                    weightedHighCoefficients[i] = high*distanceWeight;
                }
                double avgWeightedHighCoefficient = weightedHighCoefficients.Sum()/(partsA.Length*1.0);
                return avgWeightedHighCoefficient < 0.999999 ? avgWeightedHighCoefficient : 0.999999; //fudge factor
            }
            var singleComposite = CompositeCoefficient(localA, localB);
            return singleComposite < 0.999999 ? singleComposite : 0.999999; //fudge factor
        }

        private static string Strip(string str)
        {
            return StripRegex.Replace(str, string.Empty);
        }

        public static bool ContainsCustom(this string inputString, string value)
        {
            string input = inputString;
            string stringToFind = value;

            return ((input.Length - input.Replace(stringToFind, string.Empty).Length) / stringToFind.Length > 0 ? true : false);

        }
        private static double CompositeCoefficient(string strA, string strB)
        {
            double dice = strA.DiceCoefficient(strB);
            var lcs = strA.LongestCommonSubsequence(strB);
            double levenCoefficient = CalculateLevenCoefficientForCompositeCoefficient(strA, strB);//may want to tweak offset
            string strAMp = strA.ToDoubleMetaphone();
            string strBMp = strB.ToDoubleMetaphone();
            int matchCount = 0;
            if (strAMp.Length == 4 && strBMp.Length == 4)
            {
                for (int i = 0; i < strAMp.Length; i++)
                {
                    if (strAMp[i] == strBMp[i])
                    {
                        matchCount++;
                    }
                }
            }
            double mpCoefficient = matchCount == 0 ? 0.0 : matchCount / 4.0;
            double avgCoefficent = (dice + lcs.Item2 + levenCoefficient + mpCoefficient) / 4.0;
            return avgCoefficent;
        }

        public static double CalculateLevenCoefficientForCompositeCoefficient(string strA, string strB)
        {
            int leven = strA.LevenshteinDistance(strB);
            double levenCoefficient = 1.0 / (1.0 * (leven + 0.2));
            return levenCoefficient;
        }
    }
}