// 
// Program.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Skytap.Cloud.Properties;
using Skytap.Utilities;

namespace Skytap.Cloud
{
    /// <summary>
    /// Class representing main entry point into the application. Keep a minimum
    /// of logic here.
    /// </summary>
    public class Program
    {
        private const string LogFilePrefix = "SkytapCLI";

        private static Logger _logger;

        /// <summary>
        /// Main entry point to the SktapCLI application.
        /// </summary>
        /// <param name="args">Command-line arguments that were specified.</param>
        /// <returns>0 if the program succeeded, a value other than 0 if it failed.</returns>
        public static int Main(string[] args)
        {
            const int resultError = -1;

            InitializeLogFile();

            _logger.LogInfo("Starting new run...");

            try
            {
                _logger.LogInfo("Application configuration parameters:\n\n{0}", SkytapApi.ConfigParams.ToString());

                var inputs = new ProgramArguments(args);
                var commands = new Dictionary<string, ICommand>();

                _logger.LogInfo("EXE command-line: {0}", inputs.ToString());

                /* Populate Command Dictionary */
                var types = from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.GetInterfaces().Contains(typeof(ICommand)) && t.GetConstructor(Type.EmptyTypes) != null
                            select t;

                foreach (var type in types)
                {
                    var commandInstance = Activator.CreateInstance(type) as ICommand;
                    Debug.Assert(commandInstance != null);

                    commands.Add(commandInstance.Name.ToLowerInvariant(), commandInstance);
                }

                /* Validate action is valid, also that we have a username and password */
                if (!HasNeededParams(inputs, commands))
                {
                    PrintUsage(commands.Values.ToArray());

                    _logger.LogError(Resources.Program_Main_ERROR_DidNotSpecifyAllProgramArguments, inputs.ToString());

                    return resultError;
                }

                /* Validate the action requested has the arguments it needs */
                ICommand command = commands[inputs.ArgumentMap["action"]];

                if (!command.ValidateArgs(inputs.ArgumentMap))
                {
                    PrintUsage(new[] { command });

                    _logger.LogError(Resources.Program_Main_ERROR_InvalidCommandLineArgs, inputs.ToString());

                    return resultError;
                }

                /* All looks good, lets run it */
                return command.Invoke(inputs.ArgumentMap);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception thrown: {0}\n", e.Message);
                _logger.LogError("Stack Trace: {0}\n", e.StackTrace);
                _logger.LogError("Arguments: {0}\n", String.Join(" ", args));
                
                return resultError;
            }
            finally
            {
                ShutdownLogFile();
            }
        }

        private static void InitializeLogFile()
        {
            // Choose a TRACE logger for flexibility in logging items to a file or other configured
            // event writer.
            LoggerFactory.LoggerType = LoggerTypes.Trace;

            _logger = LoggerFactory.GetLogger();
            Debug.Assert(_logger != null);
            Debug.Assert(_logger is TraceLogger);

            ((TraceLogger) (_logger)).LogFilePrefix = LogFilePrefix;
        }

        private static void ShutdownLogFile()
        {
            _logger.Dispose();
        }

        private static bool HasNeededParams(ProgramArguments inputs, Dictionary<String, ICommand> commands)
        {
            return (inputs.ArgumentMap.ContainsKey("action") && inputs.ArgumentMap.ContainsKey("username") &&
                    inputs.ArgumentMap.ContainsKey("password") && commands.Keys.Contains(inputs.ArgumentMap["action"].ToLowerInvariant()) &&
                    !String.IsNullOrEmpty(inputs.ArgumentMap["username"]) && !String.IsNullOrEmpty(inputs.ArgumentMap["password"]));
        }

        static void PrintUsage(ICommand[] commands)
        {
            Console.WriteLine(Resources.Program_Main_PrintUsage_UsageString);

            if (commands.Length > 1)
            {
                Console.WriteLine(Resources.Program_Main_PrintUsage_UsageAvailableActions);
            }

            foreach (var command in commands)
            {
                Console.WriteLine(Resources.Program_Main_PrintUsage_Action, command.Name.ToLowerInvariant(), command.Help);
            }
        }
    }
}
