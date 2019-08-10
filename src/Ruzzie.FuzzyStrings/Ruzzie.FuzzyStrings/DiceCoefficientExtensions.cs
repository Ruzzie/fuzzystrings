using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ruzzie.Caching;

namespace Ruzzie.FuzzyStrings
{
    public static class DiceCoefficientExtensions
    {
        private const string SinglePercent = "%";
        private const string SinglePound = "#";
        private const string DoublePercent = "&&";
        private const string DoublePound = "##";


        public static double DiceCoefficient(this string input, string comparedTo)
        {
            return DiceCoefficientAlternativeV2(input, comparedTo);
        }

        public static double DiceCoefficientAlternative(this string input, string comparedTo)
        {
            var ngrams = input.ToBiGrams();
            var compareToNgrams = comparedTo.ToUniqueBiGrams();

            return ngrams.DiceCoefficientAlternative(compareToNgrams, comparedTo.Length);
        }

        public static double DiceCoefficientAlternativeV2(this string input, string comparedTo)
        {
            unsafe
            {
                //var input = inputOriginal;//SinglePercent + inputOriginal + SinglePound;//string.Concat(SinglePercent, input, SinglePound);
                int inputNgramsLength = input.Length -1;

               // var comparedTo = comparedToOriginal; //SinglePercent + comparedToOriginal + SinglePound;//string.Concat(SinglePercent, comparedTo, SinglePound);
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

        private static double DiceCoefficientAlternative(this string[] nGrams, HashSet<string> compareToNGrams, int compareToLength)
        {
            int matches = 0;
            int nGramsLength = nGrams.Length;
            //Use hashset to reduce time complexity to O(1) instead of O(N)
            for (int i = 0; i < nGramsLength; i++)
            {
                if (compareToNGrams.Contains(nGrams[i]))
                {
                    ++matches;
                }               
            }
            if (matches == 0)
            {
                return 0.0d;
            }
            double totalBigrams = nGramsLength + (compareToLength + 1);//total +2 for bigram added chars and -1 for len -1
            //Console.WriteLine("Alt comparetoLen: " + (compareToLength + 1));
            return (2 * matches) / totalBigrams;
        }


        private static string[] ToBiGrams(this string input)
        {
            // nLength == 2
            //   from Jackson, return %j ja ac ck ks so on n#
            //   from Main, return #m ma ai in n#
            input = string.Concat(SinglePercent, input, SinglePound);
            return ToNGrams(input, 2);
        }

        public static string[] ToTriGrams(this string input)
        {
            // nLength == 3
            //   from Jackson, return %%j %ja jac ack cks kso son on# n##
            //   from Main, return ##m #ma mai ain in# n##
            input = DoublePercent + input + DoublePound;
            return ToNGrams(input, 3);
        }

        private static string[] ToNGrams(string input, int nLength)
        {
            int itemsCount = input.Length - 1;
            string[] ngrams = new string[itemsCount];
            for (int i = 0; i < itemsCount; i++)
            {
                ngrams[i] = input.Substring(i, nLength);
            }
            return ngrams;
        }


        private static HashSet<string> ToUniqueBiGrams(this string input)
        {
            // nLength == 2
            //   from Jackson, return %j ja ac ck ks so on n#
            //   from Main, return #m ma ai in n#
            input = string.Concat(SinglePercent, input, SinglePound);

            #if NET472
            var ngramsUnique = new HashSet<string>(input.Length);
            #else
            var ngramsUnique = new HashSet<string>();
            #endif
            int itemsCount = input.Length - 1;
            for (int i = 0; i < itemsCount; i++)
            {
                ngramsUnique.Add(input.Substring(i, 2));
            }
            //return ngrams;
            return ngramsUnique;
        }
      
    }
}