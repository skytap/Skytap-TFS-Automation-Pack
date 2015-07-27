﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Skytap.Cloud.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Skytap.Cloud.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ERROR: incorrect command-line arguments specified --&gt; {0}.
        /// </summary>
        internal static string Program_Main_ERROR_DidNotSpecifyAllProgramArguments {
            get {
                return ResourceManager.GetString("Program_Main_ERROR_DidNotSpecifyAllProgramArguments", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ERROR: Invalid command-line arguments --&gt; {0}.
        /// </summary>
        internal static string Program_Main_ERROR_InvalidCommandLineArgs {
            get {
                return ResourceManager.GetString("Program_Main_ERROR_InvalidCommandLineArgs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Action: {0}
        ///Args: 
        ///	{1}
        ///.
        /// </summary>
        internal static string Program_Main_PrintUsage_Action {
            get {
                return ResourceManager.GetString("Program_Main_PrintUsage_Action", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Available Actions: 
        ///.
        /// </summary>
        internal static string Program_Main_PrintUsage_UsageAvailableActions {
            get {
                return ResourceManager.GetString("Program_Main_PrintUsage_UsageAvailableActions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage: 
        ///
        ///  skytapcli  /action &lt;action&gt; /username &lt;username&gt; /password &lt;password/api key&gt; [/&lt;argName&gt; &lt;argValue&gt; ...]
        ///.
        /// </summary>
        internal static string Program_Main_PrintUsage_UsageString {
            get {
                return ResourceManager.GetString("Program_Main_PrintUsage_UsageString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Retrying operation in {0}.
        /// </summary>
        internal static string Retry_Execute_RetryingOperation {
            get {
                return ResourceManager.GetString("Retry_Execute_RetryingOperation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ERROR: Skytap: Configuration ({0}) never reached state ({1}) before timeout..
        /// </summary>
        internal static string SkytapApi_CheckConfigurationForDesiredState_ERROR_UnexpectedState {
            get {
                return ResourceManager.GetString("SkytapApi_CheckConfigurationForDesiredState_ERROR_UnexpectedState", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ERROR: Unexpected HTTP response code {0}.
        /// </summary>
        internal static string SkytapApi_SendSkytapHttpRequest_ERROR__UnexpectedHttpStatusCode {
            get {
                return ResourceManager.GetString("SkytapApi_SendSkytapHttpRequest_ERROR__UnexpectedHttpStatusCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Internal Error: No web request provided for Skytap API call..
        /// </summary>
        internal static string SkytapAPI_SendSkytapHttpRequest_WebRequestNull {
            get {
                return ResourceManager.GetString("SkytapAPI_SendSkytapHttpRequest_WebRequestNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to configid: If TFS is being hosted in Skytap, specify its configid
        ///	vpnid: If TFS is on-premise specify a VPN id
        ///	templateid: templateid of the target test system
        ///	configname: name to give to the new configuration.
        /// </summary>
        internal static string TfsShutdown_HelpConfigId {
            get {
                return ResourceManager.GetString("TfsShutdown_HelpConfigId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to savetemplate: (true/false) If set to true, save the configuration as a template 
        ///	configname: Name of configuration to shutdown .
        /// </summary>
        internal static string TfsShutdown_HelpSaveTemplate {
            get {
                return ResourceManager.GetString("TfsShutdown_HelpSaveTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unexpected configuration state reversion to &apos;Suspended&apos;. Will retry start operation again until threshold reached..
        /// </summary>
        internal static string TfsStartup_UnexpectedReversionToSuspended {
            get {
                return ResourceManager.GetString("TfsStartup_UnexpectedReversionToSuspended", resourceCulture);
            }
        }
    }
}
