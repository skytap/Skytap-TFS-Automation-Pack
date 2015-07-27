// 
// SkytapAPIUnitTests.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skytap.Cloud;
using Skytap.Cloud.Fakes;
using Skytap.Cloud.Utilities;
using Skytap.Utilities;

namespace SkytapCLITests
{
    [TestClass]
    public class SkytapApiTests
    {
        #region Initialize and Clean-up

        [TestInitialize]
        public void Initialize()
        {
            LoggerFactory.LoggerType = LoggerTypes.Null;
        }

        [TestCleanup]
        public void Cleanup()
        {
            LoggerFactory.GetLogger().Dispose();
        }

        #endregion Initialize and Clean-up

        #region CreateSkytabWebRequest

        [TestMethod]
        public void CreateSkytapWebRequest_Basic()
        {
            const string url = "http://someserver.somewhere.com";
            var credentials = new Credentials("John", "123456ABC");

            var httpRequest = SkytapApi.CreateSkytapWebRequest(credentials, url);

            Assert.IsNotNull(httpRequest);
            Assert.AreEqual(ApplicationParameters.DefaultHttpTimeout, httpRequest.Timeout);
            Assert.AreEqual("application/xml", httpRequest.ContentType);
            Assert.IsTrue(httpRequest.Headers["Authorization"].Contains("Basic"));
        }

        [TestMethod]
        [ExpectedException(typeof(UriFormatException))]
        public void SkytapApi_CreateSkytapWebRequest_BadUrl()
        {
            const string url = "BadlyFormattedUrl";
            var credentials = new Credentials("John", "123456ABC");

            SkytapApi.CreateSkytapWebRequest(credentials, url);
        }

        #endregion CreateSkytabWebRequest

        #region SendSkytabHttpRequest

        [TestMethod]
        public void SkytapApi_SendSkytapHttpRequest_Success()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var webHeaders = new ShimWebHeaderCollection();
                webHeaders.ToString = () => "TestString";

                var response = new ShimHttpWebResponse();
                response.StatusCodeGet = () => HttpStatusCode.OK;
                response.HeadersGet = () => webHeaders;
                response.GetResponseStream = () => new MemoryStream(System.Text.Encoding.UTF8.GetBytes("BodyString"));

                var request = new ShimHttpWebRequest();
                request.GetResponse = () => response;

                // This was experimental code that is left here for reference purposes. It shows how to 
                // simulate an exception being thrown in a shimmed object's method.
                //
                //var webException = new ShimHttpException(); 
                //ShimHttpWebRequest.AllInstances.GetResponse = url => { throw (HttpException)webException; };

                // Act
                var responseString = SkytapApi.SendSkytapHttpRequest(request);

                // Assert
                Assert.IsFalse(string.IsNullOrEmpty(responseString));
            }
        }

        [TestMethod]
        public void SkytapApi_SendSkytapHttpRequest_BadHttpStatuses()
        {
            var badHttpStatuses = new[] {HttpStatusCode.NotFound, HttpStatusCode.ServiceUnavailable};

            using (ShimsContext.Create())
            {
                foreach (var httpStatus in badHttpStatuses)
                {
                    var response = new ShimHttpWebResponse();

                    var status = httpStatus; // required as accessing foreach variable may have different behavior with different compilers
                    response.StatusCodeGet = () => status;

                    var request = new ShimHttpWebRequest();
                    request.GetResponse = () => response;

                    try
                    {
                        // The following call should trigger an exception so the next line should never be hit
                        SkytapApi.SendSkytapHttpRequest(request);

                        Assert.Fail("Exception was expected; fail the test");
                    }
                    catch (Exception e)
                    {
                        StringAssert.Contains(e.Message, ((int)httpStatus).ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SkytapApi_SendSkytapHttpRequest_NullInputParam()
        {
            SkytapApi.SendSkytapHttpRequest(null);
        }

        #endregion SendSkytabHttpRequest

        #region General APIs

        //
        // The tests in this region are not complete, i.e. they do not test all of the APIs. Part of
        // the reason for this is that the integration tests typically hit all of the API methods 
        // anyway, so implementing unit tests at this level after development of them provides
        // only incremental additional value. Ideally, unit tests for all API methods are included,
        // but it is not super high priority.
        //
        // A couple tests are included as samples on how to implement these kinds of tests.
        //

        private string _requestUrl = string.Empty;
        private readonly Credentials _credentials = new Credentials("someusername", "somekey");
        private ShimHttpWebRequest _webRequest;
        private string _webRequestMethod;

        private void InitializeApiTest()
        {
            _requestUrl = string.Empty;
            _webRequestMethod = string.Empty;
            _webRequest = new ShimHttpWebRequest {MethodSetString = m => _webRequestMethod = m};

            // Setup a shim for the call to create a web request so that when an API call references it
            // a dummy request is returned for unit testing purposes, and the requestURL is set 
            // appropriately so that it can be checked in the assert stage of the test.
            ShimSkytapApi.CreateSkytapWebRequestCredentialsString = (c, url) =>
            {
                _requestUrl = url;
                return _webRequest;
            };
        }

        [TestMethod]
        public void CreateIcnrConnection_Success()
        {
            using (ShimsContext.Create())
            {
                InitializeApiTest();
                
                // Setup the simulated response from the web request to ensure that the parsing is done 
                // correctly in the Skytap API.
                ShimSkytapApi.SendSkytapHttpRequestHttpWebRequest = r => "<response><tunnel><id>12345</id></tunnel></response>";

                var icnrId = SkytapApi.CreateIcnrConnection(_credentials, "abc", "def");

                Assert.AreEqual("12345", icnrId);
                Assert.IsFalse(string.IsNullOrEmpty(_requestUrl));
                StringAssert.EndsWith(_requestUrl, "/tunnels?source_network_id=abc&target_network_id=def");
                Assert.AreEqual(WebRequestMethods.Http.Post, _webRequestMethod);
            }
        }

        [TestMethod]
        public void DeleteIcnrConnection_Success()
        {
            using (ShimsContext.Create())
            {
                InitializeApiTest();

                ShimSkytapApi.SendSkytapHttpRequestHttpWebRequest = r => string.Empty;

                SkytapApi.DeleteIcnrConnection(_credentials, "abc");

                Assert.AreEqual(SkytapApi.HttpDeleteRequest, _webRequestMethod);
                Assert.IsFalse(string.IsNullOrEmpty(_requestUrl));
                StringAssert.EndsWith(_requestUrl, "/tunnels/abc");
            }
        }

        #endregion General APIs
    }
}
