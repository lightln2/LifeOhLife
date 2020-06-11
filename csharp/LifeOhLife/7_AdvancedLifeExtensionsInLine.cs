using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// Uses AVX2 instruction set to handle field cells in chunks of 32 bytes
    /// The fastest algorithm so far, but does not work on old processors.
    /// Also note that SIMD intrinsics are only available in .NET Core, so this class
    /// won't compile in .NET Framework
    /// </summary>
    public unsafe class AdvancedLifeExtensionsInLine : LifeJourney
    {
        byte[] field = new byte[WIDTH * HEIGHT + 32];

        byte[] upperLineSumOf2 = new byte[WIDTH];
        byte[] upperLineSumOf3 = new byte[WIDTH];
        byte[] middleLineSumOf2 = new byte[WIDTH];
        byte[] middleLineSumOf3 = new byte[WIDTH];
        byte[] lowerLineSumOf2 = new byte[WIDTH];
        byte[] lowerLineSumOf3 = new byte[WIDTH];

        private Vector256<byte> v_1, v_2, v_3;
        private Vector256<byte> v_lookup;

        public AdvancedLifeExtensionsInLine()
        {
            if (!Avx2.IsSupported) throw new NotImplementedException("Not in this life!!!");
            
            byte b_1 = 1, b_2 = 2, b_3 = 3;
            v_1 = Avx2.BroadcastScalarToVector256(&b_1);
            v_2 = Avx2.BroadcastScalarToVector256(&b_2);
            v_3 = Avx2.BroadcastScalarToVector256(&b_3);
            byte[] lookup = new byte[]
            {
                0, 0, 0, 1, 0, 0, 0, 0,
                0, 0, 1, 1, 0, 0, 0, 0,

                0, 0, 0, 1, 0, 0, 0, 0,
                0, 0, 1, 1, 0, 0, 0, 0
            };
            fixed (byte* ptr = lookup) v_lookup = Avx2.LoadVector256(ptr);
        }

        public override bool Get(int x, int y) => field[y * WIDTH + x] == 1;

        public override void Set(int x, int y, bool value) => field[y * WIDTH + x] = (byte)(value ? 1 : 0);

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
                byte* nextLinePtr = currentFieldPtr + WIDTH;

                for (int x = 0; x < WIDTH; x += 32)
                {
                    Avx2.Store(upper2 + x, Vector256<byte>.Zero);
                    Avx2.Store(upper3 + x, Vector256<byte>.Zero);
                    Avx2.Store(middle2 + x, Vector256<byte>.Zero);
                    Avx2.Store(middle3 + x, Vector256<byte>.Zero);
                    Vector256<byte> sum2 = Avx2.Add(Avx2.LoadVector256(nextLinePtr + x - 1), Avx2.LoadVector256(nextLinePtr + x + 1));
                    Avx2.Store(lower2 + x, sum2);
                    Avx2.Store(lower3 + x, Avx2.Add(sum2, Avx2.LoadVector256(nextLinePtr + x)));
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

                    for (int x = 0; x < WIDTH; x += 32)
                    {
                        Vector256<byte> left = Avx2.LoadVector256(nextLinePtr + x - 1);
                        Vector256<byte> center = Avx2.LoadVector256(nextLinePtr + x);
                        Vector256<byte> right = Avx2.LoadVector256(nextLinePtr + x + 1);
                        Vector256<byte> lowerSum2 = Avx2.Add(left, right);
                        Vector256<byte> lowerSum3 = Avx2.Add(lowerSum2, center);

                        Avx2.Store(lower2 + x, lowerSum2);
                        Avx2.Store(lower3 + x, Avx2.Add(lowerSum2, center));

                        Vector256<byte> neighbours =
                            Avx2.Add(
                                Avx2.LoadVector256(middle2 + x),
                                Avx2.Add(Avx2.LoadVector256(upper3 + x), lowerSum3));
                        Vector256<byte> alive = Avx2.LoadVector256(nextLinePtr - WIDTH + x);
                            //Avx2.Subtract(Avx2.LoadVector256(middle3 + x), Avx2.LoadVector256(middle2 + x));

                        alive = Avx2.ShiftLeftLogical(alive.AsUInt64(), (byte)3).AsByte();
                        Vector256<byte> mask = Avx2.Or(neighbours, alive);
                        Vector256<byte> shouldBeAlive = Avx2.Shuffle(v_lookup, mask);

                        Avx2.Store(nextLinePtr - WIDTH + x, shouldBeAlive);
                    }
                    *(byte*)(nextLinePtr - WIDTH) = 0;
                    *(byte*)(nextLinePtr - 1) = 0;
                }
            }

        }
    }
}
