using System;
using System.Diagnostics;

namespace LifeOhLife
{
    class Program
    {
        const int STEPS = 1000;
        static void Main(string[] args)
        {
            // classical algorithms
            RunPerformanceTests<SimpleLife>();
            RunPerformanceTests<LifeBytes>();
            RunPerformanceTests<LongLife>();
            RunPerformanceTests<LifeIsLookingUp>();
            RunPerformanceTests<LifeInBits>();
            RunPerformanceTests<LifeIsABitMagic>();
            RunPerformanceTests<AdvancedLifeExtensions>();
            // algorighms dependent on field state
            RunPerformanceTests<LifeInList>();
            RunPerformanceTests<LifeIsChange>();
        }

        static void RunPerformanceTests<T>() where T: LifeJourney, new()
        {
            LifeJourney life = new T();
            Console.WriteLine($"{life.Name}:");
            life.GenerateRandomField(12345, 0.02);
            int initialLiveCells = life.GetLiveCellsCount();
            int totalCells = LifeJourney.WIDTH * LifeJourney.HEIGHT;
            int initialHash = life.GetFingerprint();
            Stopwatch timer = new Stopwatch();
            timer.Start();
            life.Run(STEPS);
            int currentLiveCells = life.GetLiveCellsCount();
            int currentHash = life.GetFingerprint();
            double elapsedSeconds = timer.Elapsed.TotalSeconds;
            double stepsPerSecond = STEPS / elapsedSeconds;
            string infoPerformance = $"{STEPS} steps in {elapsedSeconds:0.000} seconds, {stepsPerSecond:0.000} steps/second";
            string infoStatistics = $"Live cells:  {initialLiveCells}/{totalCells} [hash={initialHash}] -> {currentLiveCells} [{currentHash}]";
            Console.WriteLine($"        {infoPerformance}; {infoStatistics}");
        }
    }
}
