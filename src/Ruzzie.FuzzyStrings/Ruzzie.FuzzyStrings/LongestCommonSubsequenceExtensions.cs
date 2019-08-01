/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * Derived from http://www.codeproject.com/KB/recipes/lcs.aspx 
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Runtime.CompilerServices;

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
                input = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(input);
                comparedTo = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(comparedTo);
            }

            int inputLen = input.Length;
            int comparedToLen = comparedTo.Length;

            int[,] lcs = new int[inputLen + 1, comparedToLen + 1];
            LcsDirection[,] tracks = new LcsDirection[inputLen + 1, comparedToLen + 1];
            int[,] w = new int[inputLen + 1, comparedToLen + 1];

            //for (int i = 0; i <= inputLen; ++i)
            //{
            //    // lcs[i, 0] = 0;  //intial value is always 0, (managed language ;))
            //    tracks[i, 0] = LcsDirection.North;
            //}
            //for (int j = 0; j <= comparedToLen; ++j)
            //{
            //    //lcs[0, j] = 0;  //intial value is always 0, (managed language ;))
            //    tracks[0, j] = LcsDirection.West;
            //}
            // tracks[0, 0] = LcsDirection.North;
            tracks[0, 0] = LcsDirection.West;
            for (int i = 1; i <= inputLen; ++i)
            {
                tracks[i, 0] = LcsDirection.North; //Initialize default
                for (int j = 1; j <= comparedToLen; ++j)
                {
                    tracks[0, j] = LcsDirection.West; //Initialize default

                    if (input[i - 1] == (comparedTo[j - 1]))
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

#if !PORTABLE && HAVE_METHODINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static int SquareOfValuePlusOneMinusSquareOfValue(int k)
        {
            /*Square(k + 1) - Square(k) // this can be written as 2k +1 */
            return ((2*k) + 1);
        }

        private static LongestCommonSubsequenceResult CalculateLongestCommonSubSequence(string input, int i, int j, in LcsDirection[,] tracks, double coefficient)
        {
            //string subseq = string.Empty;
            var inputLength = input.Length;
            char[] subSequence =  new char[inputLength];
            int subsequenceLength = 0;
            //trace the backtracking matrix.
            while (i > 0 || j > 0)
            {
                if (tracks[i, j] == LcsDirection.NorthWest)
                {
                    i--;
                    j--;
                    //subseq = input[i] + subseq;
                    subSequence[inputLength-subsequenceLength -1] = input[i];
                    subsequenceLength++;
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
            return new LongestCommonSubsequenceResult(new string(subSequence, inputLength-subsequenceLength, subsequenceLength), coefficient);
        }

        static readonly LongestCommonSubsequenceResult EmptyResult = default(LongestCommonSubsequenceResult);
        /// <summary>
        ///     Longest Common Subsequence. A good value is greater than 0.33.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="comparedTo"></param>
        /// <param name="caseSensitive"></param>
        /// <param name="includeLongestSubsequenceInResult"></param>
        /// <returns>Returns a Tuple of the sub sequence string and the match coefficient.</returns>
        public static LongestCommonSubsequenceResult LongestCommonSubsequenceUncached(this string input, string comparedTo, bool caseSensitive = false,
             bool includeLongestSubsequenceInResult = true)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(comparedTo))
            {
                return EmptyResult;
            }

            if (includeLongestSubsequenceInResult)
            {
                return LongestCommonSubsequenceUncachedWithResult(input, comparedTo, caseSensitive);
            }

            if (!caseSensitive)
            {
                input = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(input);
                comparedTo = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(comparedTo);
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
                    if (input[i - 1] == (comparedTo[j - 1]))
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
        public LongestCommonSubsequenceResult(string longestSubsequence, double coefficient) : this()
        {
            LongestSubsequence = longestSubsequence;
            Coefficient = coefficient;
            WithStringResult = true;
        }

        public LongestCommonSubsequenceResult(double coefficient) : this()
        {
            LongestSubsequence = string.Empty;
            Coefficient = coefficient;
            WithStringResult = false;
        }

        public readonly bool WithStringResult;
        public readonly string LongestSubsequence;
        public readonly double Coefficient;
    }
}