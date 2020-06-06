using System;
using System.Collections.Generic;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// This algorithm is similar to LifeInBits
    /// but it reduces storage twice storing two field cells in each byte, four bits per cell
    /// at cost of more complex bit manipulation
    /// This algorithm is the fastest among those which don't use SIMD processor instructions
    /// </summary>

    public unsafe class LifeIsABitMagic : LifeJourney
    {
        byte[] field = new byte[WIDTH + WIDTH * HEIGHT / 2];
        byte[] temp = new byte[WIDTH + WIDTH * HEIGHT / 2];

        public override bool Get(int i, int j)
        {
            int pos = WIDTH / 2 + j * (WIDTH / 2) + (i / 2);
            if (i % 2 == 1) return (field[pos] & 0x10) == 0x10;
            else return (field[pos] & 1) == 1;
        }

        public override void Set(int i, int j, bool val)
        {
            int pos = WIDTH / 2 + j * (WIDTH / 2) + (i / 2);
            if (i % 2 == 1)
            {
                if (val) field[pos] |= 0x10;
                else field[pos] &= (0xFF & ~0x10);
            }
            else
            {
                if (val) field[pos] |= 0x1;
                else field[pos] &= (0xFF & ~0x1);
            }
        }


        public override void Step()
        {
            fixed (byte* fieldPtr = field, tempPtr = temp)
            {
                for (int i = 0; i < temp.Length; i += 8)
                {
                    *(ulong*)(tempPtr + i) = 0;
                }

                for (int i = WIDTH; i < WIDTH * HEIGHT / 2; i += 8)
                {
                    ulong* ptr = (ulong*)(tempPtr + i);

                    ulong src1 = *(ulong*)(fieldPtr + i - WIDTH / 2);
                    ulong src2 = *(ulong*)(fieldPtr + i);
                    ulong src3 = *(ulong*)(fieldPtr + i + WIDTH / 2);

                    ulong src4 = *(ulong*)(fieldPtr + i - WIDTH / 2 - 8);
                    ulong src5 = *(ulong*)(fieldPtr + i - 8);
                    ulong src6 = *(ulong*)(fieldPtr + i + WIDTH / 2 - 8);

                    ulong src7 = *(ulong*)(fieldPtr + i - WIDTH / 2 + 8);
                    ulong src8 = *(ulong*)(fieldPtr + i + 8);
                    ulong src9 = *(ulong*)(fieldPtr + i + WIDTH / 2 + 8);

                    *ptr += (src1 << 4) + src1 + (src1 >> 4);
                    *ptr += (src2 << 4) + (src2 >> 4);
                    *ptr += (src3 << 4) + src3 + (src3 >> 4);

                    *ptr += (src4 >> 60) + (src5 >> 60) + (src6 >> 60);
                    *ptr += (src7 << 60) + (src8 << 60) + (src9 << 60);
                }

                // life cell: 0001
                // neighours:
                // 0: 0000
                // 1: 0001
                // 2: 0010 ***
                // 3: 0011 ***
                // 4: 0100
                // 5: 0101
                // 6: 0110
                // 7: 0111
                // 8: 1000

                for (int i = WIDTH; i < WIDTH * HEIGHT / 2; i += 8)
                {
                    ulong neighbours = *(ulong*)(tempPtr + i);
                    ulong alive = *(ulong*)(fieldPtr + i);
                    neighbours &= 0b0111_0111_0111_0111_0111_0111_0111_0111_0111_0111_0111_0111_0111_0111_0111_0111ul;

                    ulong keepAlive = ((neighbours & ~0b0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001ul) >> 1) | (alive << 2);
                    keepAlive ^= ~0b0101_0101_0101_0101_0101_0101_0101_0101_0101_0101_0101_0101_0101_0101_0101_0101ul;
                    keepAlive &= (keepAlive >> 2);
                    keepAlive &= (keepAlive >> 1);
                    keepAlive &= 0b0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001ul;

                    ulong makeNewLife = neighbours | (alive << 3);
                    makeNewLife ^= ~0b0011_0011_0011_0011_0011_0011_0011_0011_0011_0011_0011_0011_0011_0011_0011_0011ul;
                    makeNewLife &= (makeNewLife >> 2);
                    makeNewLife &= (makeNewLife >> 1);
                    makeNewLife &= 0b0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001_0001ul;

                    *(ulong*)(fieldPtr + i) = keepAlive | makeNewLife;
                }

            }

            for (int j = 0; j < HEIGHT; j++)
            {
                Set(0, j, false);
                Set(WIDTH - 1, j, false);
            }

        }
    }
}
