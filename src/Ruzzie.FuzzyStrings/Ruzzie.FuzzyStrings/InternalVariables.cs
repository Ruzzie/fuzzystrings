using System;
using System.Diagnostics;
using Ruzzie.Common.Caching;

namespace Ruzzie.FuzzyStrings
{
    public static class InternalVariables
    {
        public static readonly int DefaultCacheItemSizeInMb;
        public static readonly int AverageStringSizeInBytes = 284;
        public static readonly int MaxCacheSizeInMb;

        public static readonly StringComparerOrdinalIgnoreCaseFNV1AHash StringComparerForCacheKey =
            new StringComparerOrdinalIgnoreCaseFNV1AHash();

        static InternalVariables()
        {
            int mbOfMemoryAvailable = 0;
            try
            {
                long bytesOfMemoryAvailable;

                using (Process currentProcess = Process.GetCurrentProcess())
                {
                    bytesOfMemoryAvailable = currentProcess.MaxWorkingSet.ToInt64() * 16;
                }

                mbOfMemoryAvailable = (int)((bytesOfMemoryAvailable / 1024) / 1024);
            }
            finally
            {
                DefaultCacheItemSizeInMb = Math.Max(1, mbOfMemoryAvailable   / 400);
                MaxCacheSizeInMb         = Math.Max(DefaultCacheItemSizeInMb * 2, mbOfMemoryAvailable / 8);
            }
        }
    }
}