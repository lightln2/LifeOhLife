using System;
using System.Collections.Generic;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// Improves determining if cells are gonna live or die, like in LifeIsPredetermined,
    /// but uses bit manipulation instead of table lookups
    /// </summary>
    public unsafe class LifeInBits : LifeJourney
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

                for (int i = WIDTH; i < WIDTH * HEIGHT - WIDTH; i += 8)
                {
                    ulong neighbours = *(ulong*)(tempPtr + i);
                    ulong alive = *(ulong*)(fieldPtr + i);
                    // 8 neighbours behaves same as 0 neighbours, so, fourth bit can be discarded!
                    neighbours &= 0b00000111_00000111_00000111_00000111_00000111_00000111_00000111_00000111ul;
                    // mask combines count of neighbours in last three bits and 'alive' flag in fourth bit
                    // valid combinations for the cell to live are 0010, 0011, 1011; others cause cell to be dead in next gen.
                    ulong mask = neighbours | (alive << 3);

                    ulong keepAlive = mask & 0b00001110_00001110_00001110_00001110_00001110_00001110_00001110_00001110ul;
                    keepAlive ^= 0b00000101_00000101_00000101_00000101_00000101_00000101_00000101_00000101ul;
                    keepAlive &= (keepAlive >> 2);
                    keepAlive &= (keepAlive >> 1);
                    keepAlive &= 0b00000001_00000001_00000001_00000001_00000001_00000001_00000001_00000001ul;
                    ulong makeNewLife = mask ^ 0b00001100_00001100_00001100_00001100_00001100_00001100_00001100_00001100ul;
                    makeNewLife &= (makeNewLife >> 2);
                    makeNewLife &= (makeNewLife >> 1);
                    makeNewLife &= 0b00000001_00000001_00000001_00000001_00000001_00000001_00000001_00000001ul;
                    * (ulong*)(fieldPtr + i) = keepAlive | makeNewLife;
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
