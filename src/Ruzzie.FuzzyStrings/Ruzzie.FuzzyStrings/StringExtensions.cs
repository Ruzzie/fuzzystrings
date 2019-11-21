using System;
using System.Text;
using Ruzzie.Caching;

namespace Ruzzie.FuzzyStrings
{
    public static class StringExtensions
    {
        private static readonly IFixedSizeCache<string, double> Cache =
            new FlashCacheWithPool<string, double>(InternalVariables.StringComparerForCacheKey, (InternalVariables.MaxCacheSizeInMb * 1024 * 1024) / InternalVariables.AverageStringSizeInBytes);

        private const string Space = " ";
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
            return Cache.GetOrAdd(string.Concat(strA, strB, caseSensitive), key => strA.FuzzyMatchUncached(strB, caseSensitive));
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
            return Cache.GetOrAdd(string.Concat(strAUpperCase, strBUppercase, true, true),
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
            string localA = StripAlternativeV2(strAUpperCase.Trim());
            string localB = StripAlternativeV2(strBUppercase.Trim());

            if (string.Equals(localA, localB, StringComparison.Ordinal))
            {
                return ExactMatchProbability;
            }

            return AvgWeightedHighCoefficient(localA, localB, true, true);
        }

        public static double FuzzyMatchUncached(this string strA, string strB, bool caseSensitive = true)
        {
            string localA = StripAlternativeV2(strA.Trim());
            string localB = StripAlternativeV2(strB.Trim());
            bool isAlreadyToUpper = false;
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
        static readonly char[] Separator =  {' '};
        private static double AvgWeightedHighCoefficient(
            string localA,
            string localB,
            bool caseSensitive,
            bool isAlreadyToUpper)
        {
            if (localA.ContainsString(Space) && localB.ContainsString(Space))
            {
                var partsA = localA.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
                var partsB = localB.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
                int partsALength = partsA.Length;
                int partsBLength = partsB.Length;
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
        
        /// <summary>
        /// Strips string of characters that are not [^a-zA-Z0-9 -]*
        /// </summary>
        /// <param name="str">The string to strip</param>
        /// <returns>The stripped string</returns>
        public static string StripAlternativeV2(string str)
        {
            int length = str?.Length ?? 0;
           
            if(length == 0)
            {
                return string.Empty;
            }

            unsafe
            {
                int appendIndex = 0;
                char* buffer = stackalloc char[length];

                for (int i = 0; i < length; ++i)
                {
                    char c = str[i];
                    // ReSharper disable RedundantCast
                    if (97 <= (int) c && (int) c <= 122)//a-z
                    {
                        buffer[appendIndex] = c;
                        appendIndex++;
                    } else if (65 <= (int) c && (int) c <= 90) //A-Z
                    {
                        buffer[appendIndex] = c;
                        appendIndex++;
                    } else if (48 <= (int) c && (int) c <= 57) //0-9
                    {
                        buffer[appendIndex] = c;
                        appendIndex++;
                    }
                    else if (32 == (int) c || 45 == (int)c)//space, -
                    {
                        buffer[appendIndex] = c;
                        appendIndex++;
                    }
                    // ReSharper restore RedundantCast
                }
                return new string(buffer, 0, appendIndex);
            }
        }  

        public static bool ContainsString(this string input, string stringToFind)
        {
            unsafe
            {
                int aLength = input.Length;
                int bLength = stringToFind.Length;
                fixed (char* strAPtr = input, strBPtr = stringToFind)
                {
                    for (int i = 0; i < aLength; i++)
                    {
                        if (bLength > (aLength - i))
                        {
                            return false;
                        }

                        var hasStringAt = DoubleMetaphoneExtensions.StringAt(strAPtr, aLength, i, strBPtr, bLength);
                        if (hasStringAt)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public static bool AnyString(this string input, string stringToFind, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
             return input.IndexOf(stringToFind,0, comparison) != -1;
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