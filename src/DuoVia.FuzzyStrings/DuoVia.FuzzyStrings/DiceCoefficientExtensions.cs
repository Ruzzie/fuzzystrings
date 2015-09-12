/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Derived from http://www.codeguru.com/vb/gen/vb_misc/algorithms/article.php/c13137__1/Fuzzy-Matching-Demo-in-Access.htm
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DuoVia.FuzzyStrings
{
    public static class DiceCoefficientExtensions
    {
        static readonly ConcurrentDictionary<string, double> Cache = new ConcurrentDictionary<string, double>(256, 18000*4,StringComparer.OrdinalIgnoreCase);

        private const string SinglePercent = "%";
        private const string SinglePound = "#";
        private const string DoublePercent = "&&";
        private const string DoublePount = "##";

        public static double DiceCoefficient(this string input, string comparedTo)
        {
            return Cache.GetOrAdd(input + comparedTo, key => input.DiceCoefficientUncached(comparedTo));
        }

        public static double DiceCoefficientUncached(this string input, string comparedTo)
        {
            var ngrams = input.ToBiGrams();
            var compareToNgrams = comparedTo.ToBiGrams();
            return ngrams.DiceCoefficient(compareToNgrams);
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
            for (int i = 0; i < nGrams.Length; i++)
            {                                
                if (compareToNGrams.Contains(nGrams[i]))
                {
                    matches++;
                }
            }
            if (matches == 0)
            {
                return 0.0d;
            }
            double totalBigrams = nGrams.Length + compareToNGrams.Length;
            return (2*matches)/totalBigrams;
        }

        public static string[] ToBiGrams(this string input)
        {
            // nLength == 2
            //   from Jackson, return %j ja ac ck ks so on n#
            //   from Main, return #m ma ai in n#
            input = SinglePercent + input + SinglePound;
            return ToNGrams(input, 2);
        }

        public static string[] ToTriGrams(this string input)
        {
            // nLength == 3
            //   from Jackson, return %%j %ja jac ack cks kso son on# n##
            //   from Main, return ##m #ma mai ain in# n##
            input = DoublePercent + input + DoublePount;
            return ToNGrams(input, 3);
        }

        private static string[] ToNGrams(string input, int nLength)
        {
            int itemsCount = input.Length - 1;
            string[] ngrams = new string[input.Length - 1];
            for (int i = 0; i < itemsCount; i++)
            {
                ngrams[i] = input.Substring(i, nLength);
            }
            return ngrams;
        }
    }
}