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

        [Test]
        public void InternalVariablesMaxCacheSizeInMbTests()
        {
            Console.WriteLine("MaxCacheSizeInMb: " + InternalVariables.MaxCacheSizeInMb);
            Assert.That(InternalVariables.MaxCacheSizeInMb, Is.GreaterThanOrEqualTo(InternalVariables.DefaultCacheItemSizeInMb * 2));
        }

        [Test]
        public void InternalVariablesDefaultCacheSizeInMbTests()
        {
            Console.WriteLine("DefaultCacheItemSizeInMb: " + InternalVariables.DefaultCacheItemSizeInMb);
            Assert.That(InternalVariables.MaxCacheSizeInMb, Is.GreaterThanOrEqualTo(1));
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
            Console.WriteLine("Uncached Result:   \t Avg ticks: " + uncachedTimings.Item1);
            Console.WriteLine("Cached Result:     \t Avg ticks: " + cachedTimings.Item1);
            Console.WriteLine("Uncached Result:   \t Avg ms: " + uncachedTimings.Item2);
            Console.WriteLine("Cached Result:     \t Avg ms: " + cachedTimings.Item2);
            

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
        public void StringContainsAlternativeTest()
        {
            Random random = new Random();

            string sourceString = "Beast of Burden's power and toughness are each equal to the number of creatures on the battlefield".ToLowerInvariant();
            string stringToFind = "to the number of creatures";
            
            //classic replace method with tolowercase
            Func<bool> classicContains = () => sourceString.Contains(stringToFind+random.Next());

            Func<bool> alternativereplace = () => sourceString.ContainsString(stringToFind + random.Next());

            TimeSpan classicTimings = ExecuteMethodAndReturnTimings(100000, classicContains);
            TimeSpan alternativereplaceTimings = ExecuteMethodAndReturnTimings(100000, alternativereplace);

            Console.WriteLine("Classic timing: " + classicTimings.TotalMilliseconds);
            Console.WriteLine("Alternative timing: " + alternativereplaceTimings.TotalMilliseconds);
        }

        [Test]
        public void StripAlternativeTests()
        {
            Random random = new Random();

            string sourceString = "Beast-of-Burden's power and toughness  are each equal to the number of creatures on the battlefield.";

            //classic replace method with tolowercase
            Func<string> regexStrip = () => StringExtensions.Strip(sourceString + random.Next());

            Func<string> customStrip = () => StringExtensions.StripAlternative(sourceString + random.Next());

            var numberOfTimesToExecute = 10000;
            TimeSpan regexStripTiming = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, regexStrip);
            TimeSpan alternativeStripTiming = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, customStrip);

            Console.WriteLine("Regex timing:  " + regexStripTiming.TotalMilliseconds);
            Console.WriteLine("Custom timing: " + alternativeStripTiming.TotalMilliseconds);
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
        public void StringAnyAlternativeTest()
        {
            string sourceString = "Beast of Burden's power and toughness are each equal to the number of creatures on the battlefield";//.ToLowerInvariant();
            string stringToFind = "to the number of creatures";

            //classic replace method with tolowercase
            Func<bool> classicContains = () => sourceString.ToLowerInvariant().Contains(stringToFind.ToLowerInvariant());

            Func<bool> alternativeContains = () => sourceString.AnyString(stringToFind);


            TimeSpan classicTimings = ExecuteMethodAndReturnTimings(100000, classicContains);
            TimeSpan alternativereplaceTimings = ExecuteMethodAndReturnTimings(100000, alternativeContains);

            Console.WriteLine("Classic timing: " + classicTimings.TotalMilliseconds);
            Console.WriteLine("Alternative timing: " + alternativereplaceTimings.TotalMilliseconds);
        }

        [Test]
        public void LevenshteinDistanceAlternativeTest()
        {
            Random random = new Random(Environment.TickCount);

            int next = random.Next();
            string sourceString = "Beast of Burden's power and toughness are each equal to the number of creatures" + next;//.ToLowerInvariant();
            string stringToFind = "to the number of creatures" + next;

            //classic replace method with tolowercase
            Func<int> original = () => (sourceString + next).LevenshteinDistanceUncached(stringToFind + next);

            Func<int> alternative = () => (sourceString + next).LevenshteinDistanceUncachedAlternative(stringToFind + next);

            var numberOfTimesToExecute = 1000;

            //warmup
            TimeSpan alternativeTimings = ExecuteMethodAndReturnTimings(500, alternative);
            TimeSpan originalTimings = ExecuteMethodAndReturnTimings(500, original);

            //execute
            alternativeTimings = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, alternative);
            originalTimings = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, original);


            Console.WriteLine("Original timing: " + originalTimings.TotalMilliseconds);
            Console.WriteLine("Alternative timing: " + alternativeTimings.TotalMilliseconds);
        }

        [Test]
        public void DiceCoefficientAlternativeTest()
        {
            Random random = new Random(37);

            int next = random.Next();
            string sourceString = "Beast of Burden's power and toughness are each equal to the number of creatures" + next;
            string stringToFind = "Predominantly sedentary, the species and creatures can be locally nomadic." + next;

            //classic replace method with tolowercase
            Func<double> original = () => (sourceString + random.Next()).DiceCoefficientUncached(stringToFind + random.Next());

            Func<double> alternative = () => (sourceString + random.Next()).DiceCoefficientAlternative(stringToFind + random.Next());

            var numberOfTimesToExecute = 1000;

            //warmup
            TimeSpan alternativeTimings = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, alternative);
            TimeSpan originalTimings = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, original);

            //execute
            alternativeTimings = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, alternative);
            originalTimings = ExecuteMethodAndReturnTimings(numberOfTimesToExecute, original);


            Console.WriteLine("Original timing: " + originalTimings.TotalMilliseconds);
            Console.WriteLine("Alternative timing: " + alternativeTimings.TotalMilliseconds);
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