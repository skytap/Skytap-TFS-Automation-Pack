// 
// Logger.cs
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Skytap.Utilities
{
    /// <summary>
    /// Type of logging to take place (the target).
    /// </summary>
    public enum LoggerTypes
    {
        /// <summary>
        /// Null loggers can be useful during unit testing to simply pipe a log string to null.
        /// </summary>
        Null,

        /// <summary>
        /// Outputs log messages to the debug window.
        /// </summary>
        Debug,

        /// <summary>
        /// Outputs log messages to the console window.
        /// </summary>
        Console,

        /// <summary>
        /// Outputs log messages to a file whose name is provided by the caller.
        /// </summary>
        File,

        /// <summary>
        /// Instead of writing to a log target, simply calls a provided delegate so the caller
        /// can choose what to do with the log message.
        /// </summary>
        Delegate,

        /// <summary>
        /// Leverages System.Diagnostics.Trace and associated event listeners for logging.
        /// </summary>
        Trace
    }

    /// <summary>
    /// Level of logging that appears in a program log.
    /// </summary>
    /// <remarks>
    /// The highest (most important) log types should be earlier in the enum. Usage of the logger is 
    /// to set the level to the lowest type of logging desired, which will bring along everything 
    /// above it. For example, setting the level to Info logs everything, but setting to something 
    /// earlier in the enum, such as Error, will only log errors.
    /// </remarks>
    public enum LoggerLevels
    {
        /// <summary>
        /// Logs an error into the log file with special highlighting to indicate it is an error.
        /// </summary>
        Error,

        /// <summary>
        /// Logs the equivalent of a warning to the log file. Provides one extra level between error
        /// and info for filtering.
        /// </summary>
        Important,

        /// <summary>
        /// Most log file entries are informational, so this one is used generically and most frequently.
        /// </summary>
        Info
    }

    /// <summary>
    /// Container for some utilties used by the logging functionality. E.g. generate a log filename.
    /// </summary>
    public static class LoggerUtilities
    {
        /// <summary>
        /// Canned name for the start of a log file if one is not provided when the log class is created.
        /// </summary>
        public const string DefaultLogFilePrefix = "ApplicationLog";

        /// <summary>
        /// Generate a unique file for a log file that contains the date in the filename. The full path
        /// to the generated file is returned.
        /// </summary>
        /// <param name="logFilePrefix">A prefix to put on top of the log filename. If not provided, the
        /// prefix will be a standard "ApplicationLog" string.</param>
        /// <returns>The absolute path to the generated filename.</returns>
        public static string CreateUniqueLogFilename(string logFilePrefix = null)
        {
            if (string.IsNullOrEmpty(logFilePrefix))
            {
                logFilePrefix = DefaultLogFilePrefix;
            }

            var tempDir = Path.GetTempPath();
            Debug.Assert(Directory.Exists(tempDir));

            var logFilename = string.Format("{0}_{1}.log", logFilePrefix, DateTime.Now.ToString("yyyyMMdd_HH_mm_ss"));
            var logFilePath = Path.Combine(tempDir, logFilename);

            return logFilePath;
        }
    }

    /// <summary>
    /// Base class for all loggers. Provides the interface that all loggers must satisfy.
    /// </summary>
    /// <remarks>
    /// The Logger class and set of Logger subclasses basically implement a Strategy design pattern.
    /// </remarks>
    public abstract class Logger: IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// The <seealso cref="LoggerLevels"/> that specifies the log filter.
        /// </summary>
        public LoggerLevels Level { get; set; }

        /// <summary>
        /// Default constructor for Logger. This constructor is protected due to the class being abstract.
        /// </summary>
        protected Logger()
        {
            // By default, log everything.
            Level = LoggerLevels.Info;
            _disposed = false;
        }

        /// <summary>
        /// Finalizer for Logger class; ensures class-owned resources are disposed of appropriately.
        /// </summary>
        ~Logger()
        {
            Dispose(false);
        }

        /// <summary>
        /// Call when logging is complete to clean up any resources (like a file handle).
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Does the real work of Dispose - cleaning up any resources owned by the logger. This
        /// class should be overridden in a subclass to provide real functionality.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // For thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (!_disposed)
            {
                Flush();

                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }

        /// <summary>
        /// Flushes the contents of the log to ensure, in the case of writing to disk, that all
        /// content is written appropriately. Not all subclasses override this.
        /// </summary>
        public virtual void Flush()
        {
            // By default, a logger Flush doesn't do anything unless overridden by a subclass
        }

        /// <summary>
        /// Logs the equivalent of a warning provided filter levels allow for it.
        /// </summary>
        /// <param name="message">Message to input to the log; can be formatted using FormatString
        /// parameters.</param>
        /// <param name="formatObjects">Format string parameters.</param>
        public void LogImportant(string message, params object[] formatObjects)
        {
            if (Level >= LoggerLevels.Important)
            {
                _LogImportant(GetMessagePrefix() + message, formatObjects);
            }
        }

        /// <summary>
        /// Logs informational messages provided filtering allows for it.
        /// </summary>
        /// <param name="message">Message to input to the log; can be formatted using FormatString
        /// parameters.</param>
        /// <param name="formatObjects">Format string parameters.</param>
        public void LogInfo(string message, params object[] formatObjects)
        {
            if (Level >= LoggerLevels.Info)
            {
                _LogInfo(GetMessagePrefix() + message, formatObjects);
            }
        }

        /// <summary>
        /// Logs any errors to the log file provided filter levels allow for it.
        /// </summary>
        /// <param name="message">Message to input to the log; can be formatted using FormatString
        /// parameters.</param>
        /// <param name="formatObjects">Format string parameters.</param>
        public void LogError(string message, params object[] formatObjects)
        {
            if (Level >= LoggerLevels.Error)
            {
                _LogError(GetMessagePrefix() + message, formatObjects);
            }
        }

        /// <summary>
        /// Reset the log file and start over. Not all subclasses will implement this.
        /// </summary>
        public virtual void Reset()
        {
        }

        // The methods below must be implemented by a subclass and provide the guts of the logic for the logging
        // functionality.

        /// <summary>
        /// Implemented by subclass. Called by this base class to have a subclass do the real work 
        /// of the logging. This technique follows the strategy pattern.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="formatObjects">FormatString-style parameters containing values to replace in the message.</param>
        // ReSharper disable once InconsistentNaming
        protected abstract void _LogImportant(string message, params object[] formatObjects);

        /// <summary>
        /// Implemented by subclass. Called by this base class to have a subclass do the real work 
        /// of the logging. This technique follows the strategy pattern.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="formatObjects">FormatString-style parameters containing values to replace in the message.</param>
        // ReSharper disable once InconsistentNaming
        protected abstract void _LogInfo(string message, params object[] formatObjects);

        /// <summary>
        /// Implemented by subclass. Called by this base class to have a subclass do the real work 
        /// of the logging. This technique follows the strategy pattern.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="formatObjects">FormatString-style parameters containing values to replace in the message.</param>
        // ReSharper disable once InconsistentNaming
        protected abstract void _LogError(string message, params object[] formatObjects);

        private string GetMessagePrefix()
        {
            return string.Format("[{0}] ", DateTime.Now.ToString("h:mm:ss tt"));
        }
    }


// Disable XML comment warnings for this file as commenting all the subclasses would be overkill.
#pragma warning disable 1591


    /// <summary>
    /// Logger that does nothing on log requests.
    /// </summary>
    /// <remarks>
    /// This class is useful for test purposes where no overhead of logging should be undertaken.
    /// </remarks>
    /// <remarks>This class excluded from code coverage as it is only used for test purposes.</remarks>
    [ExcludeFromCodeCoverage]
    public class NullLogger : Logger
    {
        protected override void _LogImportant(string message, params object[] formatObjects)
        {
        }

        protected override void _LogInfo(string message, params object[] formatObjects)
        {
        }

        protected override void _LogError(string message, params object[] formatObjects)
        {
        }
    }

    /// <summary>
    /// Calls a delegate specified by <seealso cref="LogMessageHandler"/> when messages are logged.
    /// </summary>
    /// <remarks>This class excluded from code coverage as it is not currently used in the solution.</remarks>
    [ExcludeFromCodeCoverage]
    public class DelegateLogger : Logger
    {
        public delegate void LogMessage(string message, LoggerLevels level);

        public LogMessage LogMessageHandler { get; set; }

        protected override void _LogImportant(string message, params object[] formatObjects)
        {
            Debug.Assert(LogMessageHandler != null);
            LogMessageHandler(string.Format("INFO*: " + message, formatObjects), LoggerLevels.Important);
        }

        protected override void _LogInfo(string message, params object[] formatObjects)
        {
            Debug.Assert(LogMessageHandler != null);
            LogMessageHandler(string.Format("INFO:  " + message, formatObjects), LoggerLevels.Info);
        }

        protected override void _LogError(string message, params object[] formatObjects)
        {
            Debug.Assert(LogMessageHandler != null);
            LogMessageHandler(string.Format("ERROR: " + message, formatObjects), LoggerLevels.Error);
        }
    }

    /// <summary>
    /// Logs messages to the debug window.
    /// </summary>
    /// <remarks>This class excluded from code coverage as it is not currently used in the solution.</remarks>
    [ExcludeFromCodeCoverage]
    public class DebugOutputLogger : Logger
    {
        protected override void _LogImportant(string message, params object[] formatObjects)
        {
            Debug.WriteLine("INFO*: " + message, formatObjects);
        }

        protected override void _LogInfo(string message, params object[] formatObjects)
        {
            Debug.WriteLine("INFO:  " + message, formatObjects);
        }

        protected override void _LogError(string message, params object[] formatObjects)
        {
            Debug.WriteLine("ERROR: " + message, formatObjects);
        }
    }

    /// <summary>
    /// Logs messages to the console window, with color as appropriate.
    /// </summary>
    /// <remarks>This class excluded from code coverage as it is not currently used in the solution.</remarks>
    [ExcludeFromCodeCoverage]
    public class ConsoleLogger : Logger
    {
        private const ConsoleColor ImportantColor = ConsoleColor.DarkYellow;
        private const ConsoleColor InfoColor = ConsoleColor.Blue;
        private const ConsoleColor ErrorColor = ConsoleColor.Red;

        protected override void _LogImportant(string message, params object[] formatObjects)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ImportantColor;
            Trace.WriteLine(string.Format(message, formatObjects), "INFO*");
            Console.ForegroundColor = currentColor;
        }

        protected override void _LogInfo(string message, params object[] formatObjects)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = InfoColor;
            Trace.WriteLine(string.Format(message, formatObjects), "INFO");
            Console.ForegroundColor = currentColor;
        }

        protected override void _LogError(string message, params object[] formatObjects)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ErrorColor;
            Trace.WriteLine(string.Format(message, formatObjects), "ERROR");
            Console.ForegroundColor = currentColor;
        }
    }

    /// <summary>
    /// Logs information directly to a file.
    /// </summary>
    /// <remarks>
    /// This class is designed with a StringBuilder. It will continually build up the log file 
    /// contents in memory until the threshold number of lines is reached. Once the threshold is
    /// reached, or the class is being destroyed, the contents are flushed to disk. Previously,
    /// this class was implemented with a straight write to disk, but that solution was MUCH 
    /// slower.
    /// </remarks>
    /// <remarks>This class excluded from code coverage as it is not currently used in the solution.</remarks>
    [ExcludeFromCodeCoverage]
    public class FileLogger : Logger
    {
        // Play with this number to find one that is the appropriate blend of memory usage and
        // performance. 100 is a reasonable default.
        private const int NumLinesBeforeFlush = 100;

        internal static readonly string LogFilename =
            Path.Combine(Environment.GetEnvironmentVariable("TEMP"), @"Skytap.log");

        private readonly StringBuilder _logFileContents = new StringBuilder(10000);
        private int _logFileNumLines;

        public FileLogger()
        {
            // Delete the previous log file if it exists; swallow errors as the deletion doesn't matter
            try
            {
                if (File.Exists(LogFilename))
                {
                    File.Delete(LogFilename);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("ERROR: Exception occurred while trying to delete log file.");
            }
        }

        /// <summary>
        /// Write out the contents of the log string to the file, flushing the contents of
        /// the in-memory string.
        /// </summary>
        public override void Flush()
        {
            using (var textWriter = File.AppendText(LogFilename))
            {
                textWriter.WriteLine(_logFileContents.ToString());
                _logFileContents.Clear();
                _logFileNumLines = 0;
            }
        }

        protected override void _LogImportant(string message, params object[] formatObjects)
        {
            _logFileContents.AppendLine(string.Format("INFO*: " + message, formatObjects));
            IncrementLinesAndCheckFlush();
        }

        protected override void _LogInfo(string message, params object[] formatObjects)
        {
            _logFileContents.AppendLine(string.Format("INFO: " + message, formatObjects));
            IncrementLinesAndCheckFlush();
        }

        protected override void _LogError(string message, params object[] formatObjects)
        {
            _logFileContents.AppendLine(string.Format("ERROR: " + message, formatObjects));
            IncrementLinesAndCheckFlush();
        }

        private void IncrementLinesAndCheckFlush()
        {
            // If the number of lines in the string exceeds the threshold, then flush the 
            // string to a file.
            _logFileNumLines++;
            if (_logFileNumLines >= NumLinesBeforeFlush)
            {
                Flush();
            }
        }
    }

    /// <summary>
    /// Logs information using event listeners and built in System.Diagnostics.Trace functionality.
    /// </summary>
    public sealed class TraceLogger : Logger
    {
        private bool _logInitialized;
        private FileStream _logFileStream;
        private string _logFilePrefix;

        public string LogFilePrefix
        {
            get { return _logFilePrefix; }
            set
            {
                _logFilePrefix = value; 
                Reset(); 
            }
        }

        public string LogFilePath { get; private set; }
        private TraceListener TraceListener { get; set; }

        /// <summary>
        /// Constructor for TraceLogger class.
        /// </summary>
        public TraceLogger()
        {
            LogFilePrefix = string.Empty;
            Reset();
        }

        /// <summary>
        /// Constructor for TraceLogger class.
        /// </summary>
        /// <remarks>
        /// This constructor allows for overriding the default type of TraceListener. This
        /// can be useful for test purposes when a log file need not be generated.
        /// </remarks>
        public TraceLogger(TraceListener listener)
            : this()
        {
            TraceListener = listener;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_logFileStream != null)
            {
                _logFileStream.Close();
                _logFileStream = null;
            }
        }

        /// <summary>
        /// Write out the contents of the log string to the file, flushing the contents of
        /// the in-memory string.
        /// </summary>
        public override void Flush()
        {
            Trace.Flush();
        }

        /// <summary>
        /// Resets the log file by flushing the Trace, closing any files, and creating a new log filename.
        /// </summary>
        /// <remarks>
        /// This method will likely not be frequently used outside of tests, but is provided to start
        /// a new session without closing down the instance.
        /// </remarks>
        public override void Reset()
        {
            LogFilePath = LoggerUtilities.CreateUniqueLogFilename(LogFilePrefix);

            Trace.Listeners.Clear();
            Trace.AutoFlush = true;

            _logInitialized = false;
        }

        protected override void _LogImportant(string message, params object[] formatObjects)
        {
            InitializeLogFile();
            Trace.TraceWarning(message, formatObjects);
        }

        protected override void _LogInfo(string message, params object[] formatObjects)
        {
            InitializeLogFile();
            Trace.TraceInformation(message, formatObjects);
        }

        protected override void _LogError(string message, params object[] formatObjects)
        {
            InitializeLogFile();
            Trace.TraceError(message, formatObjects);
        }

        private void InitializeLogFile()
        {
            if (!_logInitialized)
            {
                // By default, create a log file on the file system, unless the caller wants to
                // override with a different type of trace listener.
                if (TraceListener == null ||
                    TraceListener is TextWriterTraceListener)
                {
                    _logFileStream = File.Open(LogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                    Trace.Listeners.Add(TraceListener ?? new TextWriterTraceListener(_logFileStream));
                }
                else
                {
                    Trace.Listeners.Add(TraceListener);
                }

                _logInitialized = true;
            }
        }
    }


// Restore XML comment warning after subclass definitions
#pragma warning restore 1591


    /// <summary>
    ///  Factory class used to create specific types of loggers.
    /// </summary>
    public static class LoggerFactory
    {
        /// <summary>
        /// The default type of logger is a Null logger, which doesn't do anything.
        /// </summary>
        private static LoggerTypes _loggerType = LoggerTypes.Null;

        private static Logger _logger;

        /// <summary>
        /// Returns the type of logger that the factory last created.
        /// </summary>
        public static LoggerTypes LoggerType
        {
            get { return _loggerType; } 
            set
            {
                // Need to reset the logger pointer as we want a new instance created on
                // the next Logger request
                _loggerType = value;
                _logger = null;
            }
        }

        /// <summary>
        /// Returns a logger instance based on the type of logger that the factory was instructed to create.
        /// </summary>
        /// <returns>Instance of a logger corresponding to specified type.</returns>
        public static Logger GetLogger()
        {
            Logger logger = null;
            switch (_loggerType)
            {
                case LoggerTypes.Null:
                    if (_logger == null)
                    {
                        _logger = new NullLogger();
                    }
                    logger = _logger;
                    break;

                case LoggerTypes.Debug:
                    if (_logger == null)
                    {
                        _logger = new DebugOutputLogger();
                    }
                    logger = _logger;
                    break;

                case LoggerTypes.Console:
                    if (_logger == null)
                    {
                        _logger = new ConsoleLogger();
                    }
                    logger = _logger;
                    break;

                case LoggerTypes.Delegate:
                    if (_logger == null)
                    {
                        _logger = new DelegateLogger();
                    }
                    logger = _logger;
                    break;

                case LoggerTypes.File:
                    if (_logger == null)
                    {
                        _logger = new FileLogger();
                    }
                    logger = _logger;
                    break;

                case LoggerTypes.Trace:
                    if (_logger == null)
                    {
                        _logger = new TraceLogger();
                    }
                    logger = _logger;
                    break;

                default:
                    // Someone added a logger type but didn't update the factory to account for it.
                    Debug.Assert(false);
                    break;
            }

            return logger;
        }

        /// <summary>
        /// Resets the logger and creates a new one on next request.
        /// </summary>
        public static void Reset()
        {
            _logger = null;
            _loggerType = LoggerTypes.Null;
        }
    }
}
