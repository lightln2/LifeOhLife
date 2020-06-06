using System;
using System.Collections.Generic;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// Improves determining if cells are gonna live or die,
    /// avoiding costly comparisons and using pre-calculated table lookups instead.
    /// </summary>
    public unsafe class LifeIsLookingUp : LifeJourney
    {
        static byte[] alivePerNeighbours = new byte[256];
        byte[] field = new byte[WIDTH * HEIGHT];
        byte[] temp = new byte[WIDTH * HEIGHT];

        public LifeIsLookingUp()
        {
            alivePerNeighbours[3] = 1;
            alivePerNeighbours[8 + 2] = 1;
            alivePerNeighbours[8 + 3] = 1;
        }

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

                for (int i = WIDTH; i < WIDTH * HEIGHT - WIDTH; i++)
                {
                    byte neighbours = (byte)((tempPtr[i] & 7) | (fieldPtr[i] << 3));
                    fieldPtr[i] = alivePerNeighbours[neighbours];
                }

            }

            for (int j = 1; j < HEIGHT - 1; j++)
            {
                field[j * WIDTH] = 0;
                field[j * WIDTH + WIDTH - 1] = 0;
            }

        }
    }
}
