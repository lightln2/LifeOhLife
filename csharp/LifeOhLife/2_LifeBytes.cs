using System;
using System.Collections.Generic;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// Represents field as array of bytes, and live cell as value = '1' in its byte.
    /// This way, counting neighbours can be implemented just by summing up the corresponding bytes.
    /// </summary>
    public class LifeBytes : LifeJourney
    {
        byte[] field = new byte[WIDTH * HEIGHT];
        byte[] temp = new byte[WIDTH * HEIGHT];

        public override bool Get(int i, int j) => field[j * WIDTH + i] == 1;

        public override void Set(int i, int j, bool value) => field[j * WIDTH + i] = (byte)(value ? 1 : 0);

        public override void Step()
        {
            for (int i = 1; i < WIDTH - 1; i++)
            {
                for (int j = 1; j < HEIGHT - 1; j++)
                {
                    int pos = j * WIDTH + i;
                    temp[pos] = (byte)(
                        field[pos - WIDTH - 1] + field[pos - WIDTH] + field[pos - WIDTH + 1] +
                        field[pos - 1] + field[pos + 1] +
                        field[pos + WIDTH - 1] + field[pos + WIDTH] + field[pos + WIDTH + 1]);
                }
            }

            for (int i = 1; i < WIDTH; i++)
            {
                for (int j = 1; j < HEIGHT; j++)
                {
                    int pos = j * WIDTH + i;
                    bool keepAlive = field[pos] == 1 && (temp[pos] == 2 || temp[pos] == 3);
                    bool makeNewLife = field[pos] == 0 && temp[pos] == 3;
                    field[pos] = (byte)(makeNewLife | keepAlive ? 1 : 0);
                }
            }
        }
    }
}
