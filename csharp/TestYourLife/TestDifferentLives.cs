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

            //result_x = Avx2.CompareEqual(result_x, zero);
            //result_x = Avx2.AndNot(result_x, one);

            // ***** store **** //
            Vector256<byte> reverse_x = Avx2.CompareEqual(result_x, zero);
            reverse_x = Avx2.AndNot(reverse_x, ff);

            uint reversed_x = (uint)Avx2.MoveMask(reverse_x);
            Assert.AreEqual(reversed_x, y);
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

    }
}
