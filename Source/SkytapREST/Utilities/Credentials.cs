// 
// Credentials.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 
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
