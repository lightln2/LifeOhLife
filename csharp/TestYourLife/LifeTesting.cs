using LifeOhLife;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestYourLife
{
    public class LifeTesting
    {
        public static void PerformAllTests(LifeJourney life)
        {
            TestGetSet(life);
            TestSetSimpleFigure(life);
            TestSimpleFigureAtStart(life);
            TestSimpleFigureAtSecondPos(life);
            TestSimpleFigure(life);
            TestGenerate(life);
            TestRandomField(life);
        }

        private static void TestGetSet(LifeJourney life)
        {
            life.Clear();
            life.Set(1, 1, true);
            life.Set(100, 100, true);
            life.Set(99, 99, true);
            Assert.AreEqual(true, life.Get(1, 1));
            Assert.AreEqual(true, life.Get(100, 100));
            Assert.AreEqual(true, life.Get(99, 99));
            Assert.AreEqual(false, life.Get(1, 2));
            Assert.AreEqual(false, life.Get(100, 99));
            Assert.AreEqual(false, life.Get(98, 99));

            life.Set(100, 100, false);
            Assert.AreEqual(true, life.Get(1, 1));
            Assert.AreEqual(false, life.Get(100, 100));
            Assert.AreEqual(true, life.Get(99, 99));
            Assert.AreEqual(false, life.Get(1, 2));
            Assert.AreEqual(false, life.Get(100, 99));
            Assert.AreEqual(false, life.Get(98, 99));
        }

        private static void TestSetSimpleFigure(LifeJourney life)
        {
            life.Clear();
            life.SetRectangle(110, 110, @"-x-
                                          xxx
                                          -x-");
            life.TestRectangle(109, 109, @"-----
                                           --x--
                                           -xxx-
                                           --x--
                                           -----");
        }

        private static void TestSimpleFigureAtStart(LifeJourney life)
        {
            life.Clear();
            life.SetRectangle(1, 1, @"-x-
                                      xxx
                                      -x-");
            life.Step();
            life.TestRectangle(1, 1, @"xxx-
                                       x-x-
                                       xxx-
                                       ----");
        }

        private static void TestSimpleFigureAtSecondPos(LifeJourney life)
        {
            life.Clear();
            life.SetRectangle(2, 2, @"-x-
                                      xxx
                                      -x-");
            life.Step();
            life.TestRectangle(1, 1, @"-----
                                       -xxx-
                                       -x-x-
                                       -xxx-
                                       -----");
        }

        private static void TestSimpleFigure(LifeJourney life)
        {
            life.Clear();
            life.SetRectangle(110, 110, @"-x-
                                          xxx
                                          -x-");
            life.Step();
            life.TestRectangle(109, 109, @"-----
                                           -xxx-
                                           -x-x-
                                           -xxx-
                                           -----");
        }

        private static void TestGenerate(LifeJourney life)
        {
            life.GenerateRandomField(12345, 0.5);
            Assert.AreEqual(1034216, life.GetLiveCellsCount());
            Assert.AreEqual(1583560213, life.GetFingerprint());

            life.GenerateRandomField(12345, 0.1);
            Assert.AreEqual(206625, life.GetLiveCellsCount());
            Assert.AreEqual(-1430538206, life.GetFingerprint());
        }

        private static void TestRandomField(LifeJourney life)
        {
            life.GenerateRandomField(12345, 0.5);
            life.Step();
            Assert.AreEqual(565797, life.GetLiveCellsCount());
            Assert.AreEqual(-717568334, life.GetFingerprint());

            life.GenerateRandomField(12345, 0.1);
            life.Step();
            Assert.AreEqual(98717, life.GetLiveCellsCount());
            Assert.AreEqual(1237589375, life.GetFingerprint());
        }


    }
}
