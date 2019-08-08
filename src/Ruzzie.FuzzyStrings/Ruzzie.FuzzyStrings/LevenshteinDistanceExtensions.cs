using System;
using System.Runtime.CompilerServices;

namespace Ruzzie.FuzzyStrings
{
    public static class LevenshteinDistanceExtensions
    {
        public static int LevenshteinDistance(this string input, string comparedTo, bool caseSensitive = false)
        {
            return LevenshteinDistanceUncachedAlternativeV2(input, comparedTo, caseSensitive);
        }


        #if HAVE_METHODINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public static int Min(int a, int b, int c)
        {
            return Math.Min(a, Math.Min(b, c));
        }

        public static int LevenshteinDistanceUncachedAlternative(this string input, string comparedTo, bool caseSensitive = false)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(comparedTo))
            {
                return -1;
            }

            if (!caseSensitive)
            {
                input = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(input);
                comparedTo = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(comparedTo);
            }

            int inputLen = input.Length+1;
            int comparedToLen = comparedTo.Length+1;

            int[,] matrix = new int[inputLen, comparedToLen];


            for (var i = 0; i < inputLen; i++)
            {
                matrix[i, 0] = i;
            }
            for (var i = 0; i < comparedToLen; i++)
            {
                matrix[0, i] = i;
            }

            const int bZero = 0;
            const int bOne = 1;

            //analyze
            for (var i = 1; i < inputLen; i++)
            {
                char si = input[i - 1];
                for (var j = 1; j < comparedToLen; j++)
                {
                    char tj = comparedTo[j - 1];
                    var cost = (si == tj) ? bZero : bOne;

                    int cell = Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1,
                        matrix[i - 1, j - 1] + cost);

                    //transposition
                    if (i > 1 && j > 1)
                    {
                        int trans = matrix[i - 2, j - 2] + 1;
                        if (input[i - 2] != comparedTo[j - 1])
                        {
                            trans++;
                        }
                        if (input[i - 1] != comparedTo[j - 2])
                        {
                            trans++;
                        }
                        if (cell > trans)
                        {
                            cell = trans;
                        }
                    }
                    matrix[i, j] = cell;
                }
            }
            return matrix[inputLen - 1, comparedToLen - 1];
        }

        public static int LevenshteinDistanceUncachedAlternativeV2(this string input,
            string comparedTo,
            bool caseSensitive = false)
        {

            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(comparedTo))
            {
                return -1;
            }

            if (!caseSensitive)
            {
                input = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(input);
                comparedTo = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(comparedTo);
            }

            int x, y, lastdiag, olddiag;
            var inputLength = input.Length;
            var comparedToLength = comparedTo.Length;
            unsafe
            {
                int* column = stackalloc int[inputLength + 1];//new int[inputLength + 1];
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
                        column[y] = Min(column[y] + 1, column[y - 1] + 1,
                            lastdiag + (input[y - 1] == comparedTo[x - 1] ? 0 : 1));
                        lastdiag = olddiag;
                    }
                }

                return (column[inputLength]);
            }
        }
    }
}