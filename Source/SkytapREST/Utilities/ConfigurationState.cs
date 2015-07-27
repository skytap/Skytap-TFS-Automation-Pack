// 
// ConfigurationState.cs
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
using System.IO;
using System.Xml.Serialization;

namespace Skytap.Cloud.Utilities
{
    internal class ConfigurationState
    {
        private const string ConfigurationStateSubdir = "Skytap";
        private const string SerializedFileExtension = ".xml";

        /// <summary>
        /// Serialize a <seealso cref="SkytapConfiguration"/> object instance as an XML file. 
        /// </summary>
        /// <param name="config">Configuration object instance to persist.</param>
        /// <remarks>
        /// This method is helpful to save state between runs of the executable to do the appropriate
        /// cleanup after, say, a set of tests have finished running and this executable is called again.
        /// </remarks>
        /// <returns>
        /// Path to the serialized XML file.
        /// </returns>
        public string Serialize(SkytapConfiguration config)
        {
            var configStatePath = GeneratePath(config.Name);

            var serializer = new XmlSerializer(typeof(SkytapConfiguration));
            TextWriter writer = null;

            try
            {
                writer = new StreamWriter(configStatePath);
                serializer.Serialize(writer, config);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
            return configStatePath;
        }

        /// <summary>
        /// Deserialize a <seealso cref="SkytapConfiguration"/> object instance from an XML file. 
        /// </summary>
        /// <param name="configName">Name of the Skytap configuration. This value is used to generate
        /// the filename containing the configuration state.</param>
        /// <remarks>
        /// This method is helpful to save state between runs of the executable to do the appropriate
        /// cleanup after, say, a set of tests have finished running and this executable is called again.
        /// </remarks>
        public SkytapConfiguration Deserialize(string configName)
        {
            SkytapConfiguration config;
            var serializer = new XmlSerializer(typeof(SkytapConfiguration));
            FileStream stream = null;
            try
            {
                stream = new FileStream(GeneratePath(configName), FileMode.Open);
                config = (SkytapConfiguration) serializer.Deserialize(stream);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return config;
        }

        protected virtual string GeneratePath(string configName)
        {
            Debug.Assert(!string.IsNullOrEmpty(configName));

            var configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigurationStateSubdir);
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            return Path.Combine(configDir, configName + SerializedFileExtension);
        }
    }
}
