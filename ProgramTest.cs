using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinerHashRateMonitor;

namespace Test
{
    [TestClass]
    public class ProgramTest
    {
        [TestMethod]
        public void TestConvertToMinutes()
        {
            Program.ConvertToMinutes("1d2h10m1s");
        }
    }
}
