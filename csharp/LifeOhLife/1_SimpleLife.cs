using System;
using System.Collections.Generic;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// This is the simplest ever implementation.
    /// Although its performance is terrible,
    /// it is very straightforward and easy to understand
    /// </summary>
    public class SimpleLife : LifeJourney
    {
        bool[,] field = new bool[WIDTH, HEIGHT];
        bool[,] temp = new bool[WIDTH, HEIGHT];

        public override bool Get(int i, int j) => field[i, j];

        public override void Set(int i, int j, bool value) => field[i, j] = value;

        public override void Step()
        {
            for (int i = 1; i < WIDTH - 1; i++)
            {
                for (int j = 1; j < HEIGHT - 1; j++)
                {
                    bool isAlive = field[i, j];
                    int numNeigbours = 0;
                    if (field[i - 1, j - 1]) numNeigbours++;
                    if (field[i - 1, j]) numNeigbours++;
                    if (field[i - 1, j + 1]) numNeigbours++;
                    if (field[i, j - 1]) numNeigbours++;
                    if (field[i, j + 1]) numNeigbours++;
                    if (field[i + 1, j - 1]) numNeigbours++;
                    if (field[i + 1, j]) numNeigbours++;
                    if (field[i + 1, j + 1]) numNeigbours++;
                    bool keepAlive = isAlive && (numNeigbours == 2 || numNeigbours == 3);
                    bool makeNewLive = !isAlive && numNeigbours == 3;
                    temp[i, j] = keepAlive | makeNewLive;
                }
            }

            for (int i = 1; i < WIDTH - 1; i++)
            {
                for (int j = 1; j < HEIGHT - 1; j++)
                {
                    field[i, j] = temp[i, j];
                }
            }

        }
    }
}
