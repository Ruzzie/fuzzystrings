﻿using System;
using System.Runtime.CompilerServices;

namespace Ruzzie.FuzzyStrings
{
    /// <summary>
    /// DoubleMetaphone string extension
    /// </summary>
    /// <remarks>
    /// Original C++ implementation:
    ///		"Double Metaphone (c) 1998, 1999 by Lawrence Philips"
    ///		http://www.ddj.com/cpp/184401251?pgno=1
    /// </remarks>
    public static class DoubleMetaphoneExtensions
    {
        public static string ToDoubleMetaphoneStr(this string input, bool isAlreadyToUpper = false)
        {
            return new string(input.ToDoubleMetaphone(isAlreadyToUpper));
        }

        public static char[] ToDoubleMetaphone(this string input, bool isAlreadyToUpper = false)
        {
            char[] buffer = new char[4];
            int length;
            unsafe
            {
                fixed (char* bufferPtr = buffer)
                {
                    ToDoubleMetaphoneUncached(input, isAlreadyToUpper, bufferPtr, out length);
                }
            }
            Array.Resize(ref buffer, length);
            return buffer;
        }

        /// <summary>
        /// Creates metaphone for the given string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="isAlreadyToUpper">if set to <c>true</c> [is already to upper].</param>
        /// <param name="metaphoneBuffer">The metaphone buffer to fill, this must be length 4.</param>
        /// <param name="metaphoneBufferLength">Length of the metaphone written to the buffer.</param>
        internal static unsafe void ToDoubleMetaphone(this string input, bool isAlreadyToUpper, char* metaphoneBuffer, out int metaphoneBufferLength)
        {
            input.ToDoubleMetaphoneUncached(isAlreadyToUpper,metaphoneBuffer, out metaphoneBufferLength);
        }

        private static unsafe void ToDoubleMetaphoneUncached(this string input, bool isAlreadyToUpper, char* metaphoneBuffer, out int metaphoneBufferLength)
        {
             var inputLength = input.Length;

            if (inputLength < 1)
            {
                metaphoneBufferLength = 0;
                return;
            }

            int current = 0;


            if (!isAlreadyToUpper)
            {
                input = Common.Hashing.InvariantUpperCaseStringExtensions.ToUpperInvariant(input);
            }

            bool isSlavoGermanic;

            fixed (char* ptr = input)
            {
                isSlavoGermanic = IsSlavoGermanic(ptr, inputLength);
            }

            //skip these when at start of word: MegaphonesToSkipAtStartOfWord
            if (input.StringAt(0, strGN, strKN, strPN, strWR, strPS))
            {
                ++current;
            }

            //Dirty, dirty, but lets see if we can reduce allocation time and GC time as well as unnecessary resizing of StringBuilder.
            var bufferSize = 16;
            char* primary = stackalloc char[bufferSize];
            char* secondary = stackalloc char[bufferSize];

            MetaphoneBuffer metaphoneData = new MetaphoneBuffer(bufferSize, primary, secondary);

            //Initial 'X' is pronounced 'Z' e.g. 'Xavier'
            if (CharAtOrReturnSpaceWhenOutOfRange(input, 0) == charX)
            {
                metaphoneData.Add(charS); //'Z' maps to 'S'
                ++current;
            }

            while ((metaphoneData.PrimaryIndex < 4) || (metaphoneData.SecondaryIndex < 4))
            {
                if (current >= inputLength)
                {
                    break;
                }

                current = MapCharacter(input, current, ref metaphoneData, isSlavoGermanic, inputLength - 1);//zero based index
            }

            metaphoneData.CopyTo(metaphoneBuffer, out metaphoneBufferLength);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static char CharAtOrReturnSpaceWhenOutOfRange(in string value, int index)
        {
            if (index < value.Length)
            {
                return value[index];
            }

            return ' ';
        }
        static unsafe bool IsSlavoGermanic(char* inputPtr, int inputLength)
        {
            fixed (char* czPtr = strCZ, witzPtr = strWITZ)
            {
                for (int i = 0; i < inputLength; i++)
                {
                    char currChar = inputPtr[i];
                    if (currChar == charW || currChar == charK)
                    {
                        return true;
                    }

                    if (StringAt(inputPtr, inputLength, i, czPtr, 2)
                        ||
                        StringAt(inputPtr, inputLength, i, witzPtr, 4)
                    )
                    {
                        return true;
                    }
                }

                return false;
            }
        }


        //Don't use this struct, it is only used in this specialized edge case for perf. (Yes perf, was tested and profiled).
        unsafe struct MetaphoneBuffer
        {
            public readonly char* Primary;
            public int PrimaryIndex;

            public readonly char* Secondary;
            public int SecondaryIndex;

            public readonly int MaxSize;
            private bool _alternative;

            public MetaphoneBuffer(int maxSize, char* primary, char* secondary)
            {
                MaxSize = maxSize;
                Primary = primary;
                Secondary = secondary;
                PrimaryIndex = 0;
                SecondaryIndex = 0;
                _alternative = false;
            }

            internal void Add(string main)
            {
                if (main != null)
                {
                    var mainLength = main.Length;
                    for (int i = 0;
                        i < mainLength && PrimaryIndex < MaxSize && SecondaryIndex < MaxSize;
                        i++)
                    {
                        //Maybe to optimize we could do a memcopy directly
                        var charToAdd = main[i];
                        Primary[PrimaryIndex] = charToAdd;
                        Secondary[SecondaryIndex] = charToAdd;
                        PrimaryIndex++;
                        SecondaryIndex++;
                    }
                }
            }

            internal void Add(char main)
            {
                //skip the check, to check if it fails
                Primary[PrimaryIndex++] = main;
                Secondary[SecondaryIndex++] = main;
            }

            internal void Add(string main, string alternative)
            {
                if (main != null)
                {
                    var mainLength = main.Length;
                    for (int i = 0;
                        i < mainLength && PrimaryIndex < MaxSize;
                        i++)
                    {
                        //Maybe to optimize we could do a memcopy directly
                        var charToAdd = main[i];
                        Primary[PrimaryIndex] = charToAdd;
                        PrimaryIndex++;
                    }
                }

                if (!string.IsNullOrEmpty(alternative))
                {
                    _alternative = true;
                    if (alternative.Trim().Length > 0)
                    {
                        var altLength = alternative.Length;
                        for (int i = 0;
                            i < altLength && SecondaryIndex < MaxSize;
                            i++)
                        {
                            //Maybe to optimize we could do a memcopy directly
                            var charToAdd = alternative[i];
                            Secondary[SecondaryIndex] = charToAdd;
                            SecondaryIndex++;
                        }
                    }
                }
            }

            public override string ToString()
            {
                //only give back 4 char metaph
                if (_alternative)
                {
                    return new string(Secondary, 0, Math.Min(4, SecondaryIndex));
                }

                return new string(Primary, 0, Math.Min(4, PrimaryIndex));
            }

            public void CopyTo(char* buffer, out int length)
            {
                if (_alternative)
                {
                    length = Math.Min(4, SecondaryIndex);

                    for (int i = 0; i < length; i++)
                    {
                        buffer[i] = Secondary[i];
                    }

                    return;
                }

                length = Math.Min(4, PrimaryIndex);

                for (int i = 0; i < length; i++)
                {
                    buffer[i] = Primary[i];
                }
            }
        }
        private static int MapCharacter(in string workingString, int current, ref MetaphoneBuffer metaphoneData, bool isSlavoGermanic, int last)
        {
            switch (CharAtOrReturnSpaceWhenOutOfRange(workingString, current))
            {
                case charA:
                case charE:
                case charI:
                case charO:
                case charU:
                case charY:
                    if (current == 0)
                    {
                        //all init vowels now map to 'A'
                        metaphoneData.Add('A');
                    }
                    current += 1;
                    break;

                case charB:
                    //"-mb", e.g", "dumb", already skipped over...
                    metaphoneData.Add('P');


                    if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charB)
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    break;

                case charAdash:
                    metaphoneData.Add(charS);
                    current += 1;
                    break;

                case charC:
                    current = MapCharacterC(workingString, current, ref metaphoneData);
                    break;

                case charD:
                    if (StringAt(workingString, current, strDG))
                    {
                        if (StringAt(workingString, (current + 2), strI, strE, strY))
                        {
                            //e.g. 'edge'
                            metaphoneData.Add(charJ);
                            current += 3;
                            break;
                        }
                        else
                        {
                            //e.g. 'edgar'
                            metaphoneData.Add(strTK);
                            current += 2;
                            break;
                        }
                    }

                    if (StringAt(workingString, current, strDT, strDD))
                    {
                        metaphoneData.Add(charT);
                        current += 2;
                        break;
                    }

                    //else
                    metaphoneData.Add(charT);
                    current += 1;
                    break;

                case charF:
                    if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charF)
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    metaphoneData.Add(charF);
                    break;

                case charG:
                    current = MapCharacterG(workingString, current, ref metaphoneData, isSlavoGermanic);
                    break;
                case 'H':
                    //only keep if first & before vowel or btw. 2 vowels
                    if (((current == 0) || IsVowel(CharAtOrReturnSpaceWhenOutOfRange(workingString, current - 1))) && IsVowel(CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1)))
                    {
                        metaphoneData.Add(charH);
                        current += 2;
                    }
                    else //also takes care of 'HH'
                    {
                        current += 1;
                    }
                    break;

                case 'J':
                    //obvious spanish, 'jose', 'san jacinto'
                    if (StringAt(workingString, current, strJOSE) || StringAt(workingString, 0, strSANsp))
                    {
                        if (((current == 0) && (CharAtOrReturnSpaceWhenOutOfRange(workingString,  4) == ' ')) || StringAt(workingString, 0, strSANsp))
                        {
                            metaphoneData.Add(charH);
                        }
                        else
                        {
                            metaphoneData.Add(strJ, strH);
                        }
                        current += 1;
                        break;
                    }

                    if ((current == 0) && !StringAt(workingString, current, strJOSE))
                    {
                        metaphoneData.Add(strJ, strA); //Yankelovich/Jankelowicz
                    }
                    else
                    //spanish pron. of e.g. 'bajador'
                        if (current > 0
                            && IsVowel(CharAtOrReturnSpaceWhenOutOfRange(workingString, current - 1))
                            && !isSlavoGermanic && ((CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charA)
                                                    || (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charO)))
                        {
                            metaphoneData.Add(strJ, strH);
                        }
                        else if (current == last)
                        {
                            metaphoneData.Add(strJ, sp);
                        }
                        else if (!StringAt(workingString, (current + 1), strL, strT, strK, strS, strN, strM, strB, strZ)
                                 && !StringAt(workingString, (current - 1), strS, strK, strL))
                        {
                            metaphoneData.Add(charJ);
                        }

                    if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charJ) //it could happen!
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    break;

                case charK:
                    if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charK)
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    metaphoneData.Add(charK);
                    break;

                case charL:
                    if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charL)
                    {
                        //spanish e.g. 'cabrillo', 'gallegos'
                        if (((current == (workingString.Length - 3))//-3 since NO space is added to workingString
                             && StringAt(workingString, (current - 1), strILLO, strILLA, strALLE))
                            || ((StringAt(workingString, (last - 1), strAS, strOS)
                                 || StringAt(workingString, last, strA, strO))
                                && StringAt(workingString, (current - 1), strALLE)))
                        {
                            metaphoneData.Add(strL, sp);
                            current += 2;
                            break;
                        }
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    metaphoneData.Add('L');
                    break;

                case charM:
                    if ((StringAt(workingString, (current - 1), strUMB)
                         && (((current + 1) == last)
                             || StringAt(workingString, (current + 2), strER))) //'dumb','thumb'
                        || (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charM))
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    metaphoneData.Add('M');
                    break;

                case charN:
                    if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charN)
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    metaphoneData.Add(charN);
                    break;

                case charOdash:
                    current += 1;
                    metaphoneData.Add(charN);
                    break;

                case charP:
                    if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charH)
                    {
                        metaphoneData.Add(charF);
                        current += 2;
                        break;
                    }

                    //also account for "campbell", "raspberry"
                    if (StringAt(workingString, (current + 1), strP, strB))
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    metaphoneData.Add(charP);
                    break;

                case charQ:
                    if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charQ)
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    metaphoneData.Add(charK);
                    break;

                case charR:
                    //french e.g. 'rogier', but exclude 'hochmeier'
                    if ((current == last) && !isSlavoGermanic
                        && StringAt(workingString, (current - 2), strIE)
                        && !StringAt(workingString, (current - 4), strME, strMA))
                    {
                        metaphoneData.Add(string.Empty, strR);
                    }
                    else
                    {
                        metaphoneData.Add(charR);
                    }

                    if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charR)
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    break;

                case charS:
                    current = MapCharacterS(workingString, current, ref metaphoneData, isSlavoGermanic, last);
                    break;

                case charT:
                    if (StringAt(workingString, current, strTION))
                    {
                        metaphoneData.Add(charX);
                        current += 3;
                        break;
                    }

                    if (StringAt(workingString, current, strTIA, strTCH))
                    {
                        metaphoneData.Add(charX);
                        current += 3;
                        break;
                    }

                    if (StringAt(workingString, current, strTH) || StringAt(workingString, current, strTTH))
                    {
                        //special case 'thomas', 'thames' or germanic
                        if (StringAt(workingString, (current + 2), strOM, strAM)
                            || StringAt(workingString, 0, strVANsp, strVONsp) || StringAt(workingString, 0, strSCH))
                        {
                            metaphoneData.Add(charT);
                        }
                        else
                        {
                            metaphoneData.Add(strO, strT);
                        }
                        current += 2;
                        break;
                    }

                    if (StringAt(workingString, (current + 1), strT, strD))
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    metaphoneData.Add(charT);
                    break;

                case charV:
                    if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charV)
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    metaphoneData.Add(charF);
                    break;

                case charW:
                    //can also be in middle of word
                    if (StringAt(workingString, current, strWR))
                    {
                        metaphoneData.Add(charR);
                        current += 2;
                        break;
                    }

                    if ((current == 0) && (IsVowel(CharAtOrReturnSpaceWhenOutOfRange(workingString,  1))
                                           || StringAt(workingString, current, strWH)))
                    {
                        //Wasserman should match Vasserman
                        if (IsVowel(CharAtOrReturnSpaceWhenOutOfRange(workingString,  1)))
                        {
                            metaphoneData.Add(strA, strF);
                        }
                        else
                        {
                            //need Uomo to match Womo
                            metaphoneData.Add(charA);
                        }
                    }

                    //Arnow should match Arnoff
                    if ((current == last && current > 0 && IsVowel(CharAtOrReturnSpaceWhenOutOfRange(workingString, current - 1)))
                        || StringAt(workingString, (current - 1), strEWSKI, strEWSKY, strOWSKI, strOWSKY)
                        || StringAt(workingString, 0, strSCH))
                    {
                        metaphoneData.Add(string.Empty, strF);
                        current += 1;
                        break;
                    }

                    //polish e.g. 'filipowicz'
                    if (StringAt(workingString, current, strWICZ, strWITZ))
                    {
                        metaphoneData.Add(strTS, strFX);
                        current += 4;
                        break;
                    }

                    //else skip it
                    current += 1;
                    break;

                case charX:
                    //french e.g. breaux
                    if (!((current == last)
                          && (StringAt(workingString, (current - 3), strIAU, strEAU)
                              || StringAt(workingString, (current - 2), strAU, strOU))))
                    {
                        metaphoneData.Add(strKS);
                    }

                    if (StringAt(workingString, (current + 1), strC, strX))
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    break;

                case charZ:
                    //chinese pinyin e.g. 'zhao'
                    if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charH)
                    {
                        metaphoneData.Add(charJ);
                        current += 2;
                        break;
                    }
                    else if (StringAt(workingString, (current + 1), strZO, strZI, strZA)
                             || (isSlavoGermanic && ((current > 0) && CharAtOrReturnSpaceWhenOutOfRange(workingString, current - 1) != charT)))
                    {
                        metaphoneData.Add(strS, strTS);
                    }
                    else
                    {
                        metaphoneData.Add(charS);
                    }

                    if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charZ)
                    {
                        current += 2;
                    }
                    else
                    {
                        current += 1;
                    }
                    break;

                default:
                    current += 1;
                    break;
            }
            return current;
        }

        private static int MapCharacterS(string workingString, int current, ref MetaphoneBuffer metaphoneData, bool isSlavoGermanic, int last)
        {
//special cases 'island', 'isle', 'carlisle', 'carlysle'
            if (StringAt(workingString, (current - 1), strISL, strYSL))
            {
                current += 1;
                return current;
            }

            //special case 'sugar-'
            if ((current == 0) && StringAt(workingString, current, strSUGAR))
            {
                metaphoneData.Add(strX, strS);
                current += 1;
                return current;
            }

            if (StringAt(workingString, current, strSH))
            {
                //germanic
                if (StringAt(workingString, (current + 1), strHEIM, strHOEK, strHOLM, strHOLZ))
                {
                    metaphoneData.Add(charS);
                }
                else
                {
                    metaphoneData.Add(charX);
                }
                current += 2;
                return current;
            }

            //italian & armenian
            if (StringAt(workingString, current, strSIO, strSIA) || StringAt(workingString, current, strSIAN))
            {
                if (!isSlavoGermanic)
                {
                    metaphoneData.Add(strS, strX);
                }
                else
                {
                    metaphoneData.Add(charS);
                }
                current += 3;
                return current;
            }

            //german & anglicisations, e.g. 'smith' match 'schmidt', 'snider' match 'schneider'
            //also, -sz- in slavic language altho in hungarian it is pronounced 's'
            if (((current == 0)
                 && StringAt(workingString, 1, strM, strN, strL, strW))
                || StringAt(workingString, (current + 1), strZ))
            {
                metaphoneData.Add(strS, strX);
                if (StringAt(workingString, (current + 1), strZ))
                {
                    current += 2;
                }
                else
                {
                    current += 1;
                }
                return current;
            }

            if (StringAt(workingString, current, strSC))
            {
                //Schlesinger's rule
                if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 2) == charH)
                {
                    //dutch origin, e.g. 'school', 'schooner'
                    if (StringAt(workingString, (current + 3), strOO, strER, strEN, strUY, strED, strEM))
                    {
                        //'schermerhorn', 'schenker'
                        if (StringAt(workingString, (current + 3), strER, strEN))
                        {
                            metaphoneData.Add(strX, strSK);
                        }
                        else
                        {
                            metaphoneData.Add(strSK);
                        }
                        current += 3;
                        return current;
                    }
                    else
                    {
                        if ((current == 0) && !IsVowel(CharAtOrReturnSpaceWhenOutOfRange(workingString, 3)) && (CharAtOrReturnSpaceWhenOutOfRange(workingString, 3) != charW))
                        {
                            metaphoneData.Add(strX, strS);
                        }
                        else
                        {
                            metaphoneData.Add(charX);
                        }
                        current += 3;
                        return current;
                    }
                }

                if (StringAt(workingString, (current + 2), strI, strE, strY))
                {
                    metaphoneData.Add(charS);
                    current += 3;
                    return current;
                }
                //else
                metaphoneData.Add(strSK);
                current += 3;
                return current;
            }

            //french e.g. 'resnais', 'artois'
            if ((current == last) && StringAt(workingString, (current - 2), strAI, strOI))
            {
                metaphoneData.Add(string.Empty, strS);
            }
            else
            {
                metaphoneData.Add(charS);
            }

            if (StringAt(workingString, (current + 1), strS, strZ))
            {
                current += 2;
            }
            else
            {
                current += 1;
            }
            return current;
        }

        private static int MapCharacterG(string workingString, int current, ref MetaphoneBuffer metaphoneData, bool isSlavoGermanic)
        {
            if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charH)
            {
                if ((current > 0) && !IsVowel(CharAtOrReturnSpaceWhenOutOfRange(workingString, current - 1)))
                {
                    metaphoneData.Add(charK);
                    current += 2;
                    return current;
                }

                if (current < 3)
                {
                    //'ghislane', ghiradelli
                    if (current == 0)
                    {
                        if (CharAtOrReturnSpaceWhenOutOfRange(workingString,  2) == charI)
                        {
                            metaphoneData.Add(charJ);
                        }
                        else
                        {
                            metaphoneData.Add(charK);
                        }
                        current += 2;
                        return current;
                    }
                }
                //Parker's rule (with some further refinements) - e.g., 'hugh'
                if (((current > 1) && StringAt(workingString, (current - 2), strB, strH, strD)) //e.g., 'bough'
                    || ((current > 2) && StringAt(workingString, (current - 3), strB, strH, strD)) //e.g., 'broughton'
                    || ((current > 3) && StringAt(workingString, (current - 4), strB, strH)))
                {
                    current += 2;
                    return current;
                }
                else
                {
                    //e.g., 'laugh', 'McLaughlin', 'cough', 'gough', 'rough', 'tough'
                    if ((current > 2) && (CharAtOrReturnSpaceWhenOutOfRange(workingString, current - 1) == charU)
                        && StringAt(workingString, (current - 3), strC, strG, strL, strR, strT))
                    {
                        metaphoneData.Add(charF);
                    }
                    else if ((current > 0) && CharAtOrReturnSpaceWhenOutOfRange(workingString, current - 1) != charI)
                    {
                        metaphoneData.Add(charK);
                    }

                    current += 2;
                    return current;
                }
            }

            if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charN)
            {
                if ((current == 1) && IsVowel(CharAtOrReturnSpaceWhenOutOfRange(workingString, 0)) && !isSlavoGermanic)
                {
                    metaphoneData.Add(strKN, strN);
                }
                else
                //not e.g. 'cagney'
                    if (!StringAt(workingString, (current + 2), strEY)
                        && (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) != charY) && !isSlavoGermanic)
                    {
                        metaphoneData.Add(strN, strKN);
                    }
                    else
                    {
                        metaphoneData.Add(strKN);
                    }
                current += 2;
                return current;
            }

            //'tagliaro'
            if (StringAt(workingString, (current + 1), strLI) && !isSlavoGermanic)
            {
                metaphoneData.Add(strKL, strL);
                current += 2;
                return current;
            }

            //-ges-,-gep-,-gel-, -gie- at beginning
            if ((current == 0)
                && ((CharAtOrReturnSpaceWhenOutOfRange(workingString,  1) == charY)
                    || StringAt(workingString, 1, strES, strEP, strEB, strEL, strEY, strIB, strIL, strIN, strIE, strEI, strER)))
            {
                metaphoneData.Add(strK, strJ);
                current += 2;
                return current;
            }

            // -ger-,  -gy-
            if ((StringAt(workingString, (current + 1), strER)
                 || (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charY))
                && !StringAt(workingString, 0, strDANGER, strRANGER, strMANGER)
                && !StringAt(workingString, (current - 1), strE, strI)
                && !StringAt(workingString, (current - 1), strRGY, strOGY))
            {
                metaphoneData.Add(strK, strJ);
                current += 2;
                return current;
            }

            // italian e.g, 'biaggi'
            if (StringAt(workingString, (current + 1), strE, strI, strY)
                || StringAt(workingString, (current - 1), strAGGI, strOGGI))
            {
                //obvious germanic
                if ((StringAt(workingString, 0, strVANsp, strVONsp)
                     || StringAt(workingString, 0, strSCH))
                    || StringAt(workingString, (current + 1), strET))
                {
                    metaphoneData.Add(charK);
                }
                else
                //always soft if french ending
                    if (StringAt(workingString, (current + 1), strIERsp))
                    {
                        metaphoneData.Add(charJ);
                    }
                    else
                    {
                        metaphoneData.Add(strJ, strK);
                    }
                current += 2;
                return current;
            }

            if (CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 1) == charG)
            {
                current += 2;
            }
            else
            {
                current += 1;
            }
            metaphoneData.Add(charK);
            return current;

        }

        private static int MapCharacterC(string workingString, int current, ref MetaphoneBuffer metaphoneData)
        {
//various germanic
            if ((current > 1)
                && !IsVowel(CharAtOrReturnSpaceWhenOutOfRange(workingString, current - 2))
                && StringAt(workingString, (current - 1), strACH)
                && ((CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 2) != charI)
                    && ((CharAtOrReturnSpaceWhenOutOfRange(workingString, current + 2) != charE)
                        || StringAt(workingString, (current - 2), strBACHER, strMACHER))))
            {
                metaphoneData.Add(charK);
                current += 2;
                return current;
            }

            //special case 'caesar'
            if ((current == 0) && StringAt(workingString, current, strCAESAR))
            {
                metaphoneData.Add(charS);
                current += 2;
                return current;
            }

            //italian 'chianti'
            if (StringAt(workingString, current, strCHIA))
            {
                metaphoneData.Add(charK);
                current += 2;
                return current;
            }

            if (StringAt(workingString, current, strCH))
            {
                //find 'michael'
                if ((current > 0) && StringAt(workingString, current, strCHAE))
                {
                    metaphoneData.Add(strK, strX);
                    current += 2;
                    return current;
                }

                //greek roots e.g. 'chemistry', 'chorus'
                if ((current == 0)
                    && (StringAt(workingString,  1, strHARAC, strHARIS)
                        || StringAt(workingString,  1, strHOR, strHYM, strHIA, strHEM))
                    && !StringAt(workingString, 0, strCHORE))
                {
                    metaphoneData.Add(charK);
                    current += 2;
                    return current;
                }

                //germanic, greek, or otherwise 'ch' for 'kh' sound
                if ((StringAt(workingString, 0, strVANsp, strVONsp)
                     || StringAt(workingString, 0, strSCH)) // 'architect but not 'arch', 'orchestra', 'orchid'
                    || StringAt(workingString, (current - 2), strORCHES, strARCHIT, strORCHID)
                    || StringAt(workingString, (current + 2), strT, strS)
                    || ((StringAt(workingString, (current - 1), strA, strO, strU, strE)
                         || (current == 0)) //e.g., 'wachtler', 'wechsler', but not 'tichner'
                        && StringAt(workingString, (current + 2), strL, strR, strN, strM, strB, strH, strF, strV, strW, sp)))
                {
                    metaphoneData.Add(charK);
                }
                else
                {
                    if (current > 0)
                    {
                        if (StringAt(workingString, 0, strMC))
                        {
                            //e.g., "McHugh"
                            metaphoneData.Add(charK);
                        }
                        else
                        {
                            metaphoneData.Add(strX, strK);
                        }
                    }
                    else
                    {
                        metaphoneData.Add(charX);
                    }
                }
                current += 2;
                return current;
            }
            //e.g, 'czerny'
            if (StringAt(workingString, current, strCZ) && !StringAt(workingString, (current - 2), strWICZ))
            {
                metaphoneData.Add(strS, strX);
                current += 2;
                return current;
            }

            //e.g., 'focaccia'
            if (StringAt(workingString, (current + 1), strCIA))
            {
                metaphoneData.Add(charX);
                current += 3;
                return current;
            }

            //double 'C', but not if e.g. 'McClellan'
            if (StringAt(workingString, current, strCC) && !((current == 1) && (CharAtOrReturnSpaceWhenOutOfRange(workingString, 0) == charM)))
            {
                //'bellocchio' but not 'bacchus'
                if (StringAt(workingString, (current + 2), strI, strE, strH)
                    && !StringAt(workingString, (current + 2), strHU))
                {
                    //'accident', 'accede' 'succeed'
                    if (((current == 1) && (CharAtOrReturnSpaceWhenOutOfRange(workingString, current - 1) == charA))
                        || StringAt(workingString, (current - 1), strUCCEE, strUCCES))
                    {
                        metaphoneData.Add(strKS);
                    }
                    //'bacci', 'bertucci', other italian
                    else
                    {
                        metaphoneData.Add(charX);
                    }
                    current += 3;
                    return current;
                }
                else
                {
                    //Pierce's rule
                    metaphoneData.Add(charK);
                    current += 2;
                    return current;
                }
            }

            if (StringAt(workingString, current, strCK, strCG, strCQ))
            {
                metaphoneData.Add(charK);
                current += 2;
                return current;
            }

            if (StringAt(workingString, current, strCI, strCE, strCY))
            {
                //italian vs. english
                if (StringAt(workingString, current, strCIO, strCIE, strCIA))
                {
                    metaphoneData.Add(strS, strX);
                }
                else
                {
                    metaphoneData.Add(charS);
                }
                current += 2;
                return current;
            }

            //else
            metaphoneData.Add(charK);

            //name sent in 'mac caffrey', 'mac gregor
            if (StringAt(workingString, (current + 1), strspC, strspQ, strspG))
            {
                current += 3;
            }
            else if (StringAt(workingString, (current + 1), strC, strK, strQ)
                     && !StringAt(workingString, (current + 1), strCE, strCI))
            {
                current += 2;
            }
            else
            {
                current += 1;
            }
            return current;
        }

        static bool IsVowel(this char self)
        {
            return (self == charA) || (self == charE) || (self == charI)
                || (self == charO) || (self == charU) || (self == charY);
        }

        // ReSharper disable InconsistentNaming
        private const char charA = 'A';
        private const char charW = 'W';
        private const char charK = 'K';
        private const string strCZ = "CZ";
        private const string strWITZ = "WITZ";
        private const string strGN = "GN";
        private const string strKN = "KN";
        private const string strPN = "PN";
        private const string strWR = "WR";
        private const string strPS = "PS";
        private const char charX = 'X';
        private const string strS = "S";
        private const char charE = 'E';
        private const char charI = 'I';
        private const char charO = 'O';
        private const char charU = 'U';
        private const char charY = 'Y';
        private const char charB = 'B';
        private const char charAdash = 'Ã';
        private const string strACH = "ACH";
        private const string strBACHER = "BACHER";
        private const string strMACHER = "MACHER";
        private const string strK = "K";
        private const string strCAESAR = "CAESAR";
        private const string strCHIA = "CHIA";
        private const string strCH = "CH";
        private const string strCHAE = "CHAE";
        private const string strX = "X";
        private const string strHARAC = "HARAC";
        private const string strHARIS = "HARIS";
        private const string strHOR = "HOR";
        private const string strHYM = "HYM";
        private const string strHIA = "HIA";
        private const string strHEM = "HEM";
        private const string strCHORE = "CHORE";
        private const string strVANsp = "VAN ";
        private const string strVONsp = "VON ";
        private const string strSCH = "SCH";
        private const string strORCHES = "ORCHES";
        private const string strARCHIT = "ARCHIT";
        private const string strORCHID = "ORCHID";
        private const string strT = "T";
        private const string strA = "A";
        private const string strO = "O";
        private const string strU = "U";
        private const string strE = "E";
        private const string strL = "L";
        private const string strR = "R";
        private const string strN = "N";
        private const string strM = "M";
        private const string strB = "B";
        private const string strH = "H";
        private const string strF = "F";
        private const string strV = "V";
        private const string strW = "W";
        private const string sp = " ";
        private const string strMC = "MC";
        private const string strWICZ = "WICZ";
        private const string strCIA = "CIA";
        private const string strCC = "CC";
        private const char charM = 'M';
        private const string strI = "I";
        private const string strHU = "HU";
        private const string strUCCEE = "UCCEE";
        private const string strUCCES = "UCCES";
        private const string strKS = "KS";
        private const string strCK = "CK";
        private const string strCG = "CG";
        private const string strCQ = "CQ";
        private const string strCI = "CI";
        private const string strCE = "CE";
        private const string strCY = "CY";
        private const string strCIO = "CIO";
        private const string strCIE = "CIE";
        private const string strspC = " C";
        private const string strspQ = " Q";
        private const string strspG = " G";
        private const string strC = "C";
        private const string strQ = "Q";
        private const char charC = 'C';
        private const char charD = 'D';
        private const string strDG = "DG";
        private const string strY = "Y";
        private const string strJ = "J";
        private const string strTK = "TK";
        private const string strDT = "DT";
        private const string strDD = "DD";
        private const char charF = 'F';
        private const char charG = 'G';
        private const char charH = 'H';
        private const string strD = "D";
        private const string strG = "G";
        private const char charN = 'N';
        private const string strEY = "EY";
        private const string strLI = "LI";
        private const string strKL = "KL";
        private const string strES = "ES";
        private const string strEP = "EP";
        private const string strEB = "EB";
        private const string strEL = "EL";
        private const string strIB = "IB";
        private const string strIL = "IL";
        private const string strIN = "IN";
        private const string strIE = "IE";
        private const string strEI = "EI";
        private const string strER = "ER";
        private const string strDANGER = "DANGER";
        private const string strRANGER = "RANGER";
        private const string strMANGER = "MANGER";
        private const string strRGY = "RGY";
        private const string strOGY = "OGY";
        private const string strAGGI = "AGGI";
        private const string strOGGI = "OGGI";
        private const string strIERsp = "IER ";
        private const string strJOSE = "JOSE";
        private const string strSANsp = "SAN ";
        private const string strZ = "Z";
        private const char charJ = 'J';
        private const char charL = 'L';
        private const string strILLO = "ILLO";
        private const string strILLA = "ILLA";
        private const string strALLE = "ALLE";
        private const string strAS = "AS";
        private const string strOS = "OS";
        private const string strUMB = "UMB";
        private const char charOdash = 'Ð';
        private const char charP = 'P';
        private const string strP = "P";
        private const char charQ = 'Q';
        private const string strME = "ME";
        private const string strMA = "MA";
        private const char charR = 'R';
        private const char charS = 'S';
        private const string strISL = "ISL";
        private const string strYSL = "YSL";
        private const string strSUGAR = "SUGAR";
        private const string strSH = "SH";
        private const string strHEIM = "HEIM";
        private const string strHOEK = "HOEK";
        private const string strHOLM = "HOLM";
        private const string strHOLZ = "HOLZ";
        private const string strSIO = "SIO";
        private const string strSIA = "SIA";
        private const string strSIAN = "SIAN";
        private const string strSC = "SC";
        private const string strOO = "OO";
        private const string strEN = "EN";
        private const string strUY = "UY";
        private const string strED = "ED";
        private const string strEM = "EM";
        private const string strSK = "SK";
        private const string strAI = "AI";
        private const string strOI = "OI";
        private const string strTION = "TION";
        private const string strTIA = "TIA";
        private const string strTCH = "TCH";
        private const char charT = 'T';
        private const string strTH = "TH";
        private const string strTTH = "TTH";
        private const string strOM = "OM";
        private const string strAM = "AM";
        private const char charV = 'V';
        private const string strWH = "WH";
        private const string strEWSKI = "EWSKI";
        private const string strEWSKY = "EWSKY";
        private const string strOWSKI = "OWSKI";
        private const string strOWSKY = "OWSKY";
        private const string strFX = "FX";
        private const string strTS = "TS";
        private const string strEAU = "EAU";
        private const string strIAU = "IAU";
        private const string strAU = "AU";
        private const string strOU = "OU";
        private const char charZ = 'Z';
        private const string strZA = "ZA";
        private const string strZI = "ZI";
        private const string strZO = "ZO";
        private const string strET = "ET";
        // ReSharper restore InconsistentNaming

        public static bool StringAt(this string self, int startIndex, string a, string b)
        {
            if (startIndex < 0)
            {
                startIndex = 0;
            }

            unsafe
            {
                fixed (char* selfPtr = self, valueAPtr = a, valueBPtr = b)
                {
                    var selfLength = self.Length;
                    var charAtParams = new CharAtSearchTupleValues(valueAPtr, a.Length, valueBPtr, b.Length);

                    return StringAt(selfPtr, selfLength, startIndex, charAtParams);
                }
            }
        }

        static bool StringAt(this string self, int startIndex, string a, string b, string c)
        {
            if (startIndex < 0)
            {
                startIndex = 0;
            }

            unsafe
            {
                fixed (char* selfPtr = self, valueAPtr = a, valueBPtr = b, valueCPtr = c)
                {
                    var selfLength = self.Length;
                    var charAtParams = new CharAtSearchTripleValues(valueAPtr, a.Length, valueBPtr, b.Length, valueCPtr, c.Length);

                    return StringAt(selfPtr, selfLength, startIndex, charAtParams);
                }
            }
        }
        public static bool StringAtOld(this string self, int startIndex, string a, string b)
        {
            return StringAt(self, startIndex, a) || StringAt(self, startIndex, b);
        }

        static bool StringAt(this string self, int startIndex, string a, string b, string c, string d)
        {
            return StringAt(self, startIndex, a, b) || StringAt(self, startIndex, c, d);
        }

        static bool StringAt(this string self, int startIndex, string a, string b, string c, string d, string e)
        {
            return StringAt(self, startIndex, a, b, c) || StringAt(self, startIndex, d, e);
        }

        static bool StringAt(this string self, int startIndex, string a, string b, string c, string d, string e, string f)
        {
            return StringAt(self, startIndex, a, b, c) || StringAt(self, startIndex, d, e, f);
        }

        static bool StringAt(this string self, int startIndex, string a, string b, string c, string d, string e, string f, string g, string h)
        {
            return StringAt(self, startIndex, a, b, c, d) || StringAt(self, startIndex, e, f, g, h);
        }

        static bool StringAt(this string self, int startIndex, string a, string b, string c, string d, string e, string f, string g, string h, string i, string j)
        {
            return StringAt(self, startIndex, a, b, c, d) || StringAt(self, startIndex, e, f, g, h) || StringAt(self, startIndex, i, j);
        }

        static bool StringAt(this string self, int startIndex, string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k)
        {
            return StringAt(self, startIndex, a, b, c, d) || StringAt(self, startIndex, e, f, g, h) || StringAt(self, startIndex, i, j, k);
        }

        internal static bool StringAt(this string self, int startIndex, string value)
        {
            if (startIndex < 0)
            {
                startIndex = 0;
            }

            unsafe
            {
                fixed (char* selfPtr = self, valuePtr = value)
                {
                    var selfLength = self.Length;
                    var valueLength = value.Length;

                    return StringAt(selfPtr, selfLength, startIndex, valuePtr, valueLength);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool StringAt(char* selfPtr, int selfLength, int startIndex, char* valuePtr, int valueLength)
        {
            if (valueLength == 0)
            {
                if (selfLength == 0 && startIndex == 0)
                {
                    return true;
                }

                return false;
            }

            if (selfLength - startIndex - valueLength < 0)
            {
                return false;
            }

            char* startSelf = selfPtr + startIndex;
            char* startValue = valuePtr;
            int pos = 0;

            while (pos < valueLength)
            {
                if (*startSelf != *startValue)
                {
                    return false;
                }

                startSelf++;
                startValue++;
                pos++;
            }

            return true;
        }

        private static unsafe bool StringAt(char* selfPtr, int selfLength, int startIndex, in CharAtSearchTupleValues searchValues)
        {
            if (selfLength == 0 && startIndex == 0)
            {
                if (searchValues.ValueALength == 0 || searchValues.ValueBLength == 0)
                {
                    return true;//Empty string in an empty string
                }

                return false;
            }

            if (selfLength - startIndex - searchValues.ValueALength < 0)
            {
                return false;
            }

            if (selfLength - startIndex - searchValues.ValueBLength < 0)
            {
                return false;
            }

            var maxSearchLength = Math.Max(searchValues.ValueALength, searchValues.ValueBLength);

            char* startSelf = selfPtr + startIndex;
            char* startAValue = searchValues.ValueAPtr;
            char* startBValue = searchValues.ValueBPtr;
            int pos = 0;

            var foundChar = false;

            while (pos < maxSearchLength)
            {
                if ( pos < searchValues.ValueALength && *startSelf == *startAValue)
                {
                    foundChar = true;
                } else if ( pos < searchValues.ValueBLength && *startSelf == *startBValue )
                {
                    foundChar = true;
                }
                else
                {
                    foundChar = false;
                }

                if (foundChar == false)
                {
                    return false;
                }

                startSelf++;
                startAValue++;
                startBValue++;
                pos++;
            }

            return foundChar;
        }

        private static unsafe bool StringAt(char* selfPtr, int selfLength, int startIndex, in CharAtSearchTripleValues searchValues)
        {
            if (selfLength == 0 && startIndex == 0)
            {
                if (searchValues.ValueALength == 0 || searchValues.ValueBLength == 0 || searchValues.ValueCLength == 0)
                {
                    return true;//Empty string in an empty string
                }

                return false;
            }

            if (selfLength - startIndex - searchValues.ValueALength < 0)
            {
                return false;
            }

            if (selfLength - startIndex - searchValues.ValueBLength < 0)
            {
                return false;
            }

            if (selfLength - startIndex - searchValues.ValueCLength < 0)
            {
                return false;
            }

            var maxSearchLength = Math.Max(Math.Max(searchValues.ValueALength, searchValues.ValueBLength), searchValues.ValueCLength);

            char* startSelf = selfPtr + startIndex;
            char* startAValue = searchValues.ValueAPtr;
            char* startBValue = searchValues.ValueBPtr;
            char* startCValue = searchValues.ValueCPtr;
            int pos = 0;

            var foundChar = false;

            while (pos < maxSearchLength)
            {
                if ( pos < searchValues.ValueALength && *startSelf == *startAValue)
                {
                    foundChar = true;
                } else if ( pos < searchValues.ValueBLength && *startSelf == *startBValue )
                {
                    foundChar = true;
                }
                else if ( pos < searchValues.ValueCLength && *startSelf == *startCValue )
                {
                    foundChar = true;
                } else
                {
                    foundChar = false;
                }

                if (foundChar == false)
                {
                    return false;
                }

                startSelf++;
                startAValue++;
                startBValue++;
                startCValue++;
                pos++;
            }

            return foundChar;
        }

        readonly unsafe struct CharAtSearchTupleValues
        {
            public readonly char* ValueAPtr;
            public readonly int ValueALength;
            public readonly char* ValueBPtr;
            public readonly int ValueBLength;

            public CharAtSearchTupleValues(char* valueAPtr, int valueALength, char* valueBPtr, int valueBLength)
            {
                ValueAPtr = valueAPtr;
                ValueALength = valueALength;
                ValueBPtr = valueBPtr;
                ValueBLength = valueBLength;
            }
        }

        readonly unsafe struct CharAtSearchTripleValues
        {
            public readonly char* ValueAPtr;
            public readonly int ValueALength;
            public readonly char* ValueBPtr;
            public readonly int ValueBLength;

            public readonly char* ValueCPtr;
            public readonly int ValueCLength;

            public CharAtSearchTripleValues(char* valueAPtr, int valueALength, char* valueBPtr, int valueBLength, char* valueCPtr, int valueCLength)
            {
                ValueAPtr = valueAPtr;
                ValueALength = valueALength;
                ValueBPtr = valueBPtr;
                ValueBLength = valueBLength;
                ValueCPtr = valueCPtr;
                ValueCLength = valueCLength;
            }
        }
    }
}
