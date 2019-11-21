using System;
using System.Runtime.CompilerServices;

namespace Ruzzie.FuzzyStrings
{
    public static class LevenshteinDistanceExtensions
    {
        public static int LevenshteinDistance(this string input, string comparedTo, bool caseSensitive = false,  bool alreadyUpperCased = false)
        {
            return LevenshteinDistanceUncachedAlternativeV2(input, comparedTo, caseSensitive, alreadyUpperCased);
        }

#if HAVE_METHODINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public static int Min(int a, int b, int c)
        {
            return Math.Min(a, Math.Min(b, c));
        }

        public static int LevenshteinDistanceUncachedAlternativeV2(this string input,
            string comparedTo,
            bool caseSensitive = false,
            bool alreadyUpperCased = false)
        {

            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(comparedTo))
            {
                return -1;
            }

            if (!caseSensitive && !alreadyUpperCased)
            {
                input = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(input);
                comparedTo = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(comparedTo);
            }

            int x, y, lastdiag, olddiag;
            var inputLength = input.Length;
            var comparedToLength = comparedTo.Length;
            unsafe
            {
                int* column = stackalloc int[inputLength + 1];
                for (y = 1; y <= inputLength; y++)
                {
                    column[y] = y;
                }

                for (x = 1; x <= comparedToLength; x++)
                {
                    column[0] = x;
                    for (y = 1, lastdiag = x - 1; y <= inputLength; y++)
                    {
                        olddiag = column[y];
                        column[y] = Min(
                            column[y] + 1,
                            column[y - 1] + 1,
                            lastdiag + (input[y - 1] == comparedTo[x - 1] ? 0 : 1));
                        lastdiag = olddiag;
                    }
                }

                return (column[inputLength]);
            }
        }
    }
}