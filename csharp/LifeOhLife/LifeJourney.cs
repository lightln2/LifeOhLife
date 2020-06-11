using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// Base for any game of life simulation algorithm.
    /// Every algorithm basically does two things per similation step:
    ///     1. For each field cell, count number of neighbours of that cell and store it to the temp array
    ///     2. For each field cell, make it live or dead, based on its current state,
    ///        number of its neghbours and on the rules of the game, 
    /// </summary>
    public abstract class LifeJourney
    {
        // life in Full HD!
        public const int WIDTH = 1920;
        public const int HEIGHT = 1080;

        public string Name => GetType().Name;

        public abstract void Set(int i, int j, bool value);

        public abstract bool Get(int i, int j);

        public abstract void Step();

        public void Run(int steps)
        {
            for (int i = 0; i < steps; i++) Step();
        }

        public virtual void Clear()
        {
            for (int x = 1; x < WIDTH - 1; x++)
            {
                for (int y = 1; y < HEIGHT - 1; y++)
                {
                    Set(x, y, false);
                }
            }
        }

        public void GenerateRandomField(int seedForRandom, double threshold)
        {
            Random rand = new Random(seedForRandom);
            for (int x = 1; x < WIDTH - 1; x++)
            {
                for (int y = 1; y < HEIGHT - 1; y++)
                {
                    bool isLiveCell = rand.NextDouble() < threshold;
                    Set(x, y, isLiveCell);
                }
            }
        }

        public int GetLiveCellsCount()
        {
            int count = 0;
            for (int x = 1; x < WIDTH - 1; x++)
            {
                for (int y = 1; y < HEIGHT - 1; y++)
                {
                    if (Get(x, y)) count++;
                }
            }
            return count;
        }

        // useful to ensure different algorithms yield the same result

        public int GetFingerprint()
        {
            int hash = 0;
            for (int x = 1; x < WIDTH - 1; x++)
            {
                for (int y = 1; y < HEIGHT - 1; y++)
                {
                    if (Get(x, y))
                    {
                        hash = hash * 31 + x;
                        hash = hash * 31 + y;
                    }
                }
            }
            return hash;
        }

        public void SetRectangle(int startX, int startY, string pattern)
        {
            string[] lines = pattern.Trim().Split('\n').Select(p => p.Trim()).ToArray();
            for (int x = 0; x < lines.Length; x++)
            {
                for (int y = 0; y < lines[x].Length; y++)
                {
                    Set(startX + y, startY + x, lines[x][y] == 'x');
                }
            }
        }

        public string GetRectangle(int startX, int startY, int lenX, int lenY)
        {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < lenY; x++)
            {
                for (int y = 0; y < lenX; y++)
                {
                    bool isAlive = Get(startX + y, startY + x);
                    sb.Append(isAlive ? 'x' : '-');
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void TestRectangle(int startX, int startY, string pattern)
        {
            string[] lines = pattern.Split('\n').Select(l => l.Trim()).ToArray();
            for (int x = 0; x < lines.Length; x++)
            {
                for (int y = 0; y < lines[x].Length; y++)
                {
                    bool isAlive = Get(startX + y, startY + x);
                    if (lines[x][y] != (isAlive ? 'x' : '-'))
                    {
                        throw new Exception($"{lines[x][y]} expected at ({startX + y}, {startY + x})");
                    }
                }
            }
        }

    }
}
