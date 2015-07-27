// 
// SkytapConfiguration.cs
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
using System.Text;

namespace Skytap.Cloud
{
    /// <summary>
    /// Represents a Skytap configuration.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class primarily serves as a property bag containing properties required to talk to 
    /// a Skytap configuration.
    /// </p>
    /// <p>
    /// This class is marked as serializable since the contents of it are stored to a file to save
    /// application state between runs.
    /// </p>
    /// </remarks>
    [Serializable]
    public class SkytapConfiguration
    {
        private const string DefaultConfigName = "Default";

        /// <summary>
        /// Name of the configuration (used for serialization and look-up purposes).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///  Network ID for the Skytap configuration. Used for creating and maintaining ICNR or VPN connections.
        /// </summary>
        public string ConfigurationNetworkId { get; set; }

        /// <summary>
        /// URL to the Skytap configuration. Used for creating and maintaining ICNR or VPN connections.
        /// </summary>
        public string ConfigurationUrl { get; set; }

        /// <summary>
        /// The ID of a VPN connection, if one was created, and empty if not.
        /// </summary>
        public string VpnId { get; set; }

        /// <summary>
        /// The ID of an ICNR connection, if one was created, and empty if not.
        /// </summary>
        public string IcnrId { get; set; }

        /// <summary>
        /// Path to the log file from a previous session so that a successive session can use the
        /// same file.
        /// </summary>
        public string LogFilePath { get; set; }

        /// <summary>
        /// SkytapConfiguration default constructor.
        /// </summary>
        public SkytapConfiguration()
        {
            Name = DefaultConfigName;
        }

        /// <summary>
        /// Generates a nicely-formatted string representation of all data fields. Useful for dumping state
        /// to a log file.
        /// </summary>
        /// <returns>String representation of the <seealso cref="SkytapConfiguration"/> object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Skytap Configuration Values:");
            builder.AppendLine(string.Format("\tName = {0}", Name));
            builder.AppendLine(string.Format("\tConfigurationNetworkId = {0}", ConfigurationNetworkId));
            builder.AppendLine(string.Format("\tConfigurationUrl = {0}", ConfigurationUrl));
            builder.AppendLine(string.Format("\tVpnId = {0}", VpnId));
            builder.AppendLine(string.Format("\tIcnrId = {0}", IcnrId));
            builder.AppendLine(string.Format("\tName = {0}", Name));

            return builder.ToString();
        }

    }
}
