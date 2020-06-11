using System;
using System.Diagnostics;

namespace LifeOhLife
{
    class Program
    {
        static void Main(string[] args)
        {
            int steps = 1000;
            // classical algorithms        
            RunPerformanceTests<SimpleLife>(steps);
            RunPerformanceTests<LifeBytes>(steps);
            RunPerformanceTests<LongLife>(steps);
            RunPerformanceTests<LifeIsLookingUp>(steps);
            RunPerformanceTests<LifeInBits>(steps);
            RunPerformanceTests<LifeIsABitMagic>(steps);
            RunPerformanceTests<AdvancedLifeExtensions>(steps);
            RunPerformanceTests<AdvancedLifeExtensionsInLine>(steps);
            RunPerformanceTests<AdvancedLifeExtensionsInLineCompressed>(steps);
            // algorithms using upper/center/lower lines instead of a temporary buffer
            RunPerformanceTests<LifeInLine_Bytes>(steps);
            RunPerformanceTests<LifeInLine_Long>(steps);
            RunPerformanceTests<LifeInLine_LongCompressed>(steps);
            // algorighms dependent on field state
            RunPerformanceTests<LifeInList>(steps);
            RunPerformanceTests<LifeIsChange>(steps);
        }

        static void RunPerformanceTests<T>(int steps) where T: LifeJourney, new()
        {
            LifeJourney life = new T();
            Console.WriteLine($"{life.Name}:");
            life.GenerateRandomField(12345, 0.5);
            int initialLiveCells = life.GetLiveCellsCount();
            int totalCells = LifeJourney.WIDTH * LifeJourney.HEIGHT;
            int initialHash = life.GetFingerprint();
            Stopwatch timer = new Stopwatch();
            timer.Start();
            life.Run(steps);
            int currentLiveCells = life.GetLiveCellsCount();
            int currentHash = life.GetFingerprint();
            double elapsedSeconds = timer.Elapsed.TotalSeconds;
            double stepsPerSecond = steps / elapsedSeconds;
            string infoPerformance = $"{steps} steps in {elapsedSeconds:0.000} seconds, {stepsPerSecond:0.000} steps/second";
            string infoStatistics = $"Live cells:  {initialLiveCells}/{totalCells} [hash={initialHash}] -> {currentLiveCells} [{currentHash}]";
            Console.WriteLine($"        {infoPerformance}; {infoStatistics}");
        }
    }
}
