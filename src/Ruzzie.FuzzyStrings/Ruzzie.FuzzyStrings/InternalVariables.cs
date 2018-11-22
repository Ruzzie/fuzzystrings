using System;
using System.Collections.Generic;

#if HAVE_VISUALBASIC_DEVICES
using Microsoft.VisualBasic.Devices;
#elif !PORTABLE && HAVE_PROCESS
using System.Diagnostics;
#endif
using Ruzzie.Caching;

namespace Ruzzie.FuzzyStrings
{
    public static class InternalVariables
    {
        public static readonly int DefaultCacheItemSizeInMb;
        public static int AverageStringSizeInBytes = 284;
        public static readonly IEqualityComparer<string> StringComparerForCacheKey = new StringComparerOrdinalIgnoreCaseFNV1AHash();
        public static readonly int MaxCacheSizeInMb;

        static InternalVariables()
        {
            int mbOfMemoryAvailable = 0;
            try
            {
                long bytesOfMemoryAvailable;                
#if HAVE_VISUALBASIC_DEVICES
                ComputerInfo info = new ComputerInfo();
                bytesOfMemoryAvailable = (long) info.AvailablePhysicalMemory;
#elif !PORTABLE && HAVE_PROCESS
                using(Process currentProcess = Process.GetCurrentProcess())
                {
                    bytesOfMemoryAvailable = currentProcess.MaxWorkingSet.ToInt64() * 16;
                }
#else
                bytesOfMemoryAvailable = GC.GetTotalMemory(false) * 16;                                    
#endif
                mbOfMemoryAvailable = (int) ((bytesOfMemoryAvailable / 1024) / 1024);
            }
            finally
            {
                DefaultCacheItemSizeInMb = Math.Max(1, mbOfMemoryAvailable / 400);
                MaxCacheSizeInMb = Math.Max(DefaultCacheItemSizeInMb * 2, mbOfMemoryAvailable / 8);
            }
        }
    }
}
