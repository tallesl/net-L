namespace LLibrary.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Runtime.CompilerServices;
    using System.IO;
    using System.Globalization;

    [TestClass]
    public class Tests
    {
        private L L;

        private string FilePath
        {
            get
            {
                var today = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                return string.Format(@"logs\{0}.log", today);
            }
        }

        private string FileContent
        {
            get
            {
                using (var file = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(file))
                {
                    return reader.ReadToEnd().TrimEnd();
                }
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            L = new L();
        }

        [TestCleanup]
        public void Cleanup()
        {
            L.Dispose();
            File.Delete(FilePath);
        }

        [TestMethod]
        public void WithFormat()
        {
            var e = new Exception("BOOM!");

            L.Log("ERROR", "A {0} happened: {1}", e.GetType(), e.Message);
            Assert.IsTrue(FileContent.EndsWith("ERROR A System.Exception happened: BOOM!"));
        }

        [TestMethod]
        public void WithoutFormat()
        {
            L.Log("INFO", "Some information.");
            Assert.IsTrue(FileContent.EndsWith("INFO  Some information."));
        }

        [TestMethod]
        public void NoWriteLock()
        {
            L.Log("INFO", "Some information.");

            using (var file = File.Open(FilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            using (var writer = new StreamWriter(file))
            {
                writer.WriteLine("Do a barrel roll!");
            }
        }
    }
}
