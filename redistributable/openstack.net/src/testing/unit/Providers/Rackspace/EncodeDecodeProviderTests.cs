using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using net.openstack.Providers.Rackspace;

namespace OpenStackNet.Testing.Unit.Providers.Rackspace
{
    [TestClass]
    public class EncodeDecodeProviderTests
    {
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Encode_Spaces()
        {
            const string stringToBeEncoded = "This is a test with spaces";
            var encodeDecodeProvider = EncodeDecodeProvider.Default;
            var result = encodeDecodeProvider.UrlEncode(stringToBeEncoded);
            Assert.AreEqual("This%20is%20a%20test%20with%20spaces", result);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Decode_Spaces()
        {
            const string stringToBeDecoded = "This%20is%20a%20test%20with%20spaces";
            var encodeDecodeProvider = EncodeDecodeProvider.Default;
            var result = encodeDecodeProvider.UrlDecode(stringToBeDecoded);
            Assert.AreEqual("This is a test with spaces", result);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Encode_Special_Characters()
        {
            const string stringToBeEncoded = "This $ is & a ? test * with @ spaces";
            var encodeDecodeProvider = EncodeDecodeProvider.Default;
            var result = encodeDecodeProvider.UrlEncode(stringToBeEncoded);
            Assert.AreEqual("This%20%24%20is%20%26%20a%20%3f%20test%20*%20with%20%40%20spaces", result);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Decode_Special_Characters()
        {
            const string stringToBeEncoded = "This%20%24%20is%20%26%20a%20%3f%20test%20*%20with%20%40%20spaces";
            var encodeDecodeProvider = EncodeDecodeProvider.Default;
            var result = encodeDecodeProvider.UrlDecode(stringToBeEncoded);
            Assert.AreEqual("This $ is & a ? test * with @ spaces", result);
        }

    }
}
