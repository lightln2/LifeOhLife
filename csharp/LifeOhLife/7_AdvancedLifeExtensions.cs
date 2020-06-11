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
    public unsafe class AdvancedLifeExtensions : LifeJourney
    {
        byte[] currentField = new byte[2* WIDTH + WIDTH * HEIGHT];
        byte[] nextField = new byte[2 * WIDTH + WIDTH * HEIGHT];
        private Vector256<byte> v_1, v_2, v_3;

        public AdvancedLifeExtensions()
        {
            if (!Avx2.IsSupported) throw new NotImplementedException("Not in this life!!!");
            
            byte b_1 = 1, b_2 = 2, b_3 = 3;
            v_1 = Avx2.BroadcastScalarToVector256(&b_1);
            v_2 = Avx2.BroadcastScalarToVector256(&b_2);
            v_3 = Avx2.BroadcastScalarToVector256(&b_3);
        }

        public override bool Get(int x, int y) => currentField[WIDTH + y * WIDTH + x] == 1;

        public override void Set(int x, int y, bool value) => currentField[WIDTH + y * WIDTH + x] = (byte)(value ? 1 : 0);

        public override void Step()
        {
            fixed (byte* fieldPtr = currentField, nextFieldPtr = nextField)
            {
                for (int i = 2 * WIDTH; i < currentField.Length - 2 * WIDTH; i += 32)
                {
                    Vector256<byte> topLeft = Avx.LoadVector256(fieldPtr + i - WIDTH - 1);
                    Vector256<byte> top = Avx.LoadVector256(fieldPtr + i - WIDTH);
                    Vector256<byte> topRight = Avx.LoadVector256(fieldPtr + i - WIDTH + 1);
                    Vector256<byte> left = Avx.LoadVector256(fieldPtr + i - 1);
                    Vector256<byte> right = Avx.LoadVector256(fieldPtr + i + 1);
                    Vector256<byte> bottomLeft = Avx.LoadVector256(fieldPtr + i + WIDTH - 1);
                    Vector256<byte> bottom = Avx.LoadVector256(fieldPtr + i + WIDTH);
                    Vector256<byte> bottomRight = Avx.LoadVector256(fieldPtr + i + WIDTH + 1);

                    Vector256<byte> sum1 = Avx2.Add(topLeft, top);
                    Vector256<byte> sum2 = Avx2.Add(topRight, left);
                    Vector256<byte> sum3 = Avx2.Add(right, bottomLeft);
                    Vector256<byte> sum4 = Avx2.Add(bottom, bottomRight);

                    Vector256<byte> sum5 = Avx2.Add(sum1, sum2);
                    Vector256<byte> sum6 = Avx2.Add(sum3, sum4);

                    Vector256<byte> neighbours = Avx2.Add(sum5, sum6);

                    Vector256<byte> alive = Avx.LoadVector256(fieldPtr + i);

                    Vector256<byte> hasTwoNeighbours = Avx2.CompareEqual(neighbours, v_2);
                    Vector256<byte> hasThreeNeighbours = Avx2.CompareEqual(neighbours, v_3);
                    hasThreeNeighbours = Avx2.And(hasThreeNeighbours, v_1);
                    Vector256<byte> aliveAndTwoNeighbours = Avx2.And(alive, hasTwoNeighbours);
                    Vector256<byte> shouldBeAlive = Avx2.Or(aliveAndTwoNeighbours, hasThreeNeighbours);
                    shouldBeAlive = Avx2.And(shouldBeAlive, v_1);
                    Avx2.Store(nextFieldPtr + i, shouldBeAlive);
                }

                byte[] tempField = currentField;
                currentField = nextField;
                nextField = tempField;
            }

            for (int y = 1; y < HEIGHT - 1; y++)
            {
                currentField[WIDTH + y * WIDTH] = 0;
                currentField[WIDTH + y * WIDTH + WIDTH - 1] = 0;
            }

        }
    }
}
