using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using JSIStudios.SimpleRESTServices.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using net.openstack.Core;
using net.openstack.Core.Domain;
using net.openstack.Core.Providers;
using net.openstack.Providers.Rackspace;
using net.openstack.Providers.Rackspace.Objects;
using Newtonsoft.Json;

namespace OpenStackNet.Testing.Unit.Providers.Rackspace
{
    [TestClass]
    public class ProviderBaseTests
    {
        private const string _testService = "test";

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Correct_Endpoint_When_Identity_Is_Explicitly_Set_And_Region_Is_Explicitly_Declared()
        {
            UserAccess userAccess = JsonConvert.DeserializeObject<UserAccess>(
                @"{ serviceCatalog:[{ type : """ + _testService + @""", endpoints : [{region:""DFW""}, {region:""ORD""}]}], user:{""RAX-AUTH:defaultRegion"":""DFW""} }");

            var identityProviderMock = new Mock<IIdentityProvider>();
            identityProviderMock.Setup(m => m.GetUserAccess(It.IsAny<CloudIdentity>(), It.IsAny<bool>())).Returns(userAccess);
            var provider = new MockProvider(null, identityProviderMock.Object, null);

            var endpoint = provider.GetEndpoint(_testService, "DFW", new CloudIdentity());

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("DFW", endpoint.Region);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Correct_Endpoint_When_Identity_Is_Explicitly_Set_And_Region_Is_Different_Than_Default_Region()
        {
            UserAccess userAccess = JsonConvert.DeserializeObject<UserAccess>(
                @"{ serviceCatalog:[{ type : """ + _testService + @""", endpoints : [{region:""DFW""}, {region:""ORD""}]}], user:{""RAX-AUTH:defaultRegion"":""DFW""} }");

            var identityProviderMock = new Mock<IIdentityProvider>();
            identityProviderMock.Setup(m => m.GetUserAccess(It.IsAny<CloudIdentity>(), It.IsAny<bool>())).Returns(userAccess);
            var provider = new MockProvider(null, identityProviderMock.Object, null);

            var endpoint = provider.GetEndpoint(_testService, "ORD", new CloudIdentity());

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("ORD", endpoint.Region);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Correct_Endpoint_When_Identity_Set_On_Provider_And_Region_Is_Different_Than_Default_Region()
        {
            UserAccess userAccess = JsonConvert.DeserializeObject<UserAccess>(
                @"{ serviceCatalog:[{ type : """ + _testService + @""", endpoints : [{region:""DFW""}, {region:""ORD""}]}], user:{""RAX-AUTH:defaultRegion"":""DFW""} }");

            var identityProviderMock = new Mock<IIdentityProvider>();
            identityProviderMock.Setup(m => m.GetUserAccess(It.IsAny<CloudIdentity>(), It.IsAny<bool>())).Returns(userAccess);
            var provider = new MockProvider(new CloudIdentity(), identityProviderMock.Object, null);

            var endpoint = provider.GetEndpoint(_testService, "ORD", null);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("ORD", endpoint.Region);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Correct_Endpoint_When_Identity_Is_Explicitly_Set_And_Region_Is_NOT_Explicitly_Declared()
        {
            UserAccess userAccess = JsonConvert.DeserializeObject<UserAccess>(
                @"{ serviceCatalog:[{ type : """ + _testService + @""", endpoints : [{region:""DFW""}, {region:""ORD""}]}], user:{""RAX-AUTH:defaultRegion"":""DFW""} }");

            var identityProviderMock = new Mock<IIdentityProvider>();
            identityProviderMock.Setup(m => m.GetUserAccess(It.IsAny<CloudIdentity>(), It.IsAny<bool>())).Returns(userAccess);
            var provider = new MockProvider(null, identityProviderMock.Object, null);

            var endpoint = provider.GetEndpoint(_testService, null, new CloudIdentity());

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("DFW", endpoint.Region);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Correct_Endpoint_When_Identity_Is_Set_On_Provider_And_Region_Is_Explicitly_Declared()
        {
            UserAccess userAccess = JsonConvert.DeserializeObject<UserAccess>(
                @"{ serviceCatalog:[{ type : """ + _testService + @""", endpoints : [{region:""DFW""}, {region:""ORD""}]}], user:{""RAX-AUTH:defaultRegion"":""DFW""} }");

            var identityProviderMock = new Mock<IIdentityProvider>();
            identityProviderMock.Setup(m => m.GetUserAccess(It.IsAny<CloudIdentity>(), It.IsAny<bool>())).Returns(userAccess);
            var provider = new MockProvider(new CloudIdentity(), identityProviderMock.Object, null);

            var endpoint = provider.GetEndpoint(_testService, "DFW", null);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("DFW", endpoint.Region);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Correct_Endpoint_When_Identity_Is_Set_On_Provider_And_Region_Is_NOT_Explicitly_Declared()
        {
            UserAccess userAccess = JsonConvert.DeserializeObject<UserAccess>(
                @"{ serviceCatalog:[{ type : """ + _testService + @""", endpoints : [{region:""DFW""}, {region:""ORD""}]}], user:{""RAX-AUTH:defaultRegion"":""DFW""} }");

            var identityProviderMock = new Mock<IIdentityProvider>();
            identityProviderMock.Setup(m => m.GetUserAccess(It.IsAny<CloudIdentity>(), It.IsAny<bool>())).Returns(userAccess);
            var provider = new MockProvider(new CloudIdentity(), identityProviderMock.Object, null);

            var endpoint = provider.GetEndpoint(_testService, null, null);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("DFW", endpoint.Region);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Correct_LON_Endpoint_When_Identity_Is_Explicitly_Set_And_Region_Is_Explicitly_Declared()
        {
            UserAccess userAccess = JsonConvert.DeserializeObject<UserAccess>(
                @"{ serviceCatalog:[{ type : """ + _testService + @""", endpoints : [{region:""LON""}, {region:""LON2""}]}], user:{""RAX-AUTH:defaultRegion"":""LON""} }");

            var identityProviderMock = new Mock<IIdentityProvider>();
            identityProviderMock.Setup(m => m.GetUserAccess(It.IsAny<CloudIdentity>(), It.IsAny<bool>())).Returns(userAccess);
            var provider = new MockProvider(null, identityProviderMock.Object, null);

            var endpoint = provider.GetEndpoint(_testService, "LON", new RackspaceCloudIdentity());

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("LON", endpoint.Region);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Correct_LON_Endpoint_When_Identity_Is_Explicitly_Set_And_Region_Is_NOT_Explicitly_Declared()
        {
            UserAccess userAccess = JsonConvert.DeserializeObject<UserAccess>(
                @"{ serviceCatalog:[{ type : """ + _testService + @""", endpoints : [{region:""LON""}, {region:""LON2""}]}], user:{""RAX-AUTH:defaultRegion"":""LON""} }");

            var identityProviderMock = new Mock<IIdentityProvider>();
            identityProviderMock.Setup(m => m.GetUserAccess(It.IsAny<CloudIdentity>(), It.IsAny<bool>())).Returns(userAccess);
            var provider = new MockProvider(null, identityProviderMock.Object, null);

            var endpoint = provider.GetEndpoint(_testService, null, new RackspaceCloudIdentity());

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("LON", endpoint.Region);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Correct_LON_Endpoint_When_Identity_Is_Set_On_Provider_And_Region_Is_Explicitly_Declared()
        {
            UserAccess userAccess = JsonConvert.DeserializeObject<UserAccess>(
                @"{ serviceCatalog:[{ type : """ + _testService + @""", endpoints : [{region:""LON""}, {region:""LON2""}]}], user:{""RAX-AUTH:defaultRegion"":""LON""} }");

            var identityProviderMock = new Mock<IIdentityProvider>();
            identityProviderMock.Setup(m => m.GetUserAccess(It.IsAny<CloudIdentity>(), It.IsAny<bool>())).Returns(userAccess);
            var provider = new MockProvider(new RackspaceCloudIdentity(), identityProviderMock.Object, null);

            var endpoint = provider.GetEndpoint(_testService, "LON", null);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("LON", endpoint.Region);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Correct_LON_Endpoint_When_Identity_Is_Set_On_Provider_And_Region_Is_NOT_Explicitly_Declared()
        {
            UserAccess userAccess = JsonConvert.DeserializeObject<UserAccess>(
                @"{ serviceCatalog:[{ type : """ + _testService + @""", endpoints : [{region:""LON""}, {region:""LON2""}]}], user:{""RAX-AUTH:defaultRegion"":""LON""} }");

            var identityProviderMock = new Mock<IIdentityProvider>();
            identityProviderMock.Setup(m => m.GetUserAccess(It.IsAny<CloudIdentity>(), It.IsAny<bool>())).Returns(userAccess);
            var provider = new MockProvider(new RackspaceCloudIdentity(), identityProviderMock.Object, null);

            var endpoint = provider.GetEndpoint(_testService, null, null);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("LON", endpoint.Region);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Correct_LON_Endpoint_When_Identity_Is_Explicitly_And_Region_Is_Always_Empty()
        {
            UserAccess userAccess = JsonConvert.DeserializeObject<UserAccess>(
                @"{ serviceCatalog:[{ type : """ + _testService + @""", endpoints : [{region:""LON""}, {region:""LON2""}]}], user:{""RAX-AUTH:defaultRegion"":""LON""} }");

            var identityProviderMock = new Mock<IIdentityProvider>();
            identityProviderMock.Setup(m => m.GetUserAccess(It.IsAny<CloudIdentity>(), It.IsAny<bool>())).Returns(userAccess);
            var provider = new MockProvider(null, identityProviderMock.Object, null);

            var endpoint = provider.GetEndpoint(_testService, null, new RackspaceCloudIdentity());

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("LON", endpoint.Region);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Null_While_Building_Optional_Parameter_List_When_A_Null_Value_Is_Passed()
        {
            var providerBase = new MockProvider(null, null, null);

            var paramList = providerBase.BuildOptionalParameterList(null);

            Assert.IsNull(paramList);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Null_While_Building_Optional_Parameter_List_When_An_Empty_Value_Is_Passed()
        {
            var providerBase = new MockProvider(null, null, null);

            var paramList = providerBase.BuildOptionalParameterList(new Dictionary<string, string>());

            Assert.IsNull(paramList);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Null_While_Building_Optional_Parameter_List_When_All_Values_In_List_Are_InValid()
        {
            var providerBase = new MockProvider(null, null, null);

            var paramList = providerBase.BuildOptionalParameterList(new Dictionary<string, string>
                {
                    {"key1", ""},
                    {"key2", null},
                    {"key3", ""},
                    {"key4", null},
                });

            Assert.IsNull(paramList);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_All_Parameters_While_Building_Optional_Parameter_List_When_All_Values_In_List_Are_Valid()
        {
            var providerBase = new MockProvider(null, null, null);

            var paramList = providerBase.BuildOptionalParameterList(new Dictionary<string, string>
                {
                    {"key1", "val1"},
                    {"key2", "val2"},
                    {"key3", "val3"},
                    {"key4", "val4"},
                });

            Assert.AreEqual(4, paramList.Count);
            Assert.IsTrue(paramList.Any(p => p.Key == "key1" && p.Value == "val1"));
            Assert.IsTrue(paramList.Any(p => p.Key == "key2" && p.Value == "val2"));
            Assert.IsTrue(paramList.Any(p => p.Key == "key3" && p.Value == "val3"));
            Assert.IsTrue(paramList.Any(p => p.Key == "key4" && p.Value == "val4"));
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Return_Only_Valid_Parameters_While_Building_Optional_Parameter_List_When_Some_Values_In_List_Are_Valid()
        {
            var providerBase = new MockProvider(null, null, null);

            var paramList = providerBase.BuildOptionalParameterList(new Dictionary<string, string>
                {
                    {"key1", "val1"},
                    {"key2", ""},
                    {"key3", "val3"},
                    {"key4", null},
                });

            Assert.AreEqual(2, paramList.Count);
            Assert.IsTrue(paramList.Any(p => p.Key == "key1" && p.Value == "val1"));
            Assert.IsFalse(paramList.Any(p => p.Key == "key2" && p.Value == "val2"));
            Assert.IsTrue(paramList.Any(p => p.Key == "key3" && p.Value == "val3"));
            Assert.IsFalse(paramList.Any(p => p.Key == "key4" && p.Value == "val4"));
        }



        public class MockProvider : ProviderBase<IIdentityProvider>
        {
            internal MockProvider(CloudIdentity defaultIdentity, IIdentityProvider identityProvider, IRestService restService) : base(defaultIdentity, null, identityProvider, restService)
            {
            }

            public Endpoint GetEndpoint(string serviceType, string region, CloudIdentity identity)
            {
                return base.GetServiceEndpoint(identity, serviceType, null, region);
            }

            public new Dictionary<string, string> BuildOptionalParameterList(Dictionary<string, string> optionalParameters)
            {
                return base.BuildOptionalParameterList(optionalParameters);
            }  
        }
    }
}
