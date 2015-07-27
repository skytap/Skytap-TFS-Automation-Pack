using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skytap.Cloud;
using Skytap.Utilities;

namespace SkytapCLITests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void Main_InvalidInputs()
        {
            var configName = "UnitTestConfig_" + DateTime.Now.TimeOfDay;
            var argsStart = new[] { "/configid", "493456", "/templateid", "166433", "/configname", configName };
            var argsStop = new[] { "/configname", configName };

            try
            {
                Assert.AreEqual(-1, Program.Main(argsStart));
                Assert.AreEqual(-1, Program.Main(argsStop));
                Assert.IsNotNull(LoggerFactory.GetLogger());
                Assert.IsInstanceOfType(LoggerFactory.GetLogger(), typeof(TraceLogger));
            }
            finally
            {
                // Clean-up a generated log file as we won't need it for this test. In the future
                // it would be more efficient to create a Null logger and not log anything to the
                // file system, but this is minor. If > 1 test would leverage this, recommend it
                // get done.
                File.Delete(((TraceLogger)LoggerFactory.GetLogger()).LogFilePath);
            }
        }

    }
}
