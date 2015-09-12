using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DuoVia.FuzzyStrings;
using NUnit.Framework;

namespace fuzzytest
{
    [TestFixture]
    public class MemoryCacheWithItemLimitTest
    {
        private static Stopwatch TimeParralelCacheWrite(MemoryCacheWithItemLimit<string> cache)
        {
            var hammertime = true;
            var loopCount = 250000;

            Task hammerOnCacheTask = Task.Run(() =>
            {
                while (hammertime)
                {
                    for (var i = 0; i < loopCount; i++)
                    {
                        if (hammertime == false)
                        {
                            break;
                        }

                        if (i%2 == 0)
                        {
                            cache.GetOrAdd(i.ToString(), s => "No hammertime for: " + i.ToString());
                        }
                        else
                        {
                            cache.GetOrAdd((i - 5).ToString(), s => "No hammertime for: " + (i - 5).ToString());
                        }
                    }
                }
            });

            Stopwatch timer = new Stopwatch();

            timer.Start();
            Parallel.For(0, loopCount, i => { cache.GetOrAdd(i.ToString(), s => "No hammertime for: " + i.ToString()); });

            timer.Stop();
            hammertime = false;
            hammerOnCacheTask.Wait(10);
            //hammerOnCacheTask.Dispose();
            return timer;
        }

        private static Stopwatch TimeSingleThreadCacheWrite(MemoryCacheWithItemLimit<string> cache)
        {
            Stopwatch timer = new Stopwatch();

            timer.Start();
            for (var i = 0; i < 250000; i ++)
            {
                if (i%2 == 0)
                {
                    cache.GetOrAdd(i.ToString(), s => "No hammertime for: " + i.ToString());
                }
                else
                {
                    cache.GetOrAdd((i - 5).ToString(), s => "No hammertime for: " + (i - 5).ToString());
                }
            }
            timer.Stop();
            return timer;
        }

        [Test]
        public void CachedFuzzyMatchFasterThanUncached()
        {
            int numberOfIterationsToPerform = 1000;

            AssertCachedMethodIsFasterThanUncachedMethod(
                s => s.FuzzyMatchUncached("Compare to string FuzzyMatchUn").ToString(CultureInfo.InvariantCulture),
                s => s.FuzzyMatch("Compare to string FuzzyMatchCa").ToString(CultureInfo.InvariantCulture),
                numberOfIterationsToPerform,
                "FuzzyMatch");
        }     


        private static void AssertCachedMethodIsFasterThanUncachedMethod(Func<string, string> uncachedMethod, Func<string, string> cachedMethod,
            int numberOfIterationsToPerform, string methodname)
        {
            //first warmup
            Task<Tuple<double, double>> cachedTask =
           Task.Run(() => ExecuteMethodWithTimingAverageResults(cachedMethod, numberOfIterationsToPerform));
            Tuple<double, double> cachedTimings = cachedTask.Result;

            Task<Tuple<double, double>> uncachedTask =
                Task.Run(() => ExecuteMethodWithTimingAverageResults(uncachedMethod, numberOfIterationsToPerform));
            Tuple<double, double> uncachedTimings = uncachedTask.Result;

            //now for real
             cachedTask =
         Task.Run(() => ExecuteMethodWithTimingAverageResults(cachedMethod, numberOfIterationsToPerform));
             cachedTimings = cachedTask.Result;

             uncachedTask =
                Task.Run(() => ExecuteMethodWithTimingAverageResults(uncachedMethod, numberOfIterationsToPerform));
            uncachedTimings = uncachedTask.Result;


            Console.WriteLine("Timing results for: " + methodname);
            Console.WriteLine("Cached Result:     \t Avg ticks: " + cachedTimings.Item1);
            Console.WriteLine("Uncached Result:   \t Avg ticks: " + uncachedTimings.Item1);
            Console.WriteLine("Cached Result:     \t Avg ms: " + cachedTimings.Item2);
            Console.WriteLine("Uncached Result:   \t Avg ms: " + uncachedTimings.Item2);

            Assert.That(cachedTimings.Item1, Is.LessThan(uncachedTimings.Item1));
            Assert.That(cachedTimings.Item2, Is.LessThan(uncachedTimings.Item2));
        }

        private static Tuple<double, double> ExecuteMethodWithTimingAverageResults(Func<string, string> methodToExecute, int numberOfIterationsToPerform)
        {
            ConcurrentBag<long> allTickTimes = new ConcurrentBag<long>();
            ConcurrentBag<long> allElapsedTimesMilliseconds = new ConcurrentBag<long>();
            Parallel.For(0, 2, i =>
            {
                Stopwatch timer = ExecuteMethodWithTimingInLoop(methodToExecute, numberOfIterationsToPerform);
                allTickTimes.Add(timer.ElapsedTicks);
                allElapsedTimesMilliseconds.Add(timer.ElapsedMilliseconds);
            });
            Tuple<double, double> timings = new Tuple<double, double>(allTickTimes.Average(), allElapsedTimesMilliseconds.Average());
            return timings;
        }

        private static Stopwatch ExecuteMethodWithTimingInLoop(Func<string, string> functionToExecute, int numberOfIterationsToPerform)
        {         

            List<string> sameRandomWordsList = new List<string>();
            Random random = new Random(numberOfIterationsToPerform);
            for (int i = 0; i < numberOfIterationsToPerform; i++)
            {              
                var word = string.Concat(Enumerable.Repeat("AE IOU UOIEA When enchanted creature dies, return that card to the battlefield under its owner's control.", 105).Select(s =>
                {
                    int next = random.Next('A', 'Z' + 1);
                    return new string(new char[] {(char) next, s[next%105]});
                }).ToArray());

                sameRandomWordsList.Add(word);
            }

            Stopwatch timer = new Stopwatch();
            timer.Start();

            Parallel.For(0, numberOfIterationsToPerform, j =>
            {
                
                string word = sameRandomWordsList[j];
               // Debug.WriteLine(word);
                functionToExecute.Invoke(word);
            });

            timer.Stop();
            return timer;
        }

        [Test]
        public void MultiThreadPerformanceTest()
        {
            MemoryCacheWithItemLimit<string> cache = new MemoryCacheWithItemLimit<string>(250000, true);


            ConcurrentBag<long> allTickTimes = new ConcurrentBag<long>();
            ConcurrentBag<long> allElapsedTimesMilliseconds = new ConcurrentBag<long>();

            Parallel.For(0, 25, i =>
            {
                Task.Run(() => TimeParralelCacheWrite(cache));
                Stopwatch timer = TimeParralelCacheWrite(cache);

                allTickTimes.Add(timer.ElapsedTicks);
                allElapsedTimesMilliseconds.Add(timer.ElapsedMilliseconds);
            });

            double averageTickTime = allTickTimes.Average();
            double averageTimeMilliseconds = allElapsedTimesMilliseconds.Average();

            Console.WriteLine("Average ticks: " + averageTickTime);
            Console.WriteLine("Average ms: " + averageTimeMilliseconds);

            Assert.That(averageTickTime, Is.LessThanOrEqualTo(3005055));
            Assert.That(averageTimeMilliseconds, Is.LessThanOrEqualTo(1826)); //15298 spinlock || 1826 normal lock || 800 - 1300 MemoryCache ||

            Assert.That(cache.CacheItemCount, Is.LessThanOrEqualTo(260000));
        }

       
        [Test]
        public void StringContainsAlternativeTest()
        {
            string sourceString = "Beast of Burden's power and toughness are each equal to the number of creatures on the battlefield".ToLowerInvariant();
            string stringToFind = "to the number of creatures";
            
            //classic replace method with tolowercase
            Func<bool> classicContains = () => sourceString.Contains(stringToFind);

            Func<bool> alternativereplace = () => sourceString.ContainsString(stringToFind);


            TimeSpan classicTimings = ExecuteMethodAndReturnTimings(1000000, classicContains);
            TimeSpan alternativereplaceTimings = ExecuteMethodAndReturnTimings(1000000, alternativereplace);

            Console.WriteLine("Classic timing: " + classicTimings.TotalMilliseconds);
            Console.WriteLine("Alternative timing: " + alternativereplaceTimings.TotalMilliseconds);
        }

        [Test]
        public void StringAnyAlternativeTest()
        {
            string sourceString = "Beast of Burden's power and toughness are each equal to the number of creatures on the battlefield";//.ToLowerInvariant();
            string stringToFind = "to the number of creatures";

            //classic replace method with tolowercase
            Func<bool> classicContains = () => sourceString.ToLowerInvariant().Contains(stringToFind.ToLowerInvariant());

            Func<bool> alternativereplace = () => sourceString.AnyString(stringToFind);


            TimeSpan classicTimings = ExecuteMethodAndReturnTimings(1000000, classicContains);
            TimeSpan alternativereplaceTimings = ExecuteMethodAndReturnTimings(1000000, alternativereplace);

            Console.WriteLine("Classic timing: " + classicTimings.TotalMilliseconds);
            Console.WriteLine("Alternative timing: " + alternativereplaceTimings.TotalMilliseconds);
        }


        public TimeSpan ExecuteMethodAndReturnTimings<TResult>(int numberOfTimesToExecute, Func<TResult> funcToExecute)
        {
          
            Stopwatch timer = new Stopwatch();
            timer.Start();


            for (int i = 0; i < numberOfTimesToExecute; i++)
            {
                var result = funcToExecute.Invoke();
            }

            timer.Stop();

            return timer.Elapsed;
        }

        [Test]
        public void NullKeyTest()
        {
            MemoryCacheWithItemLimit<string> cache = new MemoryCacheWithItemLimit<string>(5);

            Assert.That(() => cache.GetOrAdd(null, s => null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void NullValuesTest()
        {
            MemoryCacheWithItemLimit<string> cache = new MemoryCacheWithItemLimit<string>(4);

            cache.GetOrAdd("MyKey0", s => null);
            cache.GetOrAdd("MyKey1", s => null);
            cache.GetOrAdd("MyKey2", s => null);
            cache.GetOrAdd("MyKey3", s => null);
            cache.GetOrAdd("MyKey4", s => null);
            cache.GetOrAdd("MyKey5", s => null);
            cache.GetOrAdd("MyKey6", s => null);
            cache.GetOrAdd("MyKey7", s => null);
            cache.GetOrAdd("MyKey8", s => null);
            cache.GetOrAdd("MyKey9", s => null);
            cache.GetOrAdd("MyKey10", s => null);
            cache.GetOrAdd("MyKey11", s => null);
            cache.GetOrAdd("MyKey12", s => null);
        }

        [Test]
        public void SingleThreadPerformanceTest()
        {
            MemoryCacheWithItemLimit<string> cache = new MemoryCacheWithItemLimit<string>(250000);
            ConcurrentBag<long> allTickTimes = new ConcurrentBag<long>();
            ConcurrentBag<long> allElapsedTimesMilliseconds = new ConcurrentBag<long>();

            for (var i = 0; i < 10; i++)
            {
                Stopwatch timer = TimeSingleThreadCacheWrite(cache);
                allTickTimes.Add(timer.ElapsedTicks);
                allElapsedTimesMilliseconds.Add(timer.ElapsedMilliseconds);
            }

            double averageTickTime = allTickTimes.Average();
            double averageTimeMilliseconds = allElapsedTimesMilliseconds.Average();

            Console.WriteLine("Average ticks: " + averageTickTime);
            Console.WriteLine("Average ms: " + averageTimeMilliseconds);

            Assert.That(averageTickTime, Is.LessThanOrEqualTo(1033126));
            Assert.That(averageTimeMilliseconds, Is.LessThanOrEqualTo(343));
        }

        [Test]
        public void Smokey()
        {
            MemoryCacheWithItemLimit<string> cache = new MemoryCacheWithItemLimit<string>(5);

            string valueOne = cache.GetOrAdd("1", key => "1Value");
            string valueTwo = cache.GetOrAdd("2", key => "2Value");
            string valueThree = cache.GetOrAdd("3", key => "3Value");
            string valueFour = cache.GetOrAdd("4", key => "4Value");
            string valueFive = cache.GetOrAdd("5", key => "5Value");

            string valueSix = cache.GetOrAdd("6", key => "6Value");

            Assert.That(cache.GetOrAdd("1", key => "1ValueUpdated"), Is.EqualTo("1Value").Or.EqualTo("1ValueUpdated")); //because of the delayed trim of the cache
            Assert.That(cache.GetOrAdd("6", key => "DoNotUpdate6"), Is.EqualTo("6Value").Or.EqualTo("DoNotUpdate6"));
            Assert.That(cache.GetOrAdd("5", key => "DoNotUpdate5"), Is.EqualTo("5Value").Or.EqualTo("DoNotUpdate5"));
            Assert.That(cache.GetOrAdd("4", key => "DoNotUpdate4"), Is.EqualTo("4Value").Or.EqualTo("DoNotUpdate4"));
            Assert.That(cache.GetOrAdd("3", key => "DoNotUpdate3"), Is.EqualTo("3Value").Or.EqualTo("DoNotUpdate3"));
            Assert.That(cache.GetOrAdd("2", key => "Update2"), Is.EqualTo("Update2").Or.EqualTo("2Value"));
        }
    }   
}