using System;

namespace LifeOhLife
{
    class Program
    {
        // Every output should look like:
        // field with 1034216 live out of 2073600 total cells initialized, hash=1583560213
        // [life algorithm]: 88047 live cells remaining after 1000 steps in *** seconds, hash=-1752572758
        static void Main(string[] args)
        {
            int steps = 1000;
            new SimpleLife().RunPerformanceTest(steps);
            new LifeBytes().RunPerformanceTest(steps);
            new LongLife().RunPerformanceTest(steps);
            new LifeIsLookingUp().RunPerformanceTest(steps);
            new LifeInBits().RunPerformanceTest(steps);
            new LifeIsABitMagic().RunPerformanceTest(steps);
            new AdvancedLifeExtensions().RunPerformanceTest(steps);
        }
    }
}
