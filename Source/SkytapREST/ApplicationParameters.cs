// 
// ApplicationParameters.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 

using System;
using System.Configuration;
using System.Linq;

namespace Skytap.Utilities
{
    /// <summary>
    /// Wrapper for the various parameters that control the Skytap plug-in execution. Example values include
    /// number of retries for an HTTP operation, amount of time to wait between retries, HTTP timeout values, 
    /// and the Skytap host URL used for API calls, among others.
    /// </summary>
    public class ApplicationParameters
    {
        // Parameter names for name/value pairs in the application configuration file (i.e. AppSettings)
        internal const string ParamNumRetries = "RetryNumRetries";
        internal const string ParamWaitTime = "RetryWaitTime";
        internal const string ParamHttpTimeout = "HttpTimeout";
        internal const string ParamSkytapHostUrl = "SkytapHostUrl";

        // Default values for the configuration parameters. Values in the configuration file will always
        // override these.
        internal const int DefaultNumRetries = 5;
        internal const int DefaultHttpTimeout = 300000; // 5 min in ms
        internal const string DefaultSkytapHostUrl = "https://cloud.skytap.com";

        /// <summary>
        /// Default interval time used by callers that do not specify a specific interval time. 
        /// </summary>
        internal static readonly TimeSpan DefaultRetryWaitTime = new TimeSpan(0 /*hours*/, 5 /*minutes*/, 0 /*seconds*/);

        /// <summary>
        /// Timeout to use on HTTP requests, in milliseconds.
        /// </summary>
        public int HttpTimeout { get; private set; }

        /// <summary>
        /// Number of times an API call should be retried when it is invokved with retry semantics.
        /// </summary>
        public int RetryNumRetries { get; private set; }

        /// <summary>
        /// Amount of time to wait between retries of an API call.
        /// </summary>
        public TimeSpan RetryWaitTime { get; private set; }

        /// <summary>
        /// Base URL used to communicate with Skytap services.
        /// </summary>
        public string SkytapHostUrl { get; private set; }

        /// <summary>
        /// Default constructor. Initializes the parameters either from constant defaults or 
        /// the application configuration file, which takes precedence.
        /// </summary>
        public ApplicationParameters()
        {
            Refresh();
        }

        /// <summary>
        /// Re-read the configuration parameters out of the application configuration file.
        /// </summary>
        public void Refresh()
        {
            RefreshHttpTimeout();
            RefreshNumRetries();
            RefreshSkytapHostUrl();
            RefreshWaitTime();
        }

        /// <summary>
        /// Produces a string representation of all the application parameters managed by the
        /// <seealso cref="ApplicationParameters"/> class.
        /// </summary>
        /// <returns>All parameter values in a nicely formatted string.</returns>
        public override string ToString()
        {
            return
                string.Format(
                    "HTTP Timeout    = {0} ms\n" + 
                    "Num Retries     = {1}\n" + 
                    "Retry Wait Time = {2}\n" +
                    "Skytap Host URL = {3}\n", 
                    HttpTimeout, RetryNumRetries, RetryWaitTime.ToString("g"), SkytapHostUrl);
        }

        private void RefreshHttpTimeout()
        {
            HttpTimeout = DefaultHttpTimeout;
            if (ConfigurationManager.AppSettings.AllKeys.Contains(ParamHttpTimeout))
            {
                var httpTimeout = ConfigurationManager.AppSettings[ParamHttpTimeout];
                if (!string.IsNullOrEmpty(httpTimeout))
                {
                    HttpTimeout = Convert.ToInt32(ConfigurationManager.AppSettings[ParamHttpTimeout]);
                }
            }
        }

        private void RefreshNumRetries()
        {
            RetryNumRetries = DefaultNumRetries;
            if (ConfigurationManager.AppSettings.AllKeys.Contains(ParamNumRetries))
            {
                var numRetries = ConfigurationManager.AppSettings[ParamNumRetries];
                if (!string.IsNullOrEmpty(numRetries))
                {
                    RetryNumRetries = Convert.ToInt32(ConfigurationManager.AppSettings[ParamNumRetries]);
                }
            }
        }

        private void RefreshSkytapHostUrl()
        {
            SkytapHostUrl = DefaultSkytapHostUrl;
            if (ConfigurationManager.AppSettings.AllKeys.Contains(ParamSkytapHostUrl))
            {
                var hostUrl = ConfigurationManager.AppSettings[ParamSkytapHostUrl];
                if (!string.IsNullOrEmpty(hostUrl))
                {
                    SkytapHostUrl = hostUrl;
                }
            }
        }

        private void RefreshWaitTime()
        {
            RetryWaitTime = DefaultRetryWaitTime;
            if (ConfigurationManager.AppSettings.AllKeys.Contains(ParamWaitTime))
            {
                var waitTimeString = ConfigurationManager.AppSettings[ParamWaitTime];
                if (!string.IsNullOrEmpty(waitTimeString))
                {
                    TimeSpan waitTimeSpan;

                    // NOTE: Setting should be specified in this format: hh:mm:ss
                    if (TimeSpan.TryParse(waitTimeString, out waitTimeSpan))
                    {
                        RetryWaitTime = waitTimeSpan;
                    }
                }
            }
        }

    }
}
