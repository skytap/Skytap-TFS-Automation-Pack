// 
// Retry.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 

using System;
using System.Collections.Generic;
using System.Threading;

namespace Skytap.Utilities
{
    /// <summary>
    /// Allows for retry logic when this class is wrapped around an API. 
    /// </summary>
    /// <remarks>
    /// The <seealso cref="Execute"/> method is the key to usage of this class. A sample usage would be:
    /// <code>
    /// var responseContent = Retry.Execute(() => SendRequest(request));
    /// </code>
    /// If that call fails (throws an exception), then the timeout specified to this class will elapse and 
    /// the call will be repeated up to the specified number of retries. 
    /// </remarks>
    public static class Retry
    {
        /// <summary>
        /// The default number of retries to use when this class is leveraged for retry operations.
        /// </summary>
        public const int DefaultNumRetries = 5;

        /// <summary>
        /// Default interval time used by callers that do not specify a specific interval time. 
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = new TimeSpan(0 /*hours*/, 5 /*minutes*/, 0 /*seconds*/);

        /// <summary>
        /// Number of retries that were completed for an <seealso cref="Execute"/> call. Used for test 
        /// verification purposes.
        /// </summary>
        public static int NumRetries { get; private set; }

        /// <summary>
        /// List of exceptions generated from the various retries. Used for test verification purposes.
        /// </summary>
        public static List<Exception> Exceptions { get; private set; }

        /// <summary>
        /// Total amount of time spent in retry delays. Used for test verification purposes.
        /// </summary>
        public static TimeSpan TotalRetryTime { get; private set; }

        /// <summary>
        /// See <seealso cref="Execute"/> overload for full details. This variant of the function is a helper
        /// for those delegates that do not need to return a value.
        /// </summary>
        /// <param name="action">The non-return type delegate to execute</param>
        /// <param name="retryCount">Number of times to retry the operation.</param>
        /// <param name="retryInterval">The amount of time to wait between retries. Note that this param
        /// is specified as Nullable (the '?' in the signature) to allow for a caller to not provide it
        /// and use the default.</param>
        public static void Execute(Action action, int retryCount = DefaultNumRetries, TimeSpan? retryInterval = null)
        {
            Execute<object>(() =>
                            {
                                action();
                                return null;
                            }, retryCount, retryInterval);
        }

        /// <summary>
        /// Execute a provided delegate with retry semantics based on parameters passed in.
        /// </summary>
        /// <typeparam name="T">The return value for the passed-in delegate to execute.</typeparam>
        /// <param name="action">The delegate to execute with retry logic</param>
        /// <param name="retryCount">Number of times to retry the operation.</param>
        /// <param name="retryInterval">The amount of time to wait between retries. Note that this param
        /// is specified as Nullable (the '?' in the signature) to allow for a caller to not provide it
        /// and use the default.</param>
        /// <returns>The return value from the delegate, or an exception thrown if the delegate failed.</returns>
        public static T Execute<T>(Func<T> action, int retryCount = DefaultNumRetries, TimeSpan? retryInterval = null)
        {
            Exceptions = new List<Exception>();
            NumRetries = 0;
            TotalRetryTime = new TimeSpan();

            // If no TimeSpan was provided for retry interval, assign a default.
            if (retryInterval == null)
            {
                retryInterval = DefaultTimeout;
            }

            for (var retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    return action();
                }
                catch (Exception e)
                {
                    Exceptions.Add(e);

                    // FUTURE: Disable logging here for now until we come up with a good way to not couple to 
                    // a specific set of logging classes (e.g. fire an event, return a string, etc.)
                    // logger.LogImportant(Resources.Retry_Execute_RetryingOperation, retryInterval.Value);

                    NumRetries++;
                    TotalRetryTime = TotalRetryTime.Add(retryInterval.Value);

                    Thread.Sleep(retryInterval.Value);
                }
            }

            throw new AggregateException(Exceptions);
        }
    }
}
