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
    public unsafe class LifeCompressed : LifeJourney
    {
        private static readonly byte[] ONES = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        private static readonly byte[] TWOS = new byte[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        private static readonly byte[] THREES = new byte[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
        private Vector256<byte> v_1, v_2, v_3;

        byte[] field = new byte[WIDTH * HEIGHT];
        byte[] temp = new byte[WIDTH * HEIGHT];

        public LifeCompressed()
        {
            if (!Avx2.IsSupported) throw new NotImplementedException("Not in this life!!!");
            fixed (byte* ptr = ONES) v_1 = Avx.LoadVector256(ptr);
            fixed (byte* ptr = TWOS) v_2 = Avx.LoadVector256(ptr);
            fixed (byte* ptr = THREES) v_3 = Avx.LoadVector256(ptr);
        }

        public override bool Get(int i, int j) => field[j * WIDTH + i] == 1;

        public override void Set(int i, int j, bool value) => field[j * WIDTH + i] = (byte)(value ? 1 : 0);

        public override void Step()
        {
            fixed (byte* fieldPtr = field, tempPtr = temp)
            {
                Vector256<byte> zero = Vector256<byte>.Zero;
                for (int i = 0; i < WIDTH * HEIGHT; i += 32)
                {
                    Avx.Store(tempPtr + i, zero);
                }

                for (int i = WIDTH; i < WIDTH * HEIGHT - WIDTH; i += 32)
                {
                    Vector256<byte> src1 = Avx.LoadVector256(fieldPtr + i - WIDTH - 1);
                    Vector256<byte> src2 = Avx.LoadVector256(fieldPtr + i - WIDTH);
                    Vector256<byte> sum1 = Avx2.Add(src1, src2);
                    Vector256<byte> src3 = Avx.LoadVector256(fieldPtr + i - WIDTH + 1);
                    Vector256<byte> src4 = Avx.LoadVector256(fieldPtr + i - 1);
                    Vector256<byte> sum2 = Avx2.Add(src3, src4);
                    Vector256<byte> src5 = Avx.LoadVector256(fieldPtr + i + 1);
                    Vector256<byte> src6 = Avx.LoadVector256(fieldPtr + i + WIDTH - 1);
                    Vector256<byte> sum3 = Avx2.Add(src5, src6);
                    Vector256<byte> src7 = Avx.LoadVector256(fieldPtr + i + WIDTH);
                    Vector256<byte> src8 = Avx.LoadVector256(fieldPtr + i + WIDTH + 1);
                    Vector256<byte> sum4 = Avx2.Add(src7, src8);

                    Vector256<byte> sum5 = Avx2.Add(sum1, sum2);
                    Vector256<byte> sum6 = Avx2.Add(sum3, sum4);
                    Vector256<byte> sum = Avx2.Add(sum5, sum6);

                    Avx.Store(tempPtr + i, sum);
                }

                for (int i = WIDTH; i < WIDTH * HEIGHT - WIDTH; i += 32)
                {
                    Vector256<byte> neighbours = Avx.LoadVector256(tempPtr + i);
                    Vector256<byte> alive = Avx.LoadVector256(fieldPtr + i);

                    Vector256<byte> isAlive = Avx2.CompareEqual(alive, v_1);
                    Vector256<byte> isTwoNeighbours = Avx2.CompareEqual(neighbours, v_2);
                    Vector256<byte> isThreeNeighbours = Avx2.CompareEqual(neighbours, v_3);

                    Vector256<byte> isTwoOrThreeNeighbours = Avx2.Or(isTwoNeighbours, isThreeNeighbours);
                    Vector256<byte> aliveAndNeighbours = Avx2.And(isAlive, isTwoOrThreeNeighbours);

                    Vector256<byte> shouldBeAlive = Avx2.Or(aliveAndNeighbours, isThreeNeighbours);
                    shouldBeAlive = Avx2.And(shouldBeAlive, v_1);

                    Avx2.Store(fieldPtr + i, shouldBeAlive);
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
