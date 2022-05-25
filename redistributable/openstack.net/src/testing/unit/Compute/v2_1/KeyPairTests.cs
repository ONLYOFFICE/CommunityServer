using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;
using OpenStack.Synchronous;
using OpenStack.Testing;
using Xunit;

namespace OpenStack.Compute.v2_1
{
    public class KeyPairTests
    {
        private readonly ComputeService _computeService;

        public KeyPairTests()
        {
            _computeService = new ComputeService(Stubs.AuthenticationProvider, "region");
        }

        [Fact]
        public void DeserializeKeyPairCollection()
        {
            // this one is odd because even in the list, the items are wrapped with a root that needs to be unwrapped
            string json = JObject.Parse(@"{
  'keypairs': [
    {
      'keypair': {
        'public_key': 'ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQDrBzodZLiWO6nIGGy9ZOVeFhbF6EaG8HUqrknNVKynH6+Hc5ToY71gmeQGJ7XZTAlyKKdFmPhNPCQCYqFQxjPKD3xTIAoGChlRHfkjYwjefbqxFswi9S0Fi3Lq8mawUVuPmPnuTr8KhL8ibnBbAxZnrcfTKBIoxhU+kN56CCmLnkJc5ouG/UcF+UpqUso45pYRf0YWANyyuafyCmj6NiDxMCGy/vnKUBLzMg8wQ01xGSGOfyGDIwuTFZpoPzjeqEV8oUGvxYt9Xyzh/pPKoOz1cz0wBDaVDpucTz3UEq65F9HhCmdwwjso8MP1K46LkM2JNQWQ0eTotqFvUJEoP2ff Generated-by-Nova',
        'name': 'keypair-d20a3d59-9433-4b79-8726-20b431d89c78',
        'fingerprint': 'ce:88:fe:6a:9e:c0:d5:91:08:8b:57:80:be:e6:ec:3d'
      }
},
    {
      'keypair': {
        'public_key': 'ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAAAgQDx8nkQv/zgGgB4rMYmIf+6A4l6Rr+o/6lHBQdW5aYd44bd8JttDCE/F/pNRr0lRE+PiqSPO8nDPHw0010JeMH9gYgnnFlyY3/OcJ02RhIPyyxYpv9FhY+2YiUkpwFOcLImyrxEsYXpD/0d3ac30bNH6Sw9JD9UZHYcpSxsIbECHw== Generated-by-Nova',
        'name': 'uploaded-keypair',
        'fingerprint': '1e:2c:9b:56:79:4b:45:77:f9:ca:7a:98:2c:b0:d5:3c'
      }
    }
  ]
}").ToString();

            var results = OpenStackNet.Deserialize<KeyPairSummaryCollection>(json);
            Assert.NotNull(results);
            Assert.Equal(2, results.Count());
            var result = results.First();
            Assert.Empty(((IHaveExtraData)result).Data);
            Assert.NotNull(result.PublicKey);
        }

        [Fact]
        public void GetKeyPair()
        {
            using (var httpTest = new HttpTest())
            {
                const string name = "keypair-name";
                httpTest.RespondWithJson(new KeyPair { Name = name });
                KeyPair result = _computeService.GetKeyPair(name);

                httpTest.ShouldHaveCalled($"*/os-keypairs/{name}");
                Assert.NotNull(result);
                Assert.Equal(name, result.Name);
            }
        }

        [Fact]
        public void CreateKeyPair()
        {
            using (var httpTest = new HttpTest())
            {
                const string name = "keypair-name";
                httpTest.RespondWithJson(new KeyPairResponse {Name = name, PrivateKey = Guid.NewGuid().ToString()});
                KeyPairResponse result = _computeService.CreateKeyPair(new KeyPairRequest(name));

                httpTest.ShouldHaveCalled("*/os-keypairs");
                Assert.NotNull(result);
                Assert.Equal(name, result.Name);
                Assert.NotNull(result.PrivateKey);
            }
        }

        [Fact]
        public void ImportKeyPair()
        {
            using (var httpTest = new HttpTest())
            {
                const string name = "keypair-name";
                httpTest.RespondWithJson(new KeyPairSummary {Name = name});
                KeyPairSummary result = _computeService.ImportKeyPair(new KeyPairDefinition(name, Guid.NewGuid().ToString()));

                httpTest.ShouldHaveCalled("*/os-keypairs");
                Assert.NotNull(result);
                Assert.Equal(name, result.Name);
            }
        }

        [Fact]
        public void ListKeypairs()
        {
            using (var httpTest = new HttpTest())
            {
                const string name = "keypair-name";
                httpTest.RespondWithJson(new KeyPairSummaryCollection
                {
                    new KeyPairSummary { Name = name }
                });
                IEnumerable<KeyPairSummary> results = _computeService.ListKeyPairs();

                httpTest.ShouldHaveCalled("*/os-keypairs");
                Assert.NotNull(results);
                Assert.Single(results);
                Assert.Equal(name, results.First().Name);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NotFound)]
        public void DeleteKeyPair(HttpStatusCode responseCode)
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new KeyPair {Name = "keypair-name"});
                httpTest.RespondWith((int) responseCode, "All gone!");

                KeyPairSummary result = _computeService.GetKeyPair("keypair-name");
                result.Delete();

                httpTest.ShouldHaveCalled("*/os-keypairs");
            }
        }
    }
}
