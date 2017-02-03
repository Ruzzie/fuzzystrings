using System;

namespace Ruzzie.FuzzyStrings
{
    public class WhitespaceTokenizer : IStringTokenizer
    {
        private static readonly char[] SplitChars = null;
        public string[] Tokenize(string input)
        {
            return input.Split(SplitChars,StringSplitOptions.RemoveEmptyEntries);
        }
    }
}