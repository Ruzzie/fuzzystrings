using System;

namespace DuoVia.FuzzyStrings
{
    public static class LevenshteinDistanceExtensions
    {
        public static int LevenshteinDistance(this string input, string comparedTo, bool caseSensitive = false)
        {
            return LevenshteinDistanceUncached(input, comparedTo, caseSensitive);
        }

        /// <summary>
        ///     Levenshtein Distance algorithm with transposition. <br />
        ///     A value of 1 or 2 is okay, 3 is iffy and greater than 4 is a poor match
        /// </summary>
        /// <param name="input"></param>
        /// <param name="comparedTo"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        /// <remarks>always case insensitive</remarks>
        public static int LevenshteinDistanceUncached(this string input, string comparedTo, bool caseSensitive = false)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(comparedTo))
            {
                return -1;
            }

            if (!caseSensitive)
            {
                input = input.ToUpperInvariant();
                comparedTo = comparedTo.ToUpperInvariant();
            }

            int inputLen = input.Length;
            int comparedToLen = comparedTo.Length;

            int[,] matrix = new int[inputLen, comparedToLen];

            //initialize           
            for (var i = 0; i < inputLen; i++)
            {
                matrix[i, 0] = i;
            }
            for (var i = 0; i < comparedToLen; i++)
            {
                matrix[0, i] = i;
            }

            //analyze
            for (var i = 1; i < inputLen; i++)
            {
                ushort si = input[i - 1];
                for (var j = 1; j < comparedToLen; j++)
                {
                    ushort tj = comparedTo[j - 1];
                    int cost = (si == tj) ? 0 : 1;

                    int above = matrix[i - 1, j];
                    int left = matrix[i, j - 1];
                    int diag = matrix[i - 1, j - 1];
                    int cell = FindMinimumOptimized(above + 1, left + 1, diag + cost);

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

        public static int FindMinimum(params int[] p)
        {
            if (ReferenceEquals(null,p))
            {
                return int.MinValue;
            }
            int min = int.MaxValue;
            int length = p.Length;
            for (var i = 0; i < length; ++i)
            {
                //min = Math.Min(min, p[i]);
                if (min > p[i])
                {
                    min = p[i];
                }
            }
            return min;
        }

        public static int FindMinimumOptimized(int a, int b, int c)
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
                input = input.ToUpperInvariant();
                comparedTo = comparedTo.ToUpperInvariant();
            }

            int inputLen = input.Length;
            int comparedToLen = comparedTo.Length;

            int[,] matrix = new int[inputLen, comparedToLen];


            for (var i = 0; i < inputLen; i++)
            {
                matrix[i, 0] = i;
            }
            for (var i = 0; i < comparedToLen; i++)
            {
                matrix[0, i] = i;
            }

            //analyze
            for (var i = 1; i < inputLen; i++)
            {
                char si = input[i - 1];
                for (var j = 1; j < comparedToLen; j++)
                {
                    char tj = comparedTo[j - 1];
                    int cost = (si == tj) ? 0 : 1;

                    int above = matrix[i - 1, j];
                    int left = matrix[i, j - 1];
                    int diag = matrix[i - 1, j - 1];
                    int cell = FindMinimumOptimized(above + 1, left + 1, diag + cost);

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
    }
}