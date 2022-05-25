using System;
using System.Collections.Generic;
using System.Net;
using JSIStudios.SimpleRESTServices.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using net.openstack.Core.Caching;
using net.openstack.Core.Domain;
using net.openstack.Providers.Rackspace;
using net.openstack.Providers.Rackspace.Objects;
using net.openstack.Providers.Rackspace.Objects.Response;

namespace OpenStackNet.Testing.Unit.Providers.Rackspace
{
    [TestClass]
    public class IdentityProviderCacheTests
    {
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Not_Hit_Cache_When_Authenticating_The_First_Time()
        {
            var cacheMock = new Mock<ICache<UserAccess>>();
            var restServiceMock = new Mock<IRestService>();

            restServiceMock.Setup(m => m.Execute<AuthenticationResponse>(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<RequestSettings>())).Returns(new Response<AuthenticationResponse>(HttpStatusCode.OK, "OK", new AuthenticationResponse(), new List<HttpHeader>(), null));
            restServiceMock.Setup(m => m.Execute<AuthenticationResponse>(It.IsAny<Uri>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<RequestSettings>())).Returns(new Response<AuthenticationResponse>(HttpStatusCode.OK, "OK", new AuthenticationResponse(), new List<HttpHeader>(), null));

            var identityProvider = new CloudIdentityProvider(restServiceMock.Object, cacheMock.Object);

            identityProvider.Authenticate(new RackspaceCloudIdentity());

            cacheMock.Verify(m => m.Get(It.IsAny<string>(), It.IsAny<Func<UserAccess>>(), true), Times.Once());
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Always_Request_Fresh_Data_From_Cache_When_Authenticating()
        {
            var cacheMock = new Mock<ICache<UserAccess>>();
            var restServiceMock = new Mock<IRestService>();

            restServiceMock.Setup(m => m.Execute<AuthenticationResponse>(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<RequestSettings>())).Returns(new Response<AuthenticationResponse>(HttpStatusCode.OK, "OK", new AuthenticationResponse(), new List<HttpHeader>(), null));
            restServiceMock.Setup(m => m.Execute<AuthenticationResponse>(It.IsAny<Uri>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<RequestSettings>())).Returns(new Response<AuthenticationResponse>(HttpStatusCode.OK, "OK", new AuthenticationResponse(), new List<HttpHeader>(), null));
            
            var identityProvider = new CloudIdentityProvider(restServiceMock.Object, cacheMock.Object);

            for (int i = 0; i < 100; i++)
            {
                identityProvider.Authenticate(new RackspaceCloudIdentity());
            }

            cacheMock.Verify(m => m.Get(It.IsAny<string>(), It.IsAny<Func<UserAccess>>(), true), Times.Exactly(100));
        }
    }
}
