namespace LLibrary.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.IO;
    using System.Globalization;

    [TestClass]
    public class Tests
    {
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

        private enum Enum
        {
            Foo,
            Bar,
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(FilePath);
        }

        [TestMethod]
        public void Vanilla()
        {
            using (var logger = new L())
            {
                logger.LogInfo("Some information.");
                Assert.IsTrue(FileContent.EndsWith("INFO  Some information."));
            }
        }

        [TestMethod]
        public void WithFormat()
        {
            using (var logger = new L())
            {
                logger.LogError("A {0} happened: {1}", typeof(Exception), "BOOM!");
                Assert.IsTrue(FileContent.EndsWith("ERROR A System.Exception happened: BOOM!"));
            }
        }

        [TestMethod]
        public void NotString()
        {
            using (var logger = new L())
            {
                logger.LogError(new Exception("BOOM!"));
                Assert.IsTrue(FileContent.EndsWith("ERROR System.Exception: BOOM!"));
            }
        }

        [TestMethod]
        public void NoWriteLock()
        {
            using (var logger = new L())
            {
                logger.LogInfo("Some information.");

                using (var file = File.Open(FilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                using (var writer = new StreamWriter(file))
                {
                    writer.WriteLine("Do a barrel roll!");
                }
            }
        }

        [TestMethod]
        public void EnumAsLabel()
        {
            using (var logger = new L())
            {
                logger.Log(Enum.Foo, "Here's foo.");
                Assert.IsTrue(FileContent.EndsWith("FOO   Here's foo."));

                logger.Log(Enum.Bar, "And here's bar.");
                Assert.IsTrue(FileContent.EndsWith("BAR   And here's bar."));
            }
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void DisposedException()
        {
            using (var logger = new L())
            {
                logger.Dispose();
                logger.LogInfo("Some information.");
            }
        }

        [TestMethod]
        public void EnabledLabels()
        {
            var cfg = new LConfiguration { EnabledLabels = new[] { "FOO" } };

            using (var logger = new L(cfg))
            {
                logger.Log(Enum.Foo, "Here's foo.");
                Assert.IsTrue(FileContent.EndsWith("FOO   Here's foo."));

                logger.Log(Enum.Bar, "And here's bar.");
                Assert.IsTrue(FileContent.EndsWith("FOO   Here's foo."));
            }
        }
    }
}
