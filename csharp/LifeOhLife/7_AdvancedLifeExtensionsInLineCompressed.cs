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
    public unsafe class AdvancedLifeExtensionsInLineCompressed : LifeJourney
    {
        const int LINE_WIDTH = WIDTH / 2;
        byte[] field = new byte[LINE_WIDTH * HEIGHT + 32];

        byte[] upperLineSumOf2 = new byte[LINE_WIDTH];
        byte[] upperLineSumOf3 = new byte[LINE_WIDTH];
        byte[] middleLineSumOf2 = new byte[LINE_WIDTH];
        byte[] middleLineSumOf3 = new byte[LINE_WIDTH];
        byte[] lowerLineSumOf2 = new byte[LINE_WIDTH];
        byte[] lowerLineSumOf3 = new byte[LINE_WIDTH];

        private Vector256<byte> v_lookup;
        private Vector256<byte> v_hi, v_lo;

        public AdvancedLifeExtensionsInLineCompressed()
        {
            if (!Avx2.IsSupported) throw new NotImplementedException("Not in this life!!!");
            
            byte[] lookup = new byte[]
            {
                0, 0, 0, 1, 0, 0, 0, 0,
                0, 0, 1, 1, 0, 0, 0, 0,

                0, 0, 0, 1, 0, 0, 0, 0,
                0, 0, 1, 1, 0, 0, 0, 0
            };
            fixed (byte* ptr = lookup) v_lookup = Avx2.LoadVector256(ptr);

            byte b_hi = 0xF0;
            byte b_lo = 0x0F;
            v_hi = Avx2.BroadcastScalarToVector256(&b_hi);
            v_lo = Avx2.BroadcastScalarToVector256(&b_lo);

        }

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

                for (int x = 0; x < LINE_WIDTH; x += 32)
                {
                    Avx2.Store(upper2 + x, Vector256<byte>.Zero);
                    Avx2.Store(upper3 + x, Vector256<byte>.Zero);
                    Avx2.Store(middle2 + x, Vector256<byte>.Zero);
                    Avx2.Store(middle3 + x, Vector256<byte>.Zero);
                    Vector256<byte> nextLeft = Avx2.LoadVector256(nextLinePtr + x - 8);
                    Vector256<byte> nextCenter = Avx2.LoadVector256(nextLinePtr + x);
                    Vector256<byte> nextRight = Avx2.LoadVector256(nextLinePtr + x + 8);
                    Vector256<byte> lowerSum2 =
                        Avx2.Add(
                            Avx2.Add(
                                Avx2.ShiftRightLogical(nextCenter.AsUInt64(), 4), Avx2.ShiftLeftLogical(nextCenter.AsUInt64(), 4)),
                            Avx2.Add(
                                Avx2.ShiftRightLogical(nextLeft.AsUInt64(), 60), Avx2.ShiftLeftLogical(nextRight.AsUInt64(), 60))).AsByte();
                    Avx2.Store(lower2 + x, lowerSum2);
                    Vector256<byte> lowerSum3 = Avx2.Add(lowerSum2, nextCenter);
                    Avx2.Store(lower3 + x, lowerSum3);
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

                    for (int x = 0; x < LINE_WIDTH; x += 32)
                    {
                        Vector256<byte> nextLeft = Avx2.LoadVector256(nextLinePtr + x - 8);
                        Vector256<byte> nextCenter = Avx2.LoadVector256(nextLinePtr + x);
                        Vector256<byte> nextRight = Avx2.LoadVector256(nextLinePtr + x + 8);
                        Vector256<byte> lowerSum2 =
                            Avx2.Add(
                                Avx2.Add(
                                    Avx2.ShiftRightLogical(nextCenter.AsUInt64(), 4), Avx2.ShiftLeftLogical(nextCenter.AsUInt64(), 4)),
                                Avx2.Add(
                                    Avx2.ShiftRightLogical(nextLeft.AsUInt64(), 60), Avx2.ShiftLeftLogical(nextRight.AsUInt64(), 60))).AsByte();
                        Avx2.Store(lower2 + x, lowerSum2);
                        Vector256<byte> lowerSum3 = Avx2.Add(lowerSum2, nextCenter);
                        Avx2.Store(lower2 + x, lowerSum2);
                        Avx2.Store(lower3 + x, Avx2.Add(lowerSum2, nextCenter));

                        Vector256<byte> neighbours =
                            Avx2.Add(
                                Avx2.LoadVector256(middle2 + x),
                                Avx2.Add(Avx2.LoadVector256(upper3 + x), lowerSum3));
                        Vector256<byte> alive = Avx2.LoadVector256(nextLinePtr - LINE_WIDTH + x);

                        alive = Avx2.ShiftLeftLogical(alive.AsUInt64(), (byte)3).AsByte();
                        Vector256<byte> mask = Avx2.Or(neighbours, alive);

                        Vector256<byte> mask_hi = Avx2.And(mask, v_hi);
                        Vector256<byte> mask_lo = Avx2.And(mask, v_lo);
                        mask_hi = Avx2.ShiftRightLogical(mask_hi.AsUInt64(), 4).AsByte();
                        Vector256<byte> shouldBeAlive_hi = Avx2.Shuffle(v_lookup, mask_hi);
                        Vector256<byte> shouldBeAlive_lo = Avx2.Shuffle(v_lookup, mask_lo);
                        shouldBeAlive_hi = Avx2.ShiftLeftLogical(shouldBeAlive_hi.AsUInt64(), 4).AsByte();

                        Vector256<byte> shouldBeAlive = Avx2.Or(shouldBeAlive_hi, shouldBeAlive_lo);

                        Avx2.Store(nextLinePtr - LINE_WIDTH + x, shouldBeAlive);
                    }
                    *(byte*)(nextLinePtr - LINE_WIDTH) &= 0xF0;
                    *(byte*)(nextLinePtr - 1) &= 0x0F;
                }
            }

        }
    }
}
