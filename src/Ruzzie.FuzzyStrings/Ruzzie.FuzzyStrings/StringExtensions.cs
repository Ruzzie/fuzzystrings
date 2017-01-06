using System;
using System.Text;
using System.Text.RegularExpressions;
using Ruzzie.Caching;

namespace Ruzzie.FuzzyStrings
{
    public static class StringExtensions
    {
        private static readonly IFixedSizeCache<string, double> Cache =
            new FlashCache<string, double>(InternalVariables.MaxCacheSizeInMb, InternalVariables.StringComparerForCacheKey,
                InternalVariables.AverageStringSizeInBytes);

        private const string Space = " ";
        private static readonly Regex StripRegex = new Regex(@"[^a-zA-Z0-9 -]*",RegexOptions.Compiled);

        public static bool FuzzyEquals(this string strA, string strB, double requiredProbabilityScore = 0.75, bool caseSensitive = true)
        {
            return strA.FuzzyMatch(strB, caseSensitive) > requiredProbabilityScore;
        }

        public static double FuzzyMatch(this string strA, string strB, bool caseSensitive = true)
        {
            return Cache.GetOrAdd(string.Concat(strA, strB, caseSensitive), key => strA.FuzzyMatchUncached(strB, caseSensitive));
        }

        public static double FuzzyMatchUncached(this string strA, string strB, bool caseSensitive = true)
        {
            string localA = StripAlternative(strA.Trim());
            string localB = StripAlternative(strB.Trim());
            if (!caseSensitive)
            {                
                if (string.Equals(localA, localB, StringComparison.OrdinalIgnoreCase))
                {
                    return 0.99999899999999997d;
                }

                localA = localA.ToUpperInvariant();
                localB = localB.ToUpperInvariant();
            }
            else
            {              
                if (string.Equals(localA, localB, StringComparison.Ordinal))
                {
                    return 0.99999899999999997d;
                }
            }

            if (localA.ContainsString(Space) && localB.ContainsString(Space))
            {
                var partsA = localA.Split(' ');
                var partsB = localB.Split(' ');
                int partsAlength = partsA.Length;
                double weightedHighCoefficientsSum = 0;

                for (int i = 0; i < partsAlength; i++)
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
                    double distanceWeight = indexDistance == 0 ? 1.0 : 1.0 - (indexDistance/((double) partsAlength));
                    weightedHighCoefficientsSum += high*distanceWeight;
                }
                double avgWeightedHighCoefficient = weightedHighCoefficientsSum / partsAlength;
                return avgWeightedHighCoefficient < 0.999999 ? avgWeightedHighCoefficient : 0.999999; //fudge factor
            }
            var singleComposite = CompositeCoefficient(localA, localB);
            return singleComposite < 0.999999 ? singleComposite : 0.999999; //fudge factor
        }

        public static string Strip(string str)
        {            
            return StripRegex.Replace(str, string.Empty);
        }

        /// <summary>
        /// Strips string of characters that are not [^a-zA-Z0-9 -]*
        /// </summary>
        /// <param name="str">The string to strip</param>
        /// <returns>The stripped string</returns>
        public static string StripAlternative(string str)
        {
            int length = str.Length;
            StringBuilder b = new StringBuilder(length);
            
            for (int i = 0; i < length; ++i)
            {
                char c = str[i];
                // ReSharper disable RedundantCast
                if (97 <= (int) c && (int) c <= 122)//a-z
                {
                    b.Append(c);
                } else if (65 <= (int) c && (int) c <= 90) //A-Z
                {
                    b.Append(c);
                } else if (48 <= (int) c && (int) c <= 57) //0-9
                {
                    b.Append(c);
                }
                else if (32 == (int) c || 45 == (int)c)//space, -
                {
                    b.Append(c);
                }
                // ReSharper restore RedundantCast
            }
            return b.ToString();
        }
      

        public static bool ContainsString(this string input, string stringToFind)
        {
            return input.Contains(stringToFind);
           // return input.IndexOf(stringToFind, comparison) != -1; //input.Contains(stringToFind);
           // return ((input.Length - input.ReplaceString(stringToFind, String.Empty).Length)/stringToFind.Length > 0);
        }

        public static bool AnyString(this string input, string stringToFind, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
             return input.IndexOf(stringToFind,0, comparison) != -1; //input.Contains(stringToFind);
            // return ((input.Length - input.ReplaceString(stringToFind, String.Empty).Length)/stringToFind.Length > 0);
        }

        private static double CompositeCoefficient(string strA, string strB, bool caseSensitive = true)
        {
            double dice = strA.DiceCoefficient(strB);
            var lcs = strA.LongestCommonSubsequence(strB, caseSensitive, false);
            double levenCoefficient = CalculateLevenCoefficientForCompositeCoefficient(strA, strB, caseSensitive);//may want to tweak offset

            string strAMp = strA.ToDoubleMetaphone();
            string strBMp = strB.ToDoubleMetaphone();
            int matchCount = 0;
            int strAMpLength = strAMp.Length;
            if (strAMpLength == 4 && strBMp.Length == 4)
            {
                for (int i = 0; i < strAMpLength; i++)
                {
                    if (strAMp[i] == strBMp[i])
                    {
                        ++matchCount;
                    }
                }
            }
            double mpCoefficient = matchCount == 0 ? 0.0 : matchCount / 4.0;
            double avgCoefficent = (dice + lcs.Coeffecient + levenCoefficient + mpCoefficient) / 4.0;
            return avgCoefficent;
        }

        public static double CalculateLevenCoefficientForCompositeCoefficient(string strA, string strB, bool caseSensitive = false)
        {
            int leven = strA.LevenshteinDistance(strB, caseSensitive);
            double levenCoefficient = 1.0 / (1.0 * (leven + 0.2));
            return levenCoefficient;
        }
       
    }
}