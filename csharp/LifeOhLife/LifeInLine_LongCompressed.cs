using System;
using System.Collections.Generic;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// Uses upper-, center- and lower lines instead of temporary buffer; stores two cells per byte.
    /// Handles 8 bytes at once
    /// </summary>
    public unsafe class LifeInLine_LongCompressed : LifeJourney
    {
        const int LINE_WIDTH = WIDTH / 2;

        byte[] field = new byte[LINE_WIDTH * HEIGHT + 8];

        byte[] upperLineSumOf2 = new byte[LINE_WIDTH];
        byte[] upperLineSumOf3 = new byte[LINE_WIDTH];
        byte[] middleLineSumOf2 = new byte[LINE_WIDTH];
        byte[] middleLineSumOf3 = new byte[LINE_WIDTH];
        byte[] lowerLineSumOf2 = new byte[LINE_WIDTH];
        byte[] lowerLineSumOf3 = new byte[LINE_WIDTH];

        public override bool Get(int x, int y)
        {
            int pos = y * LINE_WIDTH + x / 2;
            int offset = (x % 2) * 4;
            ulong mask = 1ul << offset;
            return (field[pos] & mask) == mask;
        }

        public override void Set(int x, int y, bool value)
        {
            int pos = y * LINE_WIDTH + x / 2;
            int offset = (x % 2) * 4;
            byte mask = (byte)(1 << offset);
            if (value) field[pos] |= mask;
            else field[pos] &= (byte)~mask;
        }

        public override void Step()
        {
            fixed (byte* 
                         currentFieldPtr = field,
                         upperLineSumOf2Ptr = upperLineSumOf2,
                         upperLineSumOf3Ptr = upperLineSumOf3,
                         middleLineSumOf2Ptr = middleLineSumOf2,
                         middleLineSumOf3Ptr = middleLineSumOf3,
                         lowerLineSumOf2Ptr = lowerLineSumOf2,
                         lowerLineSumOf3Ptr = lowerLineSumOf3)
            {
                byte* upper2 = upperLineSumOf2Ptr, upper3 = upperLineSumOf3Ptr;
                byte* middle2 = middleLineSumOf2Ptr, middle3 = middleLineSumOf3Ptr;
                byte* lower2 = lowerLineSumOf2Ptr, lower3 = lowerLineSumOf3Ptr;
                byte* nextLinePtr = currentFieldPtr + LINE_WIDTH;
                
                for (int x = 0; x < LINE_WIDTH; x += 8)
                {
                    *(ulong*)(upper2 + x) = *(ulong*)(upper3 + x) = 0;
                    *(ulong*)(middle2 + x) = *(ulong*)(middle3 + x) = 0;
                    ulong nextLeft = *(ulong*)(nextLinePtr + x - 8);
                    ulong nextCenter = *(ulong*)(nextLinePtr + x);
                    ulong nextRight = *(ulong*)(nextLinePtr + x + 8);
                    *(ulong*)(lower2 + x) = (nextLeft >> 60) + (nextCenter >> 4) + (nextCenter << 4) + (nextRight << 60);
                    *(ulong*)(lower3 + x) = *(ulong*)(lower2 + x) + nextCenter;
                }

                for (int y = 1; y < HEIGHT - 1; y++)
                {
                    nextLinePtr += LINE_WIDTH;

                    byte* temp2 = upper2;
                    byte* temp3 = upper3;
                    upper2 = middle2;
                    upper3 = middle3;
                    middle2 = lower2;
                    middle3 = lower3;
                    lower2 = temp2;
                    lower3 = temp3;

                    for (int x = 0; x < LINE_WIDTH; x += 8)
                    {
                        ulong nextLeft = *(ulong*)(nextLinePtr + x - 8);
                        ulong nextCenter = *(ulong*)(nextLinePtr + x);
                        ulong nextRight = *(ulong*)(nextLinePtr + x + 8);
                        *(ulong*)(lower2 + x) = (nextLeft >> 60) + (nextCenter >> 4) + (nextCenter << 4) + (nextRight << 60);
                        *(ulong*)(lower3 + x) = *(ulong*)(lower2 + x) + nextCenter;

                        ulong neighbours = *(ulong*)(upper3 + x) + *(ulong*)(middle2 + x) + *(ulong*)(lower3 + x);
                        ulong alive = *(ulong*)(middle3 + x) - *(ulong*)(middle2 + x);

                        ulong mask = neighbours | (alive << 3);
                        ulong keepAlive = (mask & 0xEEEEEEEEEEEEEEEEul) ^ 0x5555555555555555ul;
                        keepAlive &= (keepAlive >> 2);
                        keepAlive &= (keepAlive >> 1);
                        ulong makeNewLife = mask ^ 0xCCCCCCCCCCCCCCCCul;
                        makeNewLife &= (makeNewLife >> 2);
                        makeNewLife &= (makeNewLife >> 1);

                        *(ulong*)(nextLinePtr - LINE_WIDTH + x) = (keepAlive | makeNewLife) & 0x1111111111111111ul;
                    }

                    *(byte*)(nextLinePtr - LINE_WIDTH) &= 0xF0;
                    *(byte*)(nextLinePtr - 1) &= 0x0F;

                }

            }

        }
    }
}
