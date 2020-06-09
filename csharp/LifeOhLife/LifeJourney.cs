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
            for (int i = 1; i < WIDTH - 1; i++)
            {
                for (int j = 1; j < HEIGHT - 1; j++)
                {
                    Set(i, j, false);
                }
            }
        }

        public void GenerateRandomField(int seedForRandom, double threshold)
        {
            Random rand = new Random(seedForRandom);
            for (int i = 1; i < WIDTH - 1; i++)
            {
                for (int j = 1; j < HEIGHT - 1; j++)
                {
                    bool isLiveCell = rand.NextDouble() < threshold;
                    Set(i, j, isLiveCell);
                }
            }
        }

        public int GetLiveCellsCount()
        {
            int count = 0;
            for (int i = 1; i < WIDTH - 1; i++)
            {
                for (int j = 1; j < HEIGHT - 1; j++)
                {
                    if (Get(i, j)) count++;
                }
            }
            return count;
        }

        // useful to ensure different algorithms yield the same result

        public int GetFingerprint()
        {
            int hash = 0;
            for (int i = 1; i < WIDTH - 1; i++)
            {
                for (int j = 1; j < HEIGHT - 1; j++)
                {
                    if (Get(i, j))
                    {
                        hash = hash * 31 + i;
                        hash = hash * 31 + j;
                    }
                }
            }
            return hash;
        }

        public void SetRectangle(int startX, int startY, string pattern)
        {
            string[] lines = pattern.Trim().Split('\n').Select(p => p.Trim()).ToArray();
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    Set(startX + j, startY + i, lines[i][j] == 'x');
                }
            }
        }

        public string GetRectangle(int startX, int startY, int lenX, int lenY)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lenY; i++)
            {
                for (int j = 0; j < lenX; j++)
                {
                    bool isAlive = Get(startX + j, startY + i);
                    sb.Append(isAlive ? 'x' : '-');
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void TestRectangle(int startX, int startY, string pattern)
        {
            string[] lines = pattern.Split('\n').Select(l => l.Trim()).ToArray();
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    bool isAlive = Get(startX + j, startY + i);
                    if (lines[i][j] != (isAlive ? 'x' : '-'))
                    {
                        throw new Exception($"{lines[i][j]} expected at ({startX + j}, {startY + i})");
                    }
                }
            }
        }

    }
}
