// 
// Constants.cs
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
 
 namespace Skytap.Cloud
{
    internal abstract class Arguments
    {
        public const string Username = "username";
        public const string Password = "password";
        public const string ConfigName = "configname";
        public const string ConfigId = "configid";
        public const string SaveTemplate = "savetemplate";
        public const string TemplateId = "templateid";
        public const string VpnId = "vpnid";
    }

    internal static class ConfigurationStates
    {
        public const string Busy = "busy";
        public const string Halted = "halted";
        public const string Running = "running";
        public const string Stopped = "stopped";
        public const string Suspended = "suspended";
    }

    internal static class CommandResults
    {
        public const int Success = 0;
        public const int Fail = 1;
    }
}
