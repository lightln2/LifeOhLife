using LifeOhLife;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace TestYourLife
{
    [TestClass]
    public class TestDifferentLives
    {
        [TestMethod]
        public unsafe void Test_AVX_BitsToBytes()
        {
            uint x = 0b0000_0001__0010_0011__0100_0101__0110_0111u;
            uint y = 0b1000_1001__1010_1011__1100_1101__1110_1111u;
            Vector256<byte> mask1, mask2, zero = Vector256<byte>.Zero, one, ff;

            byte[] mask1_bytes = new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3,
            };
            byte[] mask2_bytes = new byte[]
            {
                0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80,
                0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80,
                0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80,
                0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80,
            };

            fixed (byte* ptr = mask1_bytes) mask1 = Avx2.LoadVector256(ptr);
            fixed (byte* ptr = mask2_bytes) mask2 = Avx2.LoadVector256(ptr);

            byte one_byte = 1;
            one = Avx2.BroadcastScalarToVector256(&one_byte);
            byte ff_byte = 0xff;
            ff = Avx2.BroadcastScalarToVector256(&ff_byte);

            // ***** load **** //
            Vector256<uint> ux = Avx2.BroadcastScalarToVector256(&y);
            Vector256<byte> bx = ux.AsByte();
            Vector256<byte> shuffled_x = Avx2.Shuffle(bx, mask1);
            Vector256<byte> result_x = Avx2.And(shuffled_x, mask2);
            result_x = Avx2.Min(result_x, one);

            // ***** store **** //
            Vector256<byte> reverse_x = Avx2.CompareEqual(result_x, zero);
            reverse_x = Avx2.AndNot(reverse_x, ff);

            uint reversed_x = (uint)Avx2.MoveMask(reverse_x);
            Assert.AreEqual(reversed_x, y);
        }

        [TestMethod]
        public unsafe void Test_AVX_BitsToBytesCompressed()
        {
            ulong x = 0b0000_0001__0010_0011__0100_0101__0110_0111____1000_1001__1010_1011__1100_1101__1110_1111ul;
            Vector256<byte> mask1, mask2, mask3, zero = Vector256<byte>.Zero, one, ff, low4, hi4, restore_mask1, restore_mask2;

            byte[] mask1_bytes = new byte[]
            {
                0, 0, 0, 0,
                1, 1, 1, 1,
                2, 2, 2, 2,
                3, 3, 3, 3,
                4, 4, 4, 4,
                5, 5, 5, 5,
                6, 6, 6, 6,
                7, 7, 7, 7,
            };

            byte[] mask2_bytes = new byte[]
            {
                0x01, 0x04, 0x10, 0x40,
                0x01, 0x04, 0x10, 0x40,
                0x01, 0x04, 0x10, 0x40,
                0x01, 0x04, 0x10, 0x40,
                0x01, 0x04, 0x10, 0x40,
                0x01, 0x04, 0x10, 0x40,
                0x01, 0x04, 0x10, 0x40,
                0x01, 0x04, 0x10, 0x40,
            };

            byte[] mask3_bytes = new byte[]
            {
                0x02, 0x08, 0x20, 0x80,
                0x02, 0x08, 0x20, 0x80,
                0x02, 0x08, 0x20, 0x80,
                0x02, 0x08, 0x20, 0x80,
                0x02, 0x08, 0x20, 0x80,
                0x02, 0x08, 0x20, 0x80,
                0x02, 0x08, 0x20, 0x80,
                0x02, 0x08, 0x20, 0x80,
            };

            byte[] restore_mask1_bytes = new byte[]
            {
                0, 255, 1, 255, 2, 255, 3, 255, 4, 255, 5, 255, 6, 255, 7, 255,
                0, 255, 1, 255, 2, 255, 3, 255, 4, 255, 5, 255, 6, 255, 7, 255,
            };
            byte[] restore_mask2_bytes = new byte[]
            {
                8, 255, 9, 255, 10, 255, 11, 255, 12, 255, 13, 255, 14, 255, 15, 255,
                8, 255, 9, 255, 10, 255, 11, 255, 12, 255, 13, 255, 14, 255, 15, 255,
            };

            fixed (byte* ptr = mask1_bytes) mask1 = Avx2.LoadVector256(ptr);
            fixed (byte* ptr = mask2_bytes) mask2 = Avx2.LoadVector256(ptr);
            fixed (byte* ptr = mask3_bytes) mask3 = Avx2.LoadVector256(ptr);
            fixed (byte* ptr = restore_mask1_bytes) restore_mask1 = Avx2.LoadVector256(ptr);
            fixed (byte* ptr = restore_mask2_bytes) restore_mask2 = Avx2.LoadVector256(ptr);

            byte one_byte = 1;
            one = Avx2.BroadcastScalarToVector256(&one_byte);
            byte ff_byte = 0xff;
            ff = Avx2.BroadcastScalarToVector256(&ff_byte);
            byte lo4_byte = 0x0f;
            low4 = Avx2.BroadcastScalarToVector256(&lo4_byte);
            byte hi4_byte = 0xf0;
            hi4 = Avx2.BroadcastScalarToVector256(&hi4_byte);

            // ***** load **** //
            Vector256<byte> v = Avx2.BroadcastScalarToVector256(&x).AsByte();
            v = Avx2.Shuffle(v, mask1);
            Vector256<byte> v1 = Avx2.And(v, mask2);
            v1 = Avx2.Min(v1, one);
            Vector256<byte> v2 = Avx2.And(v, mask3);
            v2 = Avx2.Min(v2, one);
            v2 = Avx2.ShiftLeftLogical(v2.AsUInt64(), 4).AsByte();
            v = Avx2.Or(v1, v2);

            // ***** restore **** //

            v1 = Avx2.And(v, low4);
            v2 = Avx2.And(v, hi4);
            v1 = Avx2.CompareEqual(v1, zero);
            v1 = Avx2.AndNot(v1, ff);
            v2 = Avx2.CompareEqual(v2, zero);
            v2 = Avx2.AndNot(v2, ff);

            Vector256<byte> r1 = Avx2.Shuffle(v1, restore_mask1);
            Vector256<byte> r2 = Avx2.Shuffle(v1, restore_mask2);
            Vector256<byte> r3 = Avx2.Shuffle(v2, restore_mask1);
            Vector256<byte> r4 = Avx2.Shuffle(v2, restore_mask2);
            ulong restored1 = (uint)Avx2.MoveMask(r1);
            ulong restored2 = (uint)Avx2.MoveMask(r2);
            ulong restored3 = (uint)Avx2.MoveMask(r3);
            ulong restored4 = (uint)Avx2.MoveMask(r4);

            ulong restored_lo = restored1 | (restored3 << 1);
            ulong restored_hi = restored2 | (restored4 << 1);
            ulong restored =
                (restored_lo & 0xFFFF) |
                (restored_hi & 0xFFFF) << 16 |
                (restored_lo & 0xFFFF0000) << 16 |
                (restored_hi & 0xFFFF0000) << 32;

            Assert.AreEqual(x, restored);
        }


        [TestMethod]
        public void Test_1_SimpleLife()
        {
            LifeTesting.PerformAllTests(new SimpleLife());
        }

        
        [TestMethod]
        public void Test_2_LifeBytes()
        {
            LifeTesting.PerformAllTests(new LifeBytes());
        }

        [TestMethod]
        public void Test_3_LongLife()
        {
            LifeTesting.PerformAllTests(new LongLife());
        }

        [TestMethod]
        public void Test_4_LifeIsLookingUp()
        {
            LifeTesting.PerformAllTests(new LifeIsLookingUp());
        }

        [TestMethod]
        public void Test_5_LifeInBits()
        {
            LifeTesting.PerformAllTests(new LifeInBits());
        }

        [TestMethod]
        public void Test_6_LifeIsABitMagic()
        {
            LifeTesting.PerformAllTests(new LifeIsABitMagic());
        }

        [TestMethod]
        public void Test_7_AdvancedLifeExtension()
        {
            LifeTesting.PerformAllTests(new AdvancedLifeExtensions());
        }

        [TestMethod]
        public void Test_LifeInList()
        {
            LifeTesting.PerformAllTests(new LifeInList());
        }

        [TestMethod]
        public void Test_LifeIsChange()
        {
            LifeTesting.PerformAllTests(new LifeIsChange());
        }

        [TestMethod]
        public void Test_LifeLine_Bytes()
        {
            LifeTesting.PerformAllTests(new LifeInLine_Bytes());
        }

        [TestMethod]
        public void Test_LifeLine_Long()
        {
            LifeTesting.PerformAllTests(new LifeInLine_Long());
        }

        [TestMethod]
        public void Test_LifeLine_LongCompressed()
        {
            LifeTesting.PerformAllTests(new LifeInLine_LongCompressed());
        }

        [TestMethod]
        public void Test_7_AdvancedLifeExtensionsInLine()
        {
            LifeTesting.PerformAllTests(new AdvancedLifeExtensionsInLine());
        }

        [TestMethod]
        public void Test_7_AdvancedLifeExtensionsInLineCompressed()
        {
            LifeTesting.PerformAllTests(new AdvancedLifeExtensionsInLineCompressed());
        }

    }
}
