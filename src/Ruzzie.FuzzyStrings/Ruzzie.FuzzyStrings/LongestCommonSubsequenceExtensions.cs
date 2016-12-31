/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * Derived from http://www.codeproject.com/KB/recipes/lcs.aspx 
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;

namespace Ruzzie.FuzzyStrings
{
    public static class LongestCommonSubsequenceExtensions
    {      
        public static LongestCommonSubsequenceResult LongestCommonSubsequence(this string input, string comparedTo, bool caseSensitive = false, bool includeLongestSubsequenceInResult = true)
        {
            return input.LongestCommonSubsequenceUncached(comparedTo, caseSensitive, includeLongestSubsequenceInResult);           
        }

        private static LongestCommonSubsequenceResult LongestCommonSubsequenceUncachedWithResult(string input, string comparedTo, bool caseSensitive)
        {
            if (!caseSensitive)
            {
                input = input.ToUpperInvariant();
                comparedTo = comparedTo.ToUpperInvariant();
            }

            int inputLen = input.Length;
            int comparedToLen = comparedTo.Length;

            int[,] lcs = new int[inputLen + 1, comparedToLen + 1];
            LcsDirection[,] tracks = new LcsDirection[inputLen + 1, comparedToLen + 1];
            int[,] w = new int[inputLen + 1, comparedToLen + 1];

            for (int i = 0; i <= inputLen; ++i)
            {
                // lcs[i, 0] = 0;  //intial value is always 0, (managed language ;))
                tracks[i, 0] = LcsDirection.North;
            }
            for (int j = 0; j <= comparedToLen; ++j)
            {
                //lcs[0, j] = 0;  //intial value is always 0, (managed language ;))
                tracks[0, j] = LcsDirection.West;
            }

            for (int i = 1; i <= inputLen; ++i)
            {
                for (int j = 1; j <= comparedToLen; ++j)
                {
                    if (input[i - 1].Equals(comparedTo[j - 1]))
                    {
                        int k = w[i - 1, j - 1];
                        lcs[i, j] = lcs[i - 1, j - 1] + SquareOfValuePlusOneMinusSquareOfValue(k);
                        tracks[i, j] = LcsDirection.NorthWest;
                        w[i, j] = k + 1;
                    }
                    else
                    {
                        lcs[i, j] = lcs[i - 1, j - 1];
                        tracks[i, j] = LcsDirection.None;
                    }

                    if (lcs[i - 1, j] >= lcs[i, j])
                    {
                        lcs[i, j] = lcs[i - 1, j];
                        tracks[i, j] = LcsDirection.North;
                        w[i, j] = 0;
                    }

                    if (lcs[i, j - 1] >= lcs[i, j])
                    {
                        lcs[i, j] = lcs[i, j - 1];
                        tracks[i, j] = LcsDirection.West;
                        w[i, j] = 0;
                    }
                }
            }
            double p = lcs[inputLen, comparedToLen];
            double coef = p / (inputLen * comparedToLen);
          
            return CalculateLongestCommonSubSequence(input, inputLen, comparedToLen, tracks, coef);
        }

        private static int SquareOfValuePlusOneMinusSquareOfValue(int k)
        {
            /*Square(k + 1) - Square(k) // this can be written as 2k +1 */
            return ((2*k) + 1);
        }

        private static LongestCommonSubsequenceResult CalculateLongestCommonSubSequence(string input, int i, int j, LcsDirection[,] tracks, double coef)
        {
            string subseq = string.Empty;
            //trace the backtracking matrix.
            while (i > 0 || j > 0)
            {
                if (tracks[i, j] == LcsDirection.NorthWest)
                {
                    i--;
                    j--;
                    subseq = input[i] + subseq;
                    //Trace.WriteLine(i + " " + input1[i] + " " + j);
                }

                else if (tracks[i, j] == LcsDirection.North)
                {
                    i--;
                }

                else if (tracks[i, j] == LcsDirection.West)
                {
                    j--;
                }
            }

            return new LongestCommonSubsequenceResult(subseq, coef);
        }

        /// <summary>
        ///     Longest Common Subsequence. A good value is greater than 0.33.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="comparedTo"></param>
        /// <param name="caseSensitive"></param>
        /// <param name="includeLongestSubsequenceInResult"></param>
        /// <returns>Returns a Tuple of the sub sequence string and the match coeficient.</returns>
        public static LongestCommonSubsequenceResult LongestCommonSubsequenceUncached(this string input, string comparedTo, bool caseSensitive = false,
             bool includeLongestSubsequenceInResult = true)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(comparedTo))
            {
                return new LongestCommonSubsequenceResult();
            }

            if (includeLongestSubsequenceInResult)
            {
                return LongestCommonSubsequenceUncachedWithResult(input, comparedTo, caseSensitive);
            }

            if (!caseSensitive)
            {
                input = input.ToUpperInvariant();
                comparedTo = comparedTo.ToUpperInvariant();
            }

            int inputLen = input.Length;
            int comparedToLen = comparedTo.Length;

            int[,] lcs = new int[inputLen + 1, comparedToLen + 1];
            int[,] w = new int[inputLen + 1, comparedToLen + 1];

            for (int i = 1; i <= inputLen; ++i)
            {
                for (int j = 1; j <= comparedToLen; ++j)
                {
                    int currentLcs;
                    int currentK;
                    if (input[i - 1].Equals(comparedTo[j - 1]))
                    {
                        int k = w[i - 1, j - 1];
                        currentLcs = lcs[i - 1, j - 1] + SquareOfValuePlusOneMinusSquareOfValue(k);
                        currentK = k + 1;
                    }
                    else
                    {
                        currentLcs = lcs[i - 1, j - 1];
                        currentK = w[i, j];
                    }

                    int tmpLcs = Math.Max(lcs[i - 1, j], lcs[i, j - 1]);

                    if (tmpLcs >= currentLcs)
                    {
                        currentK = 0;
                        currentLcs = tmpLcs;
                    }                   

                    lcs[i, j] = currentLcs;
                    w[i, j] = currentK;
                }
            }

            double p = lcs[inputLen, comparedToLen];
            double coef = p / (inputLen * comparedToLen);

            return new LongestCommonSubsequenceResult(coef);

        }
      
    }

    internal enum LcsDirection
    {
        None = 0,
        North = 2,
        West = 4,
        NorthWest = 8
    }

    public struct LongestCommonSubsequenceResult
    {
        public LongestCommonSubsequenceResult(string longestSubsequence, double coeffecient) : this()
        {
            LongestSubsequence = longestSubsequence;
            Coeffecient = coeffecient;
            WithStringResult = true;
        }

        public LongestCommonSubsequenceResult(double coeffecient) : this()
        {
            LongestSubsequence = null;
            Coeffecient = coeffecient;
            WithStringResult = false;
        }

        public bool WithStringResult;
        public string LongestSubsequence;
        public double Coeffecient;
    }

}