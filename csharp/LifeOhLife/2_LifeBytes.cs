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
        byte[] currentField = new byte[WIDTH * HEIGHT];
        byte[] nextField = new byte[WIDTH * HEIGHT];

        public override bool Get(int x, int y) => currentField[y * WIDTH + x] == 1;

        public override void Set(int x, int y, bool value) => currentField[y * WIDTH + x] = (byte)(value ? 1 : 0);

        public override void Step()
        {
            for (int x = 1; x < WIDTH - 1; x++)
            {
                for (int y = 1; y < HEIGHT - 1; y++)
                {
                    int pos = y * WIDTH + x;
                    nextField[pos] = (byte)(
                        currentField[pos - WIDTH - 1] + currentField[pos - WIDTH] + currentField[pos - WIDTH + 1] +
                        currentField[pos - 1] + currentField[pos + 1] +
                        currentField[pos + WIDTH - 1] + currentField[pos + WIDTH] + currentField[pos + WIDTH + 1]);
                    bool keepAlive = currentField[pos] == 1 && (nextField[pos] == 2 || nextField[pos] == 3);
                    bool makeNewLife = currentField[pos] == 0 && nextField[pos] == 3;
                    nextField[pos] = (byte)(makeNewLife | keepAlive ? 1 : 0);
                }
            }

            byte[] tmp = currentField;
            currentField = nextField;
            nextField = tmp;
        }
    }

}
