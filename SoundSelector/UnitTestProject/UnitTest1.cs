using System;
using System.Diagnostics;
using System.IO;
using Business;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTestComparisonEngine
    {
        [TestMethod]
        public void ShouldCallReportProgress()
        {
            int nbreportProgressCalled = 0;

            try
            {
                if (!Directory.Exists(Environment.CurrentDirectory + "\\AudioFilesTest"))
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + "\\AudioFilesTest");

                    File.Create(Environment.CurrentDirectory + "\\AudioFilesTest\\Test1.mp3");
                    File.Create(Environment.CurrentDirectory + "\\AudioFilesTest\\Test2.mp3");
                    File.Create(Environment.CurrentDirectory + "\\AudioFilesTest\\Test3.mp3");
                }                

                ComparisonEngine compEngine = new ComparisonEngine(null, Environment.CurrentDirectory + "\\AudioFilesTest", (double progress) =>
                {
                    nbreportProgressCalled++;
                }, 0, 0);

                compEngine.CompareAll();

                Assert.AreEqual(nbreportProgressCalled, 3);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }
    }
}
