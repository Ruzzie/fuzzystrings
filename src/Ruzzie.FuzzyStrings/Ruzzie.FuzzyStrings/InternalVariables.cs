﻿using System;
using System.Diagnostics;
using Ruzzie.Common.Caching;

namespace Ruzzie.FuzzyStrings
{
    public static class InternalVariables
    {
        public static readonly long DefaultCacheItemSizeInMb;
        public static readonly int  AverageStringSizeInBytes = 284;
        public static readonly long MaxCacheSizeInMb;

        public static readonly int MaxNumberOfStringsInCache;

        public static readonly StringComparerOrdinalIgnoreCaseFNV1AHash StringComparerForCacheKey =
            new StringComparerOrdinalIgnoreCaseFNV1AHash();

        static InternalVariables()
        {
            long mbOfMemoryAvailable = 0;
            try
            {
                long bytesOfMemoryAvailable;

                using (Process currentProcess = Process.GetCurrentProcess())
                {
                    bytesOfMemoryAvailable = currentProcess.WorkingSet64;
                }

                mbOfMemoryAvailable = (bytesOfMemoryAvailable / 1024) / 1024;
            }
            finally
            {
                DefaultCacheItemSizeInMb = Math.Max(1, mbOfMemoryAvailable   / 400L);
                MaxCacheSizeInMb         = Math.Max(DefaultCacheItemSizeInMb * 2, mbOfMemoryAvailable / 8);

                MaxNumberOfStringsInCache = CalculateMaxNumberOfStrings(MaxCacheSizeInMb, AverageStringSizeInBytes);
            }
        }

        internal static int CalculateMaxNumberOfStrings(long maxCacheSizeInMb, int averageStringSizeInBytes)
        {
            var maxNumberOfStringUnclamped =
                Math.Max(1024L, (Math.Min(4096, maxCacheSizeInMb) * 1024L * 1024L)) / averageStringSizeInBytes;
            return (int)Math.Clamp(maxNumberOfStringUnclamped, 1024, Array.MaxLength / 4096);
        }
    }
}