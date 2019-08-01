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
        public const double ExactMatchProbability = 0.99999899999999997d;
        public const double FuzzyMatchMaxProbability = 0.99998899999999997d;
#if !PORTABLE && HAVE_REGEXCOMPILEDOPTION
        private static readonly Regex StripRegex = new Regex(@"[^a-zA-Z0-9 -]*", RegexOptions.Compiled);
#else
         private static readonly Regex StripRegex = new Regex(@"[^a-zA-Z0-9 -]*");
#endif

        public static bool FuzzyEquals(this string strA, string strB, double requiredProbabilityScore = 0.75, bool caseSensitive = true)
        {
            return strA.FuzzyMatch(strB, caseSensitive) > requiredProbabilityScore;
        }

        /// <summary>
        /// Tokenize the strings and returns an average probability by matching the tokens divided by the number of comparisons.
        /// Contrary to FuzzyMatch, which also matches 'words' this method does not take distance and order into account.
        /// </summary>
        public static double FuzzyMatchTokens(this string strA, string strB, bool caseSensitive = true)
        {
            return FuzzyMatchTokens(strA, ref strB, DefaultWhitespaceTokenizer, caseSensitive);
        }

        public static readonly IStringTokenizer DefaultWhitespaceTokenizer = new WhitespaceTokenizer();

        /// <summary>
        /// Tokenize the strings and returns an average probability by matching the tokens divided by the number of comparisons.
        /// Contrary to FuzzyMatch, which also matches 'words' this method does not take distance and order into account.
        /// </summary>
        public static double FuzzyMatchTokens(this string strA, ref string strB, IStringTokenizer tokenizer, bool caseSensitive = true)
        {
            if (tokenizer == null)
            {
                throw new ArgumentNullException(nameof(tokenizer));
            }
            var strATokens = tokenizer.Tokenize(strA.Trim());
            var strBTokens = tokenizer.Tokenize(strB.Trim());
            if (strATokens.Length == 0)
            {
                if (strBTokens.Length == 0)
                {
                    return ExactMatchProbability;
                }
            }
            var comparisonCount = 0;
            var numberOfExactMatches = 0;
            var numberOfExactPositionMatches = 0;
            var totalScore = 0d;
            var strATokensLength = strATokens.Length;
            var strBTokensLength = strBTokens.Length;

            for (int i = 0; i < strATokensLength; i++)
            {
                for (int j = 0; j < strBTokensLength; j++)
                {
                    var score = FuzzyMatch(strATokens[i], strBTokens[j], caseSensitive);
                    if (Math.Abs(score - ExactMatchProbability) < 0.000000000000000001)
                    {
                        //exact match
                        numberOfExactMatches++;
                        if (i == j)
                        {
                            numberOfExactPositionMatches++;
                        }
                    }
                    totalScore += score;
                    comparisonCount++;
                }
            }

            var maxTokenCount = Math.Max(strATokensLength, strBTokensLength);
            if (maxTokenCount == numberOfExactPositionMatches)
            {
                return ExactMatchProbability;
            }

            var exactMatchScore = (double) numberOfExactMatches / comparisonCount;

            return Math.Min((totalScore / comparisonCount) + exactMatchScore, FuzzyMatchMaxProbability);
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
                    return ExactMatchProbability;
                }

                localA = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(localA);
                localB = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(localB);
            }
            else
            {              
                if (string.Equals(localA, localB, StringComparison.Ordinal))
                {
                    return ExactMatchProbability;
                }
            }

            if (localA.ContainsString(Space) && localB.ContainsString(Space))
            {
                var partsA = localA.Split(' ');
                var partsB = localB.Split(' ');
                int partsALength = partsA.Length;
                int partsBLength = partsB.Length;
                double weightedHighCoefficientsSum = 0;

                for (int i = 0; i < partsALength; i++)
                {
                    double high = 0.0;
                    int indexDistance = 0;
                   
                    for (int x = 0; x < partsBLength; x++)
                    {
                        var coefficient = CompositeCoefficient(partsA[i], partsB[x]);
                        if (coefficient > high)
                        {
                            high = coefficient;
                            indexDistance = Math.Abs(i - x);
                        }
                    }

                    double distanceWeight = indexDistance == 0 ? 1.0 : 1.0 - (indexDistance / ((double) partsALength));
                    weightedHighCoefficientsSum += high * distanceWeight;
                }
                double avgWeightedHighCoefficient = weightedHighCoefficientsSum / partsALength;
                return avgWeightedHighCoefficient < 0.999999 ? avgWeightedHighCoefficient : FuzzyMatchMaxProbability; //fudge factor
            }
            var singleComposite = CompositeCoefficient(localA, localB);
            return singleComposite < 0.999999 ? singleComposite : FuzzyMatchMaxProbability; //fudge factor
        }

        public static string StripWithRegex(string str)
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
        }

        public static bool AnyString(this string input, string stringToFind, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
             return input.IndexOf(stringToFind,0, comparison) != -1;
        }

        private static double CompositeCoefficient(string strA, string strB, bool caseSensitive = true)
        {
            double dice = strA.DiceCoefficient(strB);
            var lcs = strA.LongestCommonSubsequence(strB, caseSensitive, false);
            double levenCoefficient = CalculateLevenshteinDistanceCoefficientForCompositeCoefficient(strA, strB, caseSensitive);//may want to tweak offset

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
            double avgCoefficient = (dice + lcs.Coefficient + levenCoefficient + mpCoefficient) / 4.0;
            return avgCoefficient;
        }

        public static double CalculateLevenshteinDistanceCoefficientForCompositeCoefficient(string strA, string strB, bool caseSensitive = false)
        {
            int leven = strA.LevenshteinDistance(strB, caseSensitive);
            double levenCoefficient = 1.0 / (1.0 * (leven + 0.2));
            return levenCoefficient;
        }
    }
}