using System;
using System.Collections.Generic;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// Improves determining if cells are gonna live or die, like in LifeIsPredetermined,
    /// but uses bit manipulation instead of table lookups
    /// </summary>
    public unsafe class LifeInLine_Long : LifeJourney
    {
        byte[] currentField = new byte[WIDTH * HEIGHT + 1];

        byte[] upperLineSumOf2 = new byte[WIDTH];
        byte[] upperLineSumOf3 = new byte[WIDTH];
        byte[] middleLineSumOf2 = new byte[WIDTH];
        byte[] middleLineSumOf3 = new byte[WIDTH];
        byte[] lowerLineSumOf2 = new byte[WIDTH];
        byte[] lowerLineSumOf3 = new byte[WIDTH];

        public override bool Get(int i, int j) => currentField[j * WIDTH + i] == 1;

        public override void Set(int i, int j, bool value) => currentField[j * WIDTH + i] = (byte)(value ? 1 : 0);

        public override void Step()
        {
            fixed (byte* 
                         currentFieldPtr = currentField,
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
                byte* nextLinePtr = currentFieldPtr + WIDTH;
                
                for (int x = 0; x < WIDTH; x += 8)
                {
                    *(ulong*)(upper2 + x) = *(ulong*)(upper3 + x) = 0;
                    *(ulong*)(middle2 + x) = *(ulong*)(middle3 + x) = 0;
                    *(ulong*)(lower2 + x) = *(ulong*)(nextLinePtr + x - 1) + *(ulong*)(nextLinePtr + x + 1);
                    *(ulong*)(lower3 + x) = *(ulong*)(lower2 + x) + *(ulong*)(nextLinePtr + x);
                }

                for (int y = 1; y < HEIGHT - 1; y++)
                {
                    nextLinePtr += WIDTH;

                    byte* temp2 = upper2;
                    byte* temp3 = upper3;
                    upper2 = middle2;
                    upper3 = middle3;
                    middle2 = lower2;
                    middle3 = lower3;
                    lower2 = temp2;
                    lower3 = temp3;

                    for (int x = 0; x < WIDTH; x += 8)
                    {
                        ulong left = *(ulong*)(nextLinePtr + x - 1);
                        ulong center = *(ulong*)(nextLinePtr + x);
                        ulong right = *(ulong*)(nextLinePtr + x + 1);

                        *(ulong*)(lower2 + x) = left + right;
                        *(ulong*)(lower3 + x) = left + center + right;

                        ulong neighbours = *(ulong*)(upper3 + x) + *(ulong*)(middle2 + x) + *(ulong*)(lower3 + x);
                        ulong alive = *(ulong*)(middle3 + x) - *(ulong*)(middle2 + x);

                        ulong mask = neighbours | (alive << 3);
                        ulong keepAlive = (mask & 0xEEEEEEEEEEEEEEEEul) ^ 0x5555555555555555ul;
                        keepAlive &= (keepAlive >> 2);
                        keepAlive &= (keepAlive >> 1);
                        ulong makeNewLife = mask ^ 0xCCCCCCCCCCCCCCCCul;
                        makeNewLife &= (makeNewLife >> 2);
                        makeNewLife &= (makeNewLife >> 1);

                        *(ulong*)(nextLinePtr - WIDTH + x) = (keepAlive | makeNewLife) & 0x1111111111111111ul;
                    }

                    *(byte*)(nextLinePtr - WIDTH) = 0;
                    *(byte*)(nextLinePtr - 1) = 0;

                }

            }

        }
    }
}
