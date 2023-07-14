using System;
using Ruzzie.Common.Caching;

namespace Ruzzie.FuzzyStrings
{
    public static class StringExtensions
    {
        private static readonly FlashCache<string, double> Cache =
            new FlashCache<string, double>(InternalVariables.StringComparerForCacheKey, (InternalVariables.MaxCacheSizeInMb * 1024 * 1024) / InternalVariables.AverageStringSizeInBytes);

        public const double ExactMatchProbability = 0.99999899999999997d;
        public const double FuzzyMatchMaxProbability = 0.99998899999999997d;

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
            return Cache.GetOrAdd(string.Concat(strA, strB, caseSensitive.ToString()), key => strA.FuzzyMatchUncached(strB, caseSensitive));
        }

        /// <summary>
        /// Fuzzy matches the already upperCased strings.
        /// </summary>
        /// <param name="strAUpperCase">The string a upper case.</param>
        /// <param name="strBUppercase">The string b uppercase.</param>
        /// <returns></returns>
        /// <remarks>This only works if the caller has already upperCased the strings</remarks>
        public static double FuzzyMatchAlreadyUpperCasedStrings(this string strAUpperCase, string strBUppercase)
        {
            return Cache.GetOrAdd(string.Concat(strAUpperCase, strBUppercase, true.ToString(), true.ToString()),
                key => strAUpperCase.FuzzyMatchAlreadyUpperCasedStringsUncached(strBUppercase));
        }

        /// <summary>
        /// Fuzzy matches the already upperCased strings.
        /// </summary>
        /// <param name="strAUpperCase">The string a upper case.</param>
        /// <param name="strBUppercase">The string b uppercase.</param>
        /// <returns></returns>
        /// <remarks>This only works if the caller has already upperCased the strings</remarks>
        public static double FuzzyMatchAlreadyUpperCasedStringsUncached(this string strAUpperCase, string strBUppercase)
        {
            string localA = Common.StringExtensions.StripAlternative(strAUpperCase.Trim());
            string localB = Common.StringExtensions.StripAlternative(strBUppercase.Trim());

            if (string.Equals(localA, localB, StringComparison.Ordinal))
            {
                return ExactMatchProbability;
            }

            return AvgWeightedHighCoefficient(localA, localB, true, true);
        }

        public static double FuzzyMatchUncached(this string strA, string strB, bool caseSensitive = true)
        {
            string localA           = Common.StringExtensions.StripAlternative(strA.Trim());
            string localB           = Common.StringExtensions.StripAlternative(strB.Trim());
            bool   isAlreadyToUpper = false;
            if (!caseSensitive)
            {
                if (string.Equals(localA, localB, StringComparison.OrdinalIgnoreCase))
                {
                    return ExactMatchProbability;
                }

                localA = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(localA);
                localB = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(localB);
                isAlreadyToUpper = true;
            }
            else
            {
                if (string.Equals(localA, localB, StringComparison.Ordinal))
                {
                    return ExactMatchProbability;
                }
            }

            return AvgWeightedHighCoefficient(localA, localB, caseSensitive, isAlreadyToUpper);
        }

        private const char SpaceSeparator =  ' ';
        private static double AvgWeightedHighCoefficient(
            string localA,
            string localB,
            bool caseSensitive,
            bool isAlreadyToUpper)
        {
            var partsA       = localA.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);
            var partsB       = localB.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);
            int partsALength = partsA.Length;
            int partsBLength = partsB.Length;

            if (partsALength > 1 && partsBLength > 1)
            {
                double weightedHighCoefficientsSum = 0;

                for (int i = 0; i < partsALength; i++)
                {
                    double high = 0.0;
                    int indexDistance = 0;

                    for (int x = 0; x < partsBLength; x++)
                    {
                        var coefficient = CompositeCoefficient(partsA[i], partsB[x], caseSensitive, isAlreadyToUpper);
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
                return avgWeightedHighCoefficient < 0.999999
                    ? avgWeightedHighCoefficient
                    : FuzzyMatchMaxProbability; //fudge factor
            }

            var singleComposite = CompositeCoefficient(localA, localB, caseSensitive, isAlreadyToUpper);
            return singleComposite < 0.999999 ? singleComposite : FuzzyMatchMaxProbability; //fudge factor
        }
        
        public static bool AnyString(this string input, string stringToFind, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (comparison == StringComparison.OrdinalIgnoreCase)
            {
                return input.ToLowerInvariant().Contains(stringToFind.ToLowerInvariant());
            }

            return input.Contains(stringToFind, comparison);

            //return input.IndexOf(stringToFind,0, comparison) != -1; //this is slower
        }

        public static double CompositeCoefficient(
            string strA,
            string strB,
            bool caseSensitive = true,
            bool isAlreadyToUpper = false)
        {
            double dice = strA.DiceCoefficient(strB);
            var lcs = strA.LongestCommonSubsequence(strB, caseSensitive, false, isAlreadyToUpper);
            double levenCoefficient = CalculateLevenshteinDistanceCoefficientForCompositeCoefficient(strA, strB, caseSensitive, isAlreadyToUpper);//may want to tweak offset

            int matchCount = 0;
            if (strA.Length > 4 && strB.Length > 4)
            {
                unsafe
                {
                    char* strAMp = stackalloc char[4];
                    char* strBMp = stackalloc char[4];

                    strA.ToDoubleMetaphone(isAlreadyToUpper, strAMp, out int strAMpLength);
                    strB.ToDoubleMetaphone(isAlreadyToUpper, strBMp, out int strBMpLength);

                    if (strAMpLength == 4 && strAMpLength == strBMpLength)
                    {
                        for (int i = 0; i < strAMpLength; i++)
                        {
                            if (strAMp[i] == strBMp[i])
                            {
                                ++matchCount;
                            }
                        }
                    }
                }
            }

            double mpCoefficient = matchCount == 0 ? 0.0 : matchCount / 4.0;
            double avgCoefficient = (dice + lcs.Coefficient + levenCoefficient + mpCoefficient) / 4.0;
            return avgCoefficient;
        }

        public static double CalculateLevenshteinDistanceCoefficientForCompositeCoefficient(string strA, string strB, bool caseSensitive = false,  bool alreadyUpperCased = false)
        {
            int leven = strA.LevenshteinDistance(strB, caseSensitive, alreadyUpperCased);
            double levenCoefficient = 1.0 / (1.0 * (leven + 0.2));
            return levenCoefficient;
        }
    }
}