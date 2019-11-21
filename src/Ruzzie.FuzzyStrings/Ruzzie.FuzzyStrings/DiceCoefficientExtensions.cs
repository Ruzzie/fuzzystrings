using System.Runtime.CompilerServices;

namespace Ruzzie.FuzzyStrings
{
    public static class DiceCoefficientExtensions
    {
        public static double DiceCoefficient(this string input, string comparedTo)
        {
            return DiceCoefficientAlternativeV2(input, comparedTo);
        }

        public static double DiceCoefficientAlternativeV2(this string input, string comparedTo)
        {
            unsafe
            {
                int inputNgramsLength = input.Length -1;

                int comparedToNgramsLength = comparedTo.Length - 1;

                if (inputNgramsLength < 1 || comparedToNgramsLength < 1)
                {
                    return 0;
                }

                var lastComparedToIndexCreated = -1;
                int* comparedToNgrams = stackalloc int[comparedToNgramsLength];
                var matches = 0;
                fixed (char* inputPtr = input, comparedToPtr = comparedTo)
                {
                    //Check if the the first and / or last chars are the same if so increment match
                    // we do this so no string concatenation with the SinglePercent + input + SinglePound is needed on the input string
                    if (*inputPtr == *comparedToPtr)
                    {
                        matches++;
                    }

                    if (inputPtr[inputNgramsLength] == comparedToPtr[comparedToNgramsLength])
                    {
                        matches++;
                    }

                    for (int i = 0; i < inputNgramsLength; i++)
                    {
                        var inputBiGramValue = GetBiGramValueAsInt(inputPtr, i);

                        for (int j = 0; j < comparedToNgramsLength; j++)
                        {
                            //Only create new BiGram of comparedTo when not already created
                            if (lastComparedToIndexCreated < j)
                            {
                                comparedToNgrams[j] = GetBiGramValueAsInt(comparedToPtr, j);
                                lastComparedToIndexCreated = j;
                            }

                            //Now see if there are matches
                            if (inputBiGramValue == comparedToNgrams[j])
                            {
                                matches++;
                                break;
                            }
                        }
                    }
                }

                if (matches == 0)
                {
                    return 0.0d;
                }
                double totalBigrams = inputNgramsLength + comparedToNgramsLength + 4;//+4 since we do not concat the string anymore, which results in +2 bigrams per input
                return (2 * matches) / totalBigrams;
            }
        }

        #if HAVE_METHODINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private static unsafe int GetBiGramValueAsInt(char* value, int startIndex)
        {
            //int r = value[startIndex] << 16;
            //return r |= value[startIndex + 1];
            return *(int*) &value[startIndex];
        }
    }
}