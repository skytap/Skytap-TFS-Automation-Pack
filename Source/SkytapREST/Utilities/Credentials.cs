// 
// Credentials.cs
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
 
 namespace Skytap.Cloud.Utilities
{
    /// <summary>
    /// Wrapper around a user's name and password/key combination.
    /// </summary>
    public struct Credentials
    {
        /// <summary>
        /// User name that identifies the credentials.
        /// </summary>
        public string Username;

        /// <summary>
        /// The Key (or password) associated with the username to create the Credentials set.
        /// </summary>
        public string Key;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="username">User's name.</param>
        /// <param name="key">User's key or password</param>
        public Credentials(string username, string key)
        {
            Username = username;
            Key = key;
        }
    }
}
