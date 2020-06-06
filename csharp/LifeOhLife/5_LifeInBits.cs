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

                // life cell: 0000-0001
                // neighours:
                // 0: 0000-0000
                // 1: 0000-0001
                // 2: 0000-0010 ***
                // 3: 0000-0011 ***
                // 4: 0000-0100
                // 5: 0000-0101
                // 6: 0000-0110
                // 7: 0000-0111
                // 8: 0000-1000

                for (int i = WIDTH; i < WIDTH * HEIGHT - WIDTH; i += 8)
                {
                    ulong neighbours = *(ulong*)(tempPtr + i);
                    ulong alive = *(ulong*)(fieldPtr + i);
                    // 8 neighbours => 0 neighbours. After this, only last 3 bits are important
                    neighbours &= 0b00000111_00000111_00000111_00000111_00000111_00000111_00000111_00000111ul;

                    //neighbours = 000 ... 111;

                    ulong aliveAndNeighbours = ((neighbours & ~0b00000001_00000001_00000001_00000001_00000001_00000001_00000001_00000001ul) >> 1) | (alive << 2);
                    // should only be 0000-0101
                    aliveAndNeighbours ^= ~0b00000101_00000101_00000101_00000101_00000101_00000101_00000101_00000101ul;
                    aliveAndNeighbours &= (aliveAndNeighbours >> 2);
                    aliveAndNeighbours &= (aliveAndNeighbours >> 1);
                    aliveAndNeighbours &= 0b00000001_00000001_00000001_00000001_00000001_00000001_00000001_00000001ul;

                    ulong makeNewLife = neighbours | (alive << 3);
                    // should only be 0000-0011
                    makeNewLife ^= ~0b00000011_00000011_00000011_00000011_00000011_00000011_00000011_00000011ul;
                    makeNewLife &= (makeNewLife >> 2);
                    makeNewLife &= (makeNewLife >> 1);
                    makeNewLife &= 0b00000001_00000001_00000001_00000001_00000001_00000001_00000001_00000001ul;

                    *(ulong*)(fieldPtr + i) = aliveAndNeighbours | makeNewLife;
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
