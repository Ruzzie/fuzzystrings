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
            InternalVariables.DefaultCacheItemSizeInMb * 4,
            InternalVariables.StringComparerForCacheKey, InternalVariables.AverageStringSizeInBytes, InternalVariables.AverageStringSizeInBytes + 2);

        private static readonly IFixedSizeCache<string, HashSet<string>> BiGramsAltCache = new FlashCache<string, HashSet<string>>(InternalVariables.DefaultCacheItemSizeInMb * 4,
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

        public static double DiceCoefficientUncached(this string input, string comparedTo)
        {
            var ngrams = input.ToBiGrams();
            var compareToNgrams = comparedTo.ToBiGrams();
          
            return ngrams.DiceCoefficient(compareToNgrams);
        }

        public static double DiceCoefficientAlternative(this string input, string comparedTo)
        {
            var ngrams = input.ToBiGrams();
            var compareToNgrams = comparedTo.ToUniqueBiGrams();

            return ngrams.DiceCoefficientAlternative(compareToNgrams, comparedTo.Length);
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

        /// <summary>
        ///     Dice Coefficient used to compare nGrams arrays produced in advance.
        /// </summary>
        /// <param name="nGrams"></param>
        /// <param name="compareToNGrams"></param>
        /// <returns></returns>
        private static double DiceCoefficient(this string[] nGrams, string[] compareToNGrams)
        {
            int matches = 0;
            int nGramsLength = nGrams.Length;

            for (int i = 0; i < nGramsLength; i++)
            {                               
                if (Array.IndexOf(compareToNGrams, nGrams[i]) != -1/*compareToNGrams.Contains(nGrams[i])*/)
                {
                    ++matches;
                }
            }
            if (matches == 0)
            {
                return 0.0d;
            }
            double totalBigrams = nGramsLength + compareToNGrams.Length;
            //Console.WriteLine("Original comparetoLen: "+ compareToNGrams.Length);
            return (2*matches)/totalBigrams;
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
            return new HashSet<string>(ToNGrams(input, 2));
        }
      
    }
}