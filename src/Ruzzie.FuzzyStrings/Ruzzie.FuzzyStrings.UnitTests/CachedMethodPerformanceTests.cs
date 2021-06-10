using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Ruzzie.FuzzyStrings.UnitTests
{
    [TestFixture]
   // [Ignore("Only use when testing algorithms")]
    public class CachedMethodPerformanceTests
    {
        [Test]
        public void CachedFuzzyMatchFasterThanUncached()
        {
            int numberOfIterationsToPerform = 500;

            AssertCachedMethodIsFasterThanUncachedMethod(
                s => s.FuzzyMatchUncached("Compare to string FuzzyMatchUn").ToString(CultureInfo.InvariantCulture),
                s => s.FuzzyMatch("Compare to string FuzzyMatchCa").ToString(CultureInfo.InvariantCulture),
                numberOfIterationsToPerform,
                "FuzzyMatch");
        }

        private static void AssertCachedMethodIsFasterThanUncachedMethod(Func<string, string> uncachedMethod, Func<string, string> cachedMethod,
            int numberOfIterationsToPerform, string methodName)
        {
            //first warmup
            Task<Tuple<double, double>> cachedTask =
                RunTask(cachedMethod, numberOfIterationsToPerform);
            Tuple<double, double> cachedTimings = cachedTask.Result;

            Task<Tuple<double, double>> uncachedTask =
                RunTask(uncachedMethod, numberOfIterationsToPerform);
            Tuple<double, double> uncachedTimings = uncachedTask.Result;

            //now for real
            cachedTask =  RunTask(cachedMethod, numberOfIterationsToPerform);
            cachedTimings = cachedTask.Result;

            uncachedTask =
                RunTask(uncachedMethod, numberOfIterationsToPerform);
            uncachedTimings = uncachedTask.Result;


            Console.WriteLine("Timing results for: " + methodName);
            Console.WriteLine("Uncached Result:   \t Avg ticks: " + uncachedTimings.Item1);
            Console.WriteLine("Cached Result:     \t Avg ticks: " + cachedTimings.Item1);
            Console.WriteLine("Uncached Result:   \t Avg ms: " + uncachedTimings.Item2);
            Console.WriteLine("Cached Result:     \t Avg ms: " + cachedTimings.Item2);


            Assert.That(cachedTimings.Item1, Is.LessThan(uncachedTimings.Item1));
            Assert.That(cachedTimings.Item2, Is.LessThan(uncachedTimings.Item2));
        }

        private static Task<Tuple<double, double>> RunTask(Func<string, string> cachedMethod,
                                                           int                  numberOfIterationsToPerform)
        {

            return Task.Run(() => ExecuteMethodWithTimingAverageResults(cachedMethod, numberOfIterationsToPerform));
        }

        [Test]
        public void LongestCommonSubsequenceAlternativeWithoutBacktrackingTest()
        {
            Random random = new Random(1337);

            int next = random.Next();
            string sourceString = "Beast of Burden's power and toughness are each equal to the number of creatures" + next;//.ToLowerInvariant();
            string stringToFind = "to the number of creatures" + next;


            //with backtracking, included lcs in result
            Func<LongestCommonSubsequenceResult> original = () =>
                (sourceString + next).LongestCommonSubsequenceUncached(stringToFind + next, true, true);

            //no backtracking, does not include lcs in result
            Func<LongestCommonSubsequenceResult> alternative = () =>
                (sourceString + next).LongestCommonSubsequenceWithoutSubsequenceAlternative(stringToFind + next, true);

            var numberOfTimesToExecute = 1000;

            //warmup
            TimeSpan alternativeTimings = ExecuteMethodAndReturnTimings(250, alternative);
            TimeSpan originalTimings = ExecuteMethodAndReturnTimings(250, original);

            //execute
            alternativeTimings = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, alternative);
            originalTimings = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, original);

            Console.WriteLine("Original timing: " + originalTimings.TotalMilliseconds);
            Console.WriteLine("Alternative timing: " + alternativeTimings.TotalMilliseconds);
        }

        [Test][Ignore("Enable for testing custom algorithm")]
        public void StringContainsAlternativeTest()
        {
            Random random = new Random();

            string sourceString = "Beast of Burden's power and toughness are each equal to the number of creatures on the battlefield".ToLowerInvariant();
            string stringToFind = "to the number of creatures";

            //classic replace method with tolowercase
            Func<bool> classicContains = () => sourceString.Contains(stringToFind + random.Next());

            //note: when we find an alternative algo, we can test it here
            //Func<bool> alternativereplace = () => sourceString.ContainsString(stringToFind + random.Next());

            TimeSpan classicTimings = ExecuteMethodAndReturnTimings(100000, classicContains);
            //TimeSpan alternativereplaceTimings = ExecuteMethodAndReturnTimings(100000, alternativereplace);

            Console.WriteLine("Classic timing: " + classicTimings.TotalMilliseconds);
            //Console.WriteLine("Alternative timing: " + alternativereplaceTimings.TotalMilliseconds);
        }

        [Test][Ignore("Enable for testing custom algorithm")]
        public void StripAlternativeV2Tests()
        {
            Random random = new Random();

            string sourceString = "Beast-of-Burden's power and toughness  are each equal to the number of creatures on the battlefield.";

            //warmup
            Common.StringExtensions.StripAlternative(sourceString + random.Next());
            //StringExtensions.StripAlternativeV2(sourceString + random.Next());

            //classic replace method with tolowercase
            Func<string> altStrip = () => Common.StringExtensions.StripAlternative(sourceString + random.Next());

            //Func<string> altv2String = () => StringExtensions.StripAlternativeV2(sourceString + random.Next());

            var numberOfTimesToExecute = 1000000;
            TimeSpan altv1Timing = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, altStrip);
            //TimeSpan altv2Timing = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, altv2String);

            Console.WriteLine("Alt timing:  " + altv1Timing.TotalMilliseconds);
            //Console.WriteLine("Altv2 timing: " + altv2Timing.TotalMilliseconds);
        }

        [Test]
        public void StringAnyAlternativeTest()
        {
            string sourceString = "Beast of Burden's power and toughness are each equal to the number of creatures on the battlefield";//.ToLowerInvariant();
            string stringToFind = "to the number of creatures";

            Func<bool> classicContains = () => sourceString.ToLowerInvariant().Contains(stringToFind.ToLowerInvariant());

            Func<bool> alternativeContains = () => sourceString.AnyString(stringToFind);


            TimeSpan classicTimings = ExecuteMethodAndReturnTimings(100000, classicContains);
            TimeSpan alternativereplaceTimings = ExecuteMethodAndReturnTimings(100000, alternativeContains);

            Console.WriteLine("Classic timing: " + classicTimings.TotalMilliseconds);
            Console.WriteLine("Alternative timing: " + alternativereplaceTimings.TotalMilliseconds);
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

            ParallelOptions options = new ParallelOptions {MaxDegreeOfParallelism = 4};
            Parallel.For(0, numberOfIterationsToPerform,options, j =>
            {

                string word = sameRandomWordsList[j];
               // Debug.WriteLine(word);
                functionToExecute.Invoke(word);
            });

            timer.Stop();
            return timer;
        }

        [Test]
        public void StringBuilderForCacheKeyTests()
        {
            Random random = new Random();

            string input = "power and toughness  are each equal to the number of creatures on the battlefield.";
            string comparedTo = "Whenever a creature dealt damage by this turn dies, you gain life equal to that creature's toughness.";
            int sbLen = input.Length + comparedTo.Length + 50;


            Func<string> concat = () => string.Concat(input+random.Next(), comparedTo+random.Next(), random.Next() % 2 == 0 ? "1" : "0");

            Func<string> join = () => string.Join("_",input + random.Next(), comparedTo + random.Next(), random.Next() % 2 == 0 ? "1" : "0");

            Func<string> stringbuilder = () =>
            {
                StringBuilder sb = new StringBuilder(sbLen);
                sb.Append(input+random.Next());
                sb.Append(comparedTo+random.Next());
                sb.Append(random.Next()%2 == 0 ? "1" : "0");
                return sb.ToString();
            };

            var numberOfTimesToExecute = 100000;
            TimeSpan concatTiming = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, concat);
            TimeSpan stringBuilderTiming = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, stringbuilder);
            TimeSpan joinTiming = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, join);

            Console.WriteLine("concat timing       : " + concatTiming.TotalMilliseconds);
            Console.WriteLine("stringbuilder timing: " + stringBuilderTiming.TotalMilliseconds);
            Console.WriteLine("join          timing: " + joinTiming.TotalMilliseconds);
        }

        [Test]
        public void StringAlternativeToUpperInsteadOfToLower()
        {
            Random random = new Random(13);
            string one = "The Doctor";
            // ReSharper disable AccessToModifiedClosure
            Func<string> toLower = () => (one + random.Next()).ToLower();
            Func<string> toUpperInvariant = () =>  (one + random.Next()).ToUpperInvariant();
            // ReSharper restore AccessToModifiedClosure

            var toLowerTiming = ExecuteMethodAndReturnTimings(100000, toLower);
            random = new Random(13);
            var toUpperInvariantTiming = ExecuteMethodAndReturnTimings(100000, toUpperInvariant);

            Console.WriteLine("toLowerTiming timing: " + toLowerTiming.TotalMilliseconds);
            Console.WriteLine("toUpperInvariantTiming timing: " + toUpperInvariantTiming.TotalMilliseconds);
        }

        public TimeSpan ExecuteMethodAndReturnTimings<TResult>(int numberOfTimesToExecute, Func<TResult> funcToExecute)
        {
            Stopwatch timer = new Stopwatch();
            //timer.Start();

            for (int i = 0; i < numberOfTimesToExecute; i++)
            {   timer.Start();
                var result = funcToExecute.Invoke();
                timer.Stop();
            }

            return timer.Elapsed;
        }

    }
}