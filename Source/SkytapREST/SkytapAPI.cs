// 
// SkytapAPI.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using Skytap.Cloud.Properties;
using Skytap.Cloud.Utilities;
using Skytap.Utilities;

namespace Skytap.Cloud
{
    /// <summary>
    /// Wrapper class for calling the various Skytap REST APIs.
    /// </summary>
    public static class SkytapApi
    {
        /// <summary>
        /// The WebRequestMethods.Http does not contain a DELETE definition, so one is hard-coded here.
        /// </summary>
        public const string HttpDeleteRequest = "DELETE";

        private const string HttpApiXmlMimeType = "application/xml";
        private const string HttpApiJsonMimeType = "application/json";

        private static readonly ApplicationParameters _configParams = new ApplicationParameters();

        /// <summary>
        /// Read-only view of the current set of <seealso cref="ApplicationParameters"/> values.
        /// </summary>
        public static ApplicationParameters ConfigParams {
            get { return _configParams; }
        }

        /// <summary>
        /// Get a global list of VPNs accessible by the user
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <returns>XML response content for the request.</returns>
        public static string GetVpns(Credentials credentials)
        {
            var logger = LoggerFactory.GetLogger(); 
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);

            var request = CreateSkytapWebRequest(credentials, _configParams.SkytapHostUrl + "/vpns");
            request.Method = WebRequestMethods.Http.Get;

            var responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);

            logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);

            return responseContent;
        }

        /// <summary>
        /// Creates an ICNR connection between a source and target network.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="sourceNetId">Source Skytap network ID that the connection starts from.</param>
        /// <param name="targetNetId">Target Skytap network ID that the connection goes to.</param>
        /// <returns>String ID of the new connection if successful.</returns>
        public static string CreateIcnrConnection(Credentials credentials, string sourceNetId, string targetNetId)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);
            logger.LogInfo("source NetId: " + sourceNetId + "target Net ID: " + targetNetId + "\n");

            var url = string.Format("{0}/tunnels?source_network_id={1}&target_network_id={2}", 
                                    _configParams.SkytapHostUrl, sourceNetId, targetNetId);
            logger.LogInfo("Request URL = {0}", url);

            var request = CreateSkytapWebRequest(credentials, url);
            request.Method = WebRequestMethods.Http.Post;
            var responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);

            logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);

            // Extract the ICNR ID from the response XML
            var xmldoc = new XmlDocument();
            xmldoc.LoadXml(responseContent);

            var icnrIdXmlNode = xmldoc.SelectSingleNode("//tunnel/id");
            Debug.Assert(icnrIdXmlNode != null);

            return icnrIdXmlNode.InnerText;
        }

        /// <summary>
        /// Deletes a specified ICNR connection.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="icnrId">ID of the ICNR connection to remove.</param>
        public static void DeleteIcnrConnection(Credentials credentials, string icnrId)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);
            logger.LogInfo("Deleting ICNR ID = {0}", icnrId);

            var url = string.Format("{0}/tunnels/{1}", _configParams.SkytapHostUrl, icnrId);
            logger.LogInfo("Request URL = {0}", url);

            var request = CreateSkytapWebRequest(credentials, url);
            request.Method = HttpDeleteRequest;
            var responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);

            logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);
        }

        /// <summary>
        /// Attach an existing VPN to a configuration's network.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="configUrl">URL to the Skytap configuration that is associated with the VPN.</param>
        /// <param name="networkId">ID of the network to attach the VPN to.</param>
        /// <param name="vpnId">VPN ID to attach.</param>
        public static void AttachVpnConnection(Credentials credentials, string configUrl, string networkId, string vpnId)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);
            logger.LogInfo("Attaching VPN on Network ID = {0} using VPN ID = {1}", networkId, vpnId);

            var url = string.Format("{0}/networks/{1}/vpns", configUrl, networkId);
            var rBody = "{\"vpn_id\":\"" + vpnId + "\"}";

            logger.LogInfo("Request: URL: {0}\n\tBody: {1}\n", url, rBody);

            var request = (HttpWebRequest) WebRequest.Create(url);

            var usernamekey = string.Format("{0}:{1}", credentials.Username, credentials.Key);

            request.Timeout = _configParams.HttpTimeout;
            request.ReadWriteTimeout = _configParams.HttpTimeout;
            request.ContentType = HttpApiJsonMimeType;
            request.Accept = HttpApiJsonMimeType;
            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernamekey));
            request.Method = WebRequestMethods.Http.Post;

            // ReSharper disable once AccessToStaticMemberViaDerivedType
            byte[] rbPostData = UTF8Encoding.UTF8.GetBytes(rBody);
            request.ContentLength = rbPostData.Length;

            var dataStream = request.GetRequestStream();
            dataStream.Write(rbPostData, 0, rbPostData.Length);
            dataStream.Close();
            var responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);

            logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);
        }

        /// <summary>
        /// After attaching, connect the VPN to the specified configuration and network.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="configUrl">URL to the Skytap configuration that is associated with the VPN.</param>
        /// <param name="networkId">ID of the network to attach the VPN to.</param>
        /// <param name="vpnId">VPN ID to attach.</param>
        public static void ConnectVpn(Credentials credentials, string configUrl, string networkId, string vpnId)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);
            logger.LogInfo("Connecting VPN on Network ID = {0} using VPN ID = {1}", networkId, vpnId);

            var url = string.Format("{0}/networks/{1}/vpns/{2}.json", configUrl, networkId, vpnId);
            logger.LogInfo("Request URL = {0}", url);

            HttpWebRequest request = CreateSkytapWebRequest(credentials, url);
            request.Method = WebRequestMethods.Http.Put;
            request.ContentType = HttpApiJsonMimeType;
            request.Accept = HttpApiJsonMimeType;

            // ReSharper disable once AccessToStaticMemberViaDerivedType
            byte[] rbPostData = UTF8Encoding.UTF8.GetBytes("{ \"connected\": true }");
            request.ContentLength = rbPostData.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(rbPostData, 0, rbPostData.Length);
            dataStream.Close();
            var responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);

            logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);
        }

        /// <summary>
        /// Disconnect a VPN connection. This should be done prior to detaching a VPN.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="configUrl">URL to the Skytap configuration that is associated with the VPN.</param>
        /// <param name="networkId">ID of the network containing the VPN to disconnect.</param>
        /// <param name="vpnId">VPN ID to disconnect.</param>
        public static void DisconnectVpn(Credentials credentials, string configUrl, string networkId, string vpnId)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);
            logger.LogInfo("Disconnecting VPN on Network ID = {0} using VPN ID = {1}", networkId, vpnId);

            var url = string.Format("{0}/networks/{1}/vpns/{2}?connected=false", configUrl, networkId, vpnId);
            logger.LogInfo("Request URL = {0}", url);

            var request = CreateSkytapWebRequest(credentials, url);
            request.Method = WebRequestMethods.Http.Put;
            var responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);

            logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);
        }

        /// <summary>
        /// Detach a VPN connection. A VPN connection should be disconnected prior to detaching.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="configUrl">URL to the Skytap configuration that is associated with the VPN.</param>
        /// <param name="networkId">ID of the network containing the VPN to detach.</param>
        /// <param name="vpnId">VPN ID to detach.</param>
        public static void DetachVpn(Credentials credentials, string configUrl, string networkId, string vpnId)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);
            logger.LogInfo("Detaching VPN on Network ID = {0} using VPN ID = {1}", networkId, vpnId);

            var url = string.Format("{0}/networks/{1}/vpns/{2}", configUrl, networkId, vpnId);
            logger.LogInfo("Request URL = {0}", url);

            var request = CreateSkytapWebRequest(credentials, url);
            request.Method = HttpDeleteRequest;
            var responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);

            logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);
        }

        /// <summary>
        /// Change the state of a configuration to a specified value, such as Suspended, Stopped, or Running.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="configUrl">Configuration whose state is to be modified.</param>
        /// <param name="state">State to transition to, such as Suspended, Stopped, or Running.</param>
        public static void SetConfigurationState(Credentials credentials, string configUrl, string state)
        {
            Debug.Assert(!string.IsNullOrEmpty(credentials.Username));
            Debug.Assert(!string.IsNullOrEmpty(credentials.Key));
            Debug.Assert(!string.IsNullOrEmpty(configUrl));
            Debug.Assert(!string.IsNullOrEmpty(state));

            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);
            logger.LogInfo("Setting configuration state to {0}", state);

            var url = string.Format("{0}?runstate={1}", configUrl, state);
            logger.LogInfo("Request URL = {0}", url);
            
            var request = CreateSkytapWebRequest(credentials, url);

            request.Method = WebRequestMethods.Http.Put;
            var responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);

            logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);
        }

        /// <summary>
        /// Create a new Skytap configuration based on the specified template and provide an optional name.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="templateId">ID of the template to base the new configuration on.</param>
        /// <param name="configName">Optional name of the new configuration.</param>
        /// <returns></returns>
        public static SkytapConfiguration CreateConfiguration(Credentials credentials, string templateId, string configName = null)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);
            logger.LogInfo("Creating configuration based on template ID = {0}", templateId);

            var newSkytapConfiguration = new SkytapConfiguration();

            var url = _configParams.SkytapHostUrl + "/configurations/";
            logger.LogInfo("Request URL = {0}", url);

            var request = CreateSkytapWebRequest(credentials, url);
            request.Method = WebRequestMethods.Http.Post;

            // FUTURE: This likely can be simplified to include "?template_id=<id>" on the URL instead of this.
            // Do this once good automated tests are in place to validate.
            var webRequestContent = string.Format("<template_id>{0}</template_id>", templateId);
            byte[] rbPostData = Encoding.UTF8.GetBytes(webRequestContent);
            request.ContentLength = rbPostData.Length;

            var dataStream = request.GetRequestStream();
            dataStream.Write(rbPostData, 0, rbPostData.Length);
            dataStream.Close();

            var responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);
            logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);

            var xmldoc = new XmlDocument();
            xmldoc.LoadXml(responseContent);

            var configUrlXmlNode = xmldoc.SelectSingleNode("/configuration/url");
            Debug.Assert(configUrlXmlNode != null);
            newSkytapConfiguration.ConfigurationUrl = configUrlXmlNode.InnerText;

            var configNetworkIdNode = xmldoc.SelectSingleNode("//networks/network/id");
            Debug.Assert(configNetworkIdNode != null);
            newSkytapConfiguration.ConfigurationNetworkId = configNetworkIdNode.InnerText;

            // Change the name of Configuration if a name has been provided
            if (configName != null)
            {
                newSkytapConfiguration.Name = configName;
                UpdateConfigurationName(credentials, newSkytapConfiguration.ConfigurationUrl, configName);
            }

            return newSkytapConfiguration;
        }

        /// <summary>
        /// Change the name of a Skytap configuration.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="configurationUrl">The configuration identifier to change</param>
        /// <param name="newName">New name of the configuration (basic string; not sure of length restrictions)</param>
        public static void UpdateConfigurationName(Credentials credentials, string configurationUrl, string newName)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);
            logger.LogInfo("Updating configuration name to = {0}", newName);

            var url = HttpUtility.HtmlEncode(configurationUrl + "?name=" + newName);
            logger.LogInfo("Request URL = {0}", url);

            var request = CreateSkytapWebRequest(credentials, url);

            request.Method = WebRequestMethods.Http.Put;

            var responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);

            logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);
        }

        /// <summary>
        /// Get the network ID associated with a Skytap configuration, which is useful for other companion calls
        /// that operate on a network.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="configId">The configuration identifier to query.</param>
        /// <returns>An ID of the attached network in the configuration.</returns>
        public static string GetNetworkIdInConfiguration(Credentials credentials, string configId)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);

            var url = string.Format("{0}/configurations/{1}", _configParams.SkytapHostUrl, configId);
            logger.LogInfo("Request URL = {0}", url);

            var request = CreateSkytapWebRequest(credentials, url);
            request.Method = WebRequestMethods.Http.Get;
            var responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);
            logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);

            var xmldoc = new XmlDocument();
            xmldoc.LoadXml(responseContent);

            var configNetIdXmlNode = xmldoc.SelectSingleNode("//networks/network/id");
            Debug.Assert(configNetIdXmlNode != null);

            var configNetId = configNetIdXmlNode.InnerText;
            logger.LogInfo("DEBUG: Network ID got " + configNetId + "\n");

            return configNetId;
        }

        /// <summary>
        /// Save the specified configuration as a new Skytap template.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="testTargetConfigUrl">URL of the configuration to turn into a template.</param>
        public static void SaveAsSkytapTemplate(Credentials credentials, string testTargetConfigUrl)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);

            // Grab the configID from the configUrl
            string[] parts = testTargetConfigUrl.Split('/');
            var configId = parts[parts.Length - 1];
            logger.LogInfo("Saving target test configuration as a Skytap template. ConfigurationID = {0}", configId);

            var url = new StringBuilder();
            url.Append(_configParams.SkytapHostUrl + "/templates/");
            logger.LogInfo("Request URL = {0}", url);

            HttpWebRequest request = CreateSkytapWebRequest(credentials, url.ToString());

            request.Method = WebRequestMethods.Http.Post;

            var sbContent = new StringBuilder();
            sbContent.Append("<configuration_id>");
            sbContent.Append(configId);
            sbContent.Append("</configuration_id>");

            string postData = sbContent.ToString();
            byte[] rbPostData = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = rbPostData.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(rbPostData, 0, rbPostData.Length);
            dataStream.Close();

            var responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);

            logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);
        }

        /// <summary>
        /// Deletes a Skytap configuration, which is useful when a configuration was created for temporary use.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="configUrlToDelete">URL of the configuration to be shutdown/deleted.</param>
        public static void ShutDownConfiguration(Credentials credentials, string configUrlToDelete)
        {
            var shutdownRequestDelayTime = new TimeSpan(0, 0, 10 /* sec */);

            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);
            logger.LogInfo("Request URL = {0}", configUrlToDelete);

            var request = CreateSkytapWebRequest(credentials, configUrlToDelete);
            request.Method = HttpDeleteRequest;

            // HACKHACK! There is an issue when running the plug-in in a TFS build where a response is never received
            // during the shutdown of the configuration. A root cause has not yet been identified. In lieu of a 
            // proper fix, the following is done:
            //   (a) override the HTTP timeout to be a shorter value so that a frequent check of deletion can be made
            //   (b) if a timeout occurs, check if the configuration still exists in a separate request and if it 
            //       still exists, try the whole operation again

            // These overrides are a one-time thing for the hack above. Generally, use the timeouts defined in 
            // the method that creates the request.
            request.Timeout = 5000; // ms
            request.ReadWriteTimeout = request.Timeout;

            // NOTE that the delay time between retries is also adjusted as part of the hack. Wait a few seconds
            // instead of minutes.
            var responseContent = Retry.Execute(() => 
                {
                    try
                    {
                        var responseString = SendSkytapHttpRequest(request);

                        return responseString;
                    }
                    catch (WebException)
                    {
                        // In the case of an HTTP error, check if the configuration exists. If it does not, simply continue. 
                        // Otherwise, re-throw the error. Note that the call to GetConfiguration avoids the retry operations
                        // since this whole operation will be retried as necessary (think of the shutdown and get as one
                        // transaction).
                        try
                        {
                            logger.LogInfo("Attempting to retrieve the configuration {0} to determine if it was deleted.", configUrlToDelete);
                            GetConfiguration(credentials, configUrlToDelete, false);
                        }
                        catch (WebException e)
                        {
                            // If a 404 was returned for the retrieval of the configuration,
                            // assume it was shut down and deleted and that we are done.
                            // Otherwise, need to rethrow the exception so that another
                            // retry loop is triggered.
                            if (e.Status == WebExceptionStatus.ProtocolError &&
                                ((HttpWebResponse) e.Response).StatusCode == HttpStatusCode.NotFound)
                            {
                                logger.LogInfo("404 status code received from GetConfiguration; assuming configuration is shut down");
                            }
                            else
                            {
                                logger.LogInfo("Non-404 status code received from GetConfiguration; attempting shutdown again");
                                throw;
                            }
                        }
                    }
                    return null;
                }, 
                _configParams.RetryNumRetries, shutdownRequestDelayTime);

            if (responseContent != null)
            {
                logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);
            }
            else
            {
                logger.LogInfo("Shutdown complete, but no response received.");
            }
        }

        /// <summary>
        /// Retrieves the XML that describes the specified Skytap configuration.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="configurationUrl">URL of the configuration to query.</param>
        /// <param name="doRetries">Whether or not to retry the call if it fails.</param>
        /// <returns>An XML string with all the configuration information.</returns>
        public static string GetConfiguration(Credentials credentials, string configurationUrl, bool doRetries = true)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);
            logger.LogInfo("Request URL = {0}", configurationUrl);

            var request = CreateSkytapWebRequest(credentials, configurationUrl);
            request.Method = WebRequestMethods.Http.Get;

            string responseContent;
            if (doRetries)
            {
                responseContent = Retry.Execute(() => SendSkytapHttpRequest(request), _configParams.RetryNumRetries, _configParams.RetryWaitTime);
            }
            else
            {
                responseContent = SendSkytapHttpRequest(request);
            }

            if (!string.IsNullOrEmpty(responseContent))
            {
                logger.LogInfo("{0}: RESPONSE = {1}\n", MethodBase.GetCurrentMethod().Name, responseContent);
            }

            return responseContent;
        }

        /// <summary>
        /// Gets the current state of the configuration, such as Running, Suspended, or Stopped.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="configurationUrl">URL of the configuration to query.</param>
        /// <returns>A string representing the state of the configuration, such as running, suspended, or stopped.</returns>
        /// <remarks>This is a helper method that extracts the runstate out of the XML response, but you
        /// could just as easily use <seealso cref="GetConfiguration"/> to get the full response XML
        /// containing all properties.</remarks>
        public static string GetConfigurationState(Credentials credentials, string configurationUrl)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);

            var responseContent = GetConfiguration(credentials, configurationUrl);

            // Parse out the response XML to get just the runstate for return to the caller. 
            var xmldoc = new XmlDocument();
            xmldoc.LoadXml(responseContent);

            var configStateXmlNode = xmldoc.SelectSingleNode("/configuration/runstate");
            Debug.Assert(configStateXmlNode != null);

            return configStateXmlNode.InnerText;
        }

        /// <summary>
        /// Determine if a configuration is in an expected state, and throw an exception if it is not.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="testTargetConfigUrl">Configuration to query for the desired state.</param>
        /// <param name="desiredState">The desired state of the configuration, such as Stopped, Suspended, or Running.</param>
        public static void CheckConfigurationForDesiredState(Credentials credentials, string testTargetConfigUrl, string desiredState)
        {
            CheckConfigurationForDesiredState(credentials, testTargetConfigUrl, new List<string> { desiredState });
        }

        /// <summary>
        /// Determine if a configuration is in an expected state, and throw an exception if it is not.
        /// </summary>
        /// <param name="credentials">Username and password for the user making the request.</param>
        /// <param name="testTargetConfigUrl">Configuration to query for the desired state.</param>
        /// <param name="desiredStates">A list of states that are desired, such as a combination of
        /// Stopped, Suspended, or Running.</param>
        public static void CheckConfigurationForDesiredState(Credentials credentials, string testTargetConfigUrl, List<string> desiredStates)
        {
            var logger = LoggerFactory.GetLogger();
            logger.LogInfo("Enter {0}... \n", MethodBase.GetCurrentMethod().Name);

            Debug.Assert(!string.IsNullOrEmpty(testTargetConfigUrl));
            Debug.Assert(desiredStates != null && desiredStates.Count > 0);

            var actualState = GetConfigurationState(credentials, testTargetConfigUrl);

            if (desiredStates.Contains(actualState))
            {
                logger.LogInfo("Configuration entered one of the desired states: {0}", actualState);
            }
            else
            {
                var desiredStatesString = desiredStates.Aggregate(string.Empty, (current, s) => current + (s + ", "));
                logger.LogInfo("Configuration NOT in one of the desired states: Desired = {0}; Actual = {1}", desiredStatesString, actualState);

                throw new ApplicationException(string.Format(Resources.SkytapApi_CheckConfigurationForDesiredState_ERROR_UnexpectedState, testTargetConfigUrl, desiredStatesString));
            }
        }

        internal static HttpWebRequest CreateSkytapWebRequest(Credentials credentials, string url)
        {
            Debug.Assert(!string.IsNullOrEmpty(credentials.Username));
            Debug.Assert(!string.IsNullOrEmpty(credentials.Key));
            Debug.Assert(!string.IsNullOrEmpty(url));

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = _configParams.HttpTimeout;
            request.ReadWriteTimeout = _configParams.HttpTimeout;
            request.ContentType = HttpApiXmlMimeType;
            request.Accept = HttpApiXmlMimeType;

            // DEBUG CODE ONLY - Setting KeepAlive should be kept true for request/response efficiency
            //
            // Set KeepAlive to false explicitly for reliability reasons. This will open a new connection
            // to the Skytap servers for every request and is less efficient, but may prevent connection
            // issues with timeouts and responses not being received.
            // request.KeepAlive = false;

            var usernameKey = string.Format("{0}:{1}", credentials.Username, credentials.Key);
            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameKey));

            return request;
        }

        internal static string SendSkytapHttpRequest(HttpWebRequest httpRequest)
        {
            const int httpStatusCodeErrorBegin = 400;

            if (httpRequest == null)
            {
                throw new ArgumentNullException("httpRequest", Resources.SkytapAPI_SendSkytapHttpRequest_WebRequestNull);
            }

            var logger = LoggerFactory.GetLogger();

            HttpWebResponse httpResponse = null;
            try
            {
                using (httpResponse = (HttpWebResponse) httpRequest.GetResponse())
                {
                    // Status codes indicate categories, as follows:
                    //  1xx -- Informational
                    //  2xx -- Successful
                    //  3xx -- Redirection
                    //  4xx -- Client Error
                    //  5xx -- Server Error
                    //
                    // As a result, ensure that if the response code is >= 400 that we trigger 
                    // an error in the call. Note that the enumeration maps to the specific integer
                    // HTTP status code.
                    if ((int)httpResponse.StatusCode >= httpStatusCodeErrorBegin)
                    {
                        throw new WebException(
                            string.Format(Resources.SkytapApi_SendSkytapHttpRequest_ERROR__UnexpectedHttpStatusCode, (int)httpResponse.StatusCode));
                    }

                    Debug.Assert(httpResponse != null);
                    logger.LogInfo("DEBUG:Header " + httpResponse.Headers + "\n");

                    string responseString;
                    using (var responseStream = httpResponse.GetResponseStream())
                    {
                        Debug.Assert(responseStream != null);

                        var reader = new StreamReader(responseStream);
                        responseString = reader.ReadToEnd();
                    }

                    return responseString;
                }
            }
            catch (Exception e)
            {
                // NOTE: generally we do not want to catch ALL exceptions, but since we are logging and rethrowing,
                // this is generally acceptable. The exceptions we expect here are HttpException or 
                // WebException.
                logger.LogError("ERROR: Exception (type: {0}) --> {1}\n", e.GetType().ToString(), e.Message);

                if (httpResponse != null)
                {
                    logger.LogError("DEBUG: Header --> {0}\n", httpResponse.Headers);
                }

                // Rethrow the exact same exception so the caller is notified there was a problem
                throw;
            }
        }

    }
}
