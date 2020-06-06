using System;
using System.Collections.Generic;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// Improves LifeBytes algorithm,
    /// counting neighbours in chunks of eight contigous bytes,
    /// treating them as one single 64 bit ulong value
    /// </summary>
    public unsafe class LongLife : LifeJourney
    {
        byte[] field = new byte[WIDTH * HEIGHT];
        byte[] temp = new byte[WIDTH * HEIGHT];

        public override bool Get(int i, int j) => field[j * WIDTH + i] == 1;

        public override void Set(int i, int j, bool value) => field[j * WIDTH + i] = (byte)(value ? 1 : 0);

        public override void Step()
        {
            fixed (byte* fieldPtr = field, tempPtr = temp)
            {
                for (int i = 0; i < WIDTH * HEIGHT; i += 8)
                {
                    *(ulong*)(tempPtr + i) = 0;
                }

                for (int i = WIDTH + 1; i < WIDTH * HEIGHT - WIDTH - 1; i += 8)
                {
                    ulong* ptr = (ulong*)(tempPtr + i);
                    *ptr += *(ulong*)(fieldPtr + i - WIDTH - 1);
                    *ptr += *(ulong*)(fieldPtr + i - WIDTH);
                    *ptr += *(ulong*)(fieldPtr + i - WIDTH + 1);
                    *ptr += *(ulong*)(fieldPtr + i - 1);
                    *ptr += *(ulong*)(fieldPtr + i + 1);
                    *ptr += *(ulong*)(fieldPtr + i + WIDTH - 1);
                    *ptr += *(ulong*)(fieldPtr + i + WIDTH);
                    *ptr += *(ulong*)(fieldPtr + i + WIDTH + 1);
                }
            }

            for (int j = 1; j < HEIGHT - 1; j++)
            {
                for (int i = 1; i < WIDTH - 1; i++)
                {
                    int pos = i + j * WIDTH;
                    bool keepAlive = field[pos] == 1 && (temp[pos] == 2 || temp[pos] == 3);
                    bool makeNewLife = field[pos] == 0 && temp[pos] == 3;
                    field[pos] = (byte)(makeNewLife | keepAlive ? 1 : 0);
                }
            }

        }
    }
}
