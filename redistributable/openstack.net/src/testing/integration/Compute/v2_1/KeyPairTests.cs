using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Xunit;
using Xunit.Abstractions;
using VolumeState = net.openstack.Core.Domain.VolumeState;

namespace OpenStack.Compute.v2_1
{
    public class KeyPairTests : IDisposable
    {
        private readonly ComputeService _compute;
        private readonly ComputeTestDataManager _testData;

        public KeyPairTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            Trace.Listeners.Add(testOutput);
            OpenStackNet.Tracing.Http.Listeners.Add(testOutput);

            var authenticationProvider = TestIdentityProvider.GetIdentityProvider();
            _compute = new ComputeService(authenticationProvider, "RegionOne");

            _testData = new ComputeTestDataManager(_compute);
        }

        public void Dispose()
        {
            _testData.Dispose();

            Trace.Listeners.Clear();
            OpenStackNet.Tracing.Http.Listeners.Clear();
        }

        [Fact]
        public async Task CreateKeyPairTest()
        {
            var request = _testData.BuildKeyPairRequest();

            Trace.WriteLine($"Creating keypair named: {request.Name}");
            var keypair = await _testData.CreateKeyPair(request);
            

            Trace.WriteLine("Verifying keypair matches requested definition...");
            Assert.NotNull(keypair);
            Assert.Equal(request.Name, keypair.Name);
            Assert.NotNull(keypair.PrivateKey);
            Assert.NotNull(keypair.PrivateKey);
            Assert.NotNull(keypair.Fingerprint);
        }

        [Fact]
        public async Task ImportKeyPairTest()
        {
            var definition = new KeyPairDefinition(TestData.GenerateName(), "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQDrBzodZLiWO6nIGGy9ZOVeFhbF6EaG8HUqrknNVKynH6+Hc5ToY71gmeQGJ7XZTAlyKKdFmPhNPCQCYqFQxjPKD3xTIAoGChlRHfkjYwjefbqxFswi9S0Fi3Lq8mawUVuPmPnuTr8KhL8ibnBbAxZnrcfTKBIoxhU+kN56CCmLnkJc5ouG/UcF+UpqUso45pYRf0YWANyyuafyCmj6NiDxMCGy/vnKUBLzMg8wQ01xGSGOfyGDIwuTFZpoPzjeqEV8oUGvxYt9Xyzh/pPKoOz1cz0wBDaVDpucTz3UEq65F9HhCmdwwjso8MP1K46LkM2JNQWQ0eTotqFvUJEoP2ff Generated-by-Nova");

            Trace.WriteLine($"Importing keypair named: {definition.Name}");
            var keypair = await _compute.ImportKeyPairAsync(definition);
            _testData.Register(keypair);

            Trace.WriteLine("Verifying keypair matches requested definition...");
            Assert.NotNull(keypair);
            Assert.Equal(definition.Name, keypair.Name);
            Assert.NotNull(keypair.Fingerprint);
        }

        [Fact]
        public async Task ListKeypairsTest()
        {
            Trace.WriteLine("Generating test data...");
            var keypair = await _testData.CreateKeyPair();
            await _testData.CreateKeyPair();

            Trace.WriteLine("Listing keypairs...");
            var results = await _compute.ListKeyPairsAsync();

            Assert.NotNull(results);
            Assert.Contains(results, x => x.Name == keypair.Name);
        }

        [Fact]
        public async Task DeleteKeyPairTest()
        {
            var keypair = await _testData.CreateKeyPair();
            Trace.WriteLine($"Created keypair named: {keypair.Name}");

            Trace.WriteLine($"Deleting keypair...");
            await keypair.DeleteAsync();

            await Assert.ThrowsAsync<FlurlHttpException>(() => _compute.GetKeyPairAsync(keypair.Name));
        }
    }
}
