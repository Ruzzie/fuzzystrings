/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Derived from http://www.codeguru.com/vb/gen/vb_misc/algorithms/article.php/c13137__1/Fuzzy-Matching-Demo-in-Access.htm
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Generic;
using Ruzzie.Caching;

namespace Ruzzie.FuzzyStrings
{
    public static class DiceCoefficientExtensions
    {
        private static readonly IFixedSizeCache<string, string[]> BiGramsCache = new FlashCache<string, string[]>(
            InternalVariables.DefaultCacheItemSizeInMb,
            InternalVariables.StringComparerForCacheKey, InternalVariables.AverageStringSizeInBytes, InternalVariables.AverageStringSizeInBytes + 2);

        private static readonly IFixedSizeCache<string, HashSet<string>> BiGramsAltCache = new FlashCache<string, HashSet<string>>(InternalVariables.DefaultCacheItemSizeInMb ,
          InternalVariables.StringComparerForCacheKey, InternalVariables.AverageStringSizeInBytes, InternalVariables.AverageStringSizeInBytes + 2);

        private const string SinglePercent = "%";
        private const string SinglePound = "#";
        private const string DoublePercent = "&&";
        private const string DoublePound = "##";

        public static double DiceCoefficient(this string input, string comparedTo)
        {
            var compareToNgrams = BiGramsAltCache.GetOrAdd(comparedTo, key => comparedTo.ToUniqueBiGrams());
            var ngrams = BiGramsCache.GetOrAdd(input, key => input.ToBiGrams());
            

            //var ngrams = input.ToBiGrams();
            //var compareToNgrams = comparedTo.ToUniqueBiGrams();
            return ngrams.DiceCoefficientAlternative(compareToNgrams, comparedTo.Length);
        }

        public static double DiceCoefficientAlternative(this string input, string comparedTo)
        {
            var ngrams = input.ToBiGrams();
            var compareToNgrams = comparedTo.ToUniqueBiGrams();

            return ngrams.DiceCoefficientAlternative(compareToNgrams, comparedTo.Length);
        }

        public static double DiceCoefficientAlternativeV2(this string input, string comparedTo)
        {
            
            //To Bigrams
            input = string.Concat(SinglePercent, input, SinglePound);
            int inputNgramsLength = input.Length - 1;

            
            comparedTo = string.Concat(SinglePercent, comparedTo, SinglePound);
            int comparedToNgramsLength = comparedTo.Length - 1;

            unsafe
            {
                BiGram* inputNgrams = stackalloc BiGram[inputNgramsLength];
               // var lastComparedToIndexCreated = 0;

                fixed (char* inputPtr = input)
                {
                    for (int i = 0; i < inputNgramsLength; i++)
                    {
                        inputNgrams[i] = new BiGram(inputPtr, i);

                    }
                }


                BiGram* comparedToNgrams = stackalloc BiGram[comparedToNgramsLength];

                fixed (char* comparedToPtr = comparedTo)
                {
                    for (int i = 0; i < comparedToNgramsLength; i++)
                    {
                        comparedToNgrams[i] = new BiGram(comparedToPtr, i); 
                    }
                }

                var matches = 0;
                //Now see if there are matches
                for (int i = 0; i < inputNgramsLength; i++)
                {
                    for (int j = 0; j < comparedToNgramsLength; j++)
                    {
                        if (inputNgrams[i].Value == comparedToNgrams[j].Value)
                        {
                            matches++;
                            break;
                        }
                    }
                }

                if (matches == 0)
                {
                    return 0.0d;
                }
                double totalBigrams = inputNgramsLength + comparedToNgramsLength;
                return (2 * matches) / totalBigrams;
            }
        }

        private readonly struct BiGram : IEquatable<BiGram>
        {
            public readonly int Value;
            public unsafe BiGram(char* value, int startIndex)
            {
                Value = value[startIndex] << 16;
                Value |= value[startIndex + 1];
            }

            public bool Equals(BiGram other)
            {
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is BiGram other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Value;
            }

            public static bool operator ==(BiGram left, BiGram right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(BiGram left, BiGram right)
            {
                return !left.Equals(right);
            }
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