using LifeOhLife;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestYourLife
{
    [TestClass]
    public class TestDifferentLives
    {
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

    }
}
