/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * Derived from http://www.codeproject.com/KB/recipes/lcs.aspx 
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Runtime.CompilerServices;

namespace Ruzzie.FuzzyStrings
{
    public static class LongestCommonSubsequenceExtensions
    {
        public static LongestCommonSubsequenceResult LongestCommonSubsequence(this string input,
            string comparedTo,
            bool caseSensitive = false,
            bool includeLongestSubsequenceInResult = true,
            bool alreadyUpperCased = false
            )
        {
            return input.LongestCommonSubsequenceUncached(comparedTo, caseSensitive, includeLongestSubsequenceInResult, alreadyUpperCased);
        }

        private static LongestCommonSubsequenceResult LongestCommonSubsequenceUncachedWithResult(string input,
            string comparedTo,
            bool caseSensitive,
            bool alreadyUpperCased = false
            )
        {
            if (!caseSensitive && ! alreadyUpperCased)
            {
                input = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(input);
                comparedTo = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(comparedTo);
            }

            int inputLen = input.Length;
            int comparedToLen = comparedTo.Length;

            int[,] lcs = new int[inputLen + 1, comparedToLen + 1];
            LcsDirection[,] tracks = new LcsDirection[inputLen + 1, comparedToLen + 1];
            int[,] w = new int[inputLen + 1, comparedToLen + 1];


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
            return ((2 * k) + 1);
        }

        private static LongestCommonSubsequenceResult CalculateLongestCommonSubSequence(string input,
            int i,
            int j,
            in LcsDirection[,] tracks,
            double coefficient)
        {
            //string subseq = string.Empty;
            var inputLength = input.Length;
            char[] subSequence = new char[inputLength];
            int subsequenceLength = 0;
            //trace the backtracking matrix.
            while (i > 0 || j > 0)
            {
                if (tracks[i, j] == LcsDirection.NorthWest)
                {
                    i--;
                    j--;
                    //subseq = input[i] + subseq;
                    subSequence[inputLength - subsequenceLength - 1] = input[i];
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

            return new LongestCommonSubsequenceResult(
                new string(subSequence, inputLength - subsequenceLength, subsequenceLength), coefficient);
        }

        static readonly LongestCommonSubsequenceResult EmptyResult = default(LongestCommonSubsequenceResult);

        /// <summary>
        ///     Longest Common Subsequence. A good value is greater than 0.33.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="comparedTo"></param>
        /// <param name="caseSensitive"></param>
        /// <param name="includeLongestSubsequenceInResult"></param>
        /// <param name="alreadyUpperCased">Indicates if the input and comparedTo string are already upperCased.</param>
        /// <returns>Returns a Tuple of the sub sequence string and the match coefficient.</returns>
        public static LongestCommonSubsequenceResult LongestCommonSubsequenceUncached(this string input,
            string comparedTo,
            bool caseSensitive = false,
            bool includeLongestSubsequenceInResult = true,
            bool alreadyUpperCased = false)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(comparedTo))
            {
                return EmptyResult;
            }

            if (includeLongestSubsequenceInResult)
            {
                return LongestCommonSubsequenceUncachedWithResult(input, comparedTo, caseSensitive, alreadyUpperCased);
            }

            return LongestCommonSubsequenceWithoutSubsequenceAlternative(input, comparedTo, caseSensitive, alreadyUpperCased);
        }

        /// <summary>
        ///     Longest Common Subsequence. A good value is greater than 0.33.
        /// </summary>
        public static LongestCommonSubsequenceResult LongestCommonSubsequenceWithoutSubsequenceAlternative(
            this string input,
            string comparedTo,
            bool caseSensitive = false,
            bool alreadyUpperCased = false
        )
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(comparedTo))
            {
                return EmptyResult;
            }

            if (!caseSensitive && !alreadyUpperCased)
            {
                input = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(input);
                comparedTo = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(comparedTo);
            }

            unsafe
            {
                fixed (char* inputPtr = input, comparedToPtr = comparedTo)
                {
                    // Find lengths of 
                    // two strings 
                    int m = input.Length, n = comparedTo.Length;

                    BinaryLcsElement* L = stackalloc BinaryLcsElement[n + 1];
                   
                    // Binary index, used to  
                    // index current row and  
                    // previous row. 
                    bool bi = false;

                    for (int i = 1; i <= m; i++)
                    {
                        // Compute current 
                        // binary index 
                        bi = (i & 1) == 0;

                        for (int j = 1; j <= n; j++)
                        {
                            int tmpLcsValue;
                            var tmpCountValue = 0;
                            var previousDiagonal = L[j - 1].At(!bi);
                            if (inputPtr[i - 1] == comparedToPtr[j - 1])
                            {
                                int k = previousDiagonal.CountValue;
                                tmpLcsValue = previousDiagonal.LcsValue + SquareOfValuePlusOneMinusSquareOfValue(k);
                                tmpCountValue = k + 1;
                            }
                            else
                            {
                                tmpLcsValue = previousDiagonal.LcsValue;
                            }

                            var newLcsValue = Math.Max(L[j].At(!bi).LcsValue, L[j - 1].At(bi).LcsValue);

                            if (newLcsValue >= tmpLcsValue)
                            {
                                tmpCountValue = 0;
                                tmpLcsValue = newLcsValue;
                            }

                            L[j].SetAt(bi, new LcsTrack(tmpLcsValue, tmpCountValue));
                        }
                    }


                    // Last filled entry contains 
                    // length of LCS for X[0..n-1] 
                    // and Y[0..m-1]  
                    //double p = L[bi, n].LcsValue;
                    double p = L[n].At(bi).LcsValue;
                    double coef = p / (m * n);
                    return new LongestCommonSubsequenceResult(coef);
                }
            }
        }
#if HAVE_METHODINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int Max(int a, int b, int c)
        {
            return Math.Max(a, Math.Max(b, c));
        }
    }


    internal struct BinaryLcsElement
    {
        private LcsTrack _trueValue;
        private LcsTrack _falseValue;

        //public LcsTrack this[bool index]
        //{
        //    get
        //    {
        //        return index ? _trueValue : _falseValue;
        //    }

        //    set
        //    {
        //        if (index)
        //        {
        //            _trueValue = value;
        //        }
        //        else
        //        {
        //            _falseValue = value;
        //        }
        //    }
        //}

#if HAVE_METHODINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public LcsTrack At(bool index)
        {
            return index ? _trueValue : _falseValue;
        }

#if HAVE_METHODINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void SetAt(bool index, in LcsTrack value)
        {
            if (index)
            {
                _trueValue = value;
            }
            else
            {
                _falseValue = value;
            }
        }
    }

    internal readonly struct LcsTrack
    {
        public readonly int LcsValue;
        public readonly int CountValue;

        public LcsTrack(int lcsValue, int countValue)
        {
            LcsValue = lcsValue;
            CountValue = countValue;
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