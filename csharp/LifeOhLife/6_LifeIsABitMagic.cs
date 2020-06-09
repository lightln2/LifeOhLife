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
        const int LINE_WIDTH = WIDTH / 2;

        // additional line before and after the field
        byte[] currentField = new byte[LINE_WIDTH * (HEIGHT + 2)];
        byte[] nextField = new byte[LINE_WIDTH * (HEIGHT + 2)];

        public override bool Get(int x, int y)
        {
            int pos = LINE_WIDTH + y * LINE_WIDTH + x / 2;
            int offset = (x % 2) * 4;
            ulong mask = 1ul << offset;
            return (currentField[pos] & mask) == mask;
        }

        public override void Set(int x, int y, bool value)
        {
            int pos = LINE_WIDTH + y * LINE_WIDTH + x / 2;
            int offset = (x % 2) * 4;
            byte mask = (byte)(1 << offset);
            if (value) currentField[pos] |= mask;
            else currentField[pos] &= (byte)~mask;
        }


        public override void Step()
        {
            fixed(byte* currentFieldPtr = currentField, nextFieldPtr = nextField)
            {
                for (int i = 2 * LINE_WIDTH; i < currentField.Length - 2 * LINE_WIDTH; i += 8)
                {
                    ulong top = *(ulong*)(currentFieldPtr + i - LINE_WIDTH);
                    ulong center = *(ulong*)(currentFieldPtr + i);
                    ulong bottom = *(ulong*)(currentFieldPtr + i + LINE_WIDTH);

                    ulong leftTop = *(ulong*)(currentFieldPtr + i - LINE_WIDTH - 8);
                    ulong left = *(ulong*)(currentFieldPtr + i - 8);
                    ulong leftBottom = *(ulong*)(currentFieldPtr + i + LINE_WIDTH - 8);

                    ulong rightTop = *(ulong*)(currentFieldPtr + i - LINE_WIDTH + 8);
                    ulong right = *(ulong*)(currentFieldPtr + i + 8);
                    ulong rightBottom = *(ulong*)(currentFieldPtr + i + LINE_WIDTH + 8);

                    ulong neighbours =
                        (top >> 4) + top + (top << 4) +
                        (center >> 4) + (center << 4) +
                        (bottom >> 4) + bottom + (bottom << 4) +

                        (leftTop >> 60) + (left >> 60) + (leftBottom >> 60) +
                        (rightTop << 60) + (right << 60) + (rightBottom << 60);

                    ulong alive = center;
                    ulong mask = neighbours | (alive << 3);
                    ulong keepAlive = (mask & 0xEEEEEEEEEEEEEEEEul) ^ 0x5555555555555555ul;
                    keepAlive &= (keepAlive >> 2);
                    keepAlive &= (keepAlive >> 1);
                    ulong makeNewLife = mask ^ 0xCCCCCCCCCCCCCCCCul;
                    makeNewLife &= (makeNewLife >> 2);
                    makeNewLife &= (makeNewLife >> 1);

                    *(ulong*)(nextFieldPtr + i) = (keepAlive | makeNewLife) & 0x1111111111111111ul;
                }

            }

            byte[] cur = currentField;
            currentField = nextField;
            nextField = cur;

            for (int y = 0; y < HEIGHT; y++)
            {
                Set(0, y, false);
                Set(WIDTH - 1, y, false);
            }

        }
    }
}
