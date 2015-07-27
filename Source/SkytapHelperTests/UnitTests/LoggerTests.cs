// 
// LoggerTests.cs
/**
 * Copyright 2014 Skytap Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 **/
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skytap.Utilities;

namespace SkytapHelperTests
{
    [TestClass]
    public class LoggerTests
    {
        private TraceLogger _traceLogger;

        private Logger CreateTraceLogger()
        {
            _traceLogger = new TraceLogger(new DefaultTraceListener());
            return _traceLogger;
        }

        [TestMethod]
        public void LoggerUtilities_CreateUniqueLogFilename_Default()
        {
            var filename = LoggerUtilities.CreateUniqueLogFilename();

            Assert.IsTrue(filename.Contains(LoggerUtilities.DefaultLogFilePrefix));
            Assert.IsTrue(filename.Contains(Path.GetTempPath()));
            Assert.IsTrue(filename.Contains(".log"));
        }

        [TestMethod]
        public void LoggerUtilities_CreateUniqueLogFilename_CustomPrefix()
        {
            const string logfilenamePrefix = "SomePrefix";
            var filename = LoggerUtilities.CreateUniqueLogFilename(logfilenamePrefix);

            Assert.IsTrue(filename.Contains(logfilenamePrefix));
            Assert.IsTrue(filename.Contains(DateTime.Now.Year.ToString(new NumberFormatInfo())));
        }

        [TestMethod]
        public void LoggerFactory_CreateTraceLogger_Success()
        {
            LoggerFactory.LoggerType = LoggerTypes.Trace;

            var logger = LoggerFactory.GetLogger();

            Assert.IsNotNull(logger);
            Assert.IsInstanceOfType(logger, typeof(TraceLogger));
        }

        [TestMethod]
        public void TraceLogger_Create_Success()
        {
            using (CreateTraceLogger())
            {
                Assert.IsTrue(_traceLogger.LogFilePath.Contains(LoggerUtilities.DefaultLogFilePrefix));
                Assert.AreEqual(string.Empty, _traceLogger.LogFilePrefix);
            }
        }

        [TestMethod]
        public void TraceLogger_Create_LogFilePrefixOverrideSuccess()
        {
            const string logFilePrefix = "SomePrefix";
            using (CreateTraceLogger())
            {
                _traceLogger.LogFilePrefix = logFilePrefix;

                Assert.IsTrue(_traceLogger.LogFilePath.Contains(logFilePrefix));
            }
        }

        [TestMethod]
        public void TraceLogger_LogInfo_Success()
        {
            // NOTE: a key part of this test is not getting any unexpected exceptions. There aren't
            // many assertions on conditions unless we decide to subclass the TraceLogger (and unseal it)
            // to get at the private (which would become protected) members. Testing one of the Log* 
            // methods should be enough.

            CreateTraceLogger();
            try
            {
                _traceLogger.LogInfo("Informational test message");

                Assert.AreEqual(1, Trace.Listeners.Count);
                Assert.IsInstanceOfType(Trace.Listeners[0], typeof(DefaultTraceListener));
            }
            finally
            {
                _traceLogger.Dispose();
            }
        }
    }
}
