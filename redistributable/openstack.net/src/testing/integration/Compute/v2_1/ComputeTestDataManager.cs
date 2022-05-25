using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using net.openstack.Providers.Rackspace;
using OpenStack.BlockStorage.v2;

namespace OpenStack.Compute.v2_1
{
    public class ComputeTestDataManager : IDisposable
    {
        private readonly ComputeService _compute;
        private readonly HashSet<object> _testData;
         
        public ComputeTestDataManager(ComputeService compute)
        {
            _compute = compute;
            _testData = new HashSet<object>();

            var identityProvider = TestIdentityProvider.GetIdentityProvider();
            var blockStorage = new CloudBlockStorageProvider(null, "RegionOne", identityProvider, null);
            BlockStorage = new BlockStorageTestDataManager(blockStorage);
        }

        public BlockStorageTestDataManager BlockStorage { get; }        

        public void Register(IEnumerable<object> testItems)
        {
            foreach (var testItem in testItems)
            {
                Register(testItem);
            }
        }

        public void Register(object testItem)
        {
            _testData.Add(testItem);
        }

        public void Dispose()
        {
            var errors = new List<Exception>();

            try
            {
                DeleteServers(_testData.OfType<Server>());
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            try
            {
                DeleteImages(_testData.OfType<Image>());
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            try
            {
                DeleteSecurityGroups(_testData.OfType<SecurityGroup>());
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            try
            {
                DeleteServerGroups(_testData.OfType<ServerGroup>());
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            try
            {
                DeleteVolumeSnapshots(_testData.OfType<VolumeSnapshot>());
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            try
            {
                DeleteVolumes(_testData.OfType<Volume>());
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            try
            {
                DeleteKeyPairs(_testData.OfType<KeyPairSummary>());
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            try
            {
                BlockStorage.Dispose();
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            if (errors.Any())
                throw new AggregateException("Unable to remove all test data!", errors);
        }

        #region Servers
        public ServerCreateDefinition BuildServer()
        {
            string name = TestData.GenerateName();
            var flavor = GetDefaultFlavor();
            var image = GetDefaultImage();
            Task.WaitAll(flavor, image);
            return new ServerCreateDefinition(name, image.Result, flavor.Result);
        }

        private Identifier _defaultFlavor;
        private async Task<Identifier> GetDefaultFlavor()
        {
            if (_defaultFlavor == null)
            {
                var flavors = await _compute.ListFlavorSummariesAsync();
                _defaultFlavor = flavors.First(x => x.Name == "m1.tiny").Id;
            }
            return _defaultFlavor;
        }

        private Identifier _defaultImage;
        private async Task<Identifier> GetDefaultImage()
        {
            if (_defaultImage == null)
            {
                var images = await _compute.ListImageSummariesAsync(new ImageListOptions {Name = "cirros"});
                _defaultImage = images.First().Id;
            }
            return _defaultImage;
        }

        public async Task<Server> CreateServer()
        {
            var definition = BuildServer();
            return await CreateServer(definition);
        }

        public async Task<Server> CreateServer(ServerCreateDefinition definition)
        {
            var server = await _compute.CreateServerAsync(definition);
            Register(server);
            return server;
        }

        public async Task<IEnumerable<Server>> CreateServers()
        {
            var definitions = new[] { BuildServer(), BuildServer(), BuildServer() };
            return await CreateServers(definitions);
        }

        public async Task<IEnumerable<Server>> CreateServers(IEnumerable<ServerCreateDefinition> definitions)
        {
            var creates = definitions.Select(definition => _compute.CreateServerAsync(definition)).ToArray();
            var servers = await Task.WhenAll(creates);
            Register(servers);
            return servers;
        }

        public void DeleteServers(IEnumerable<Server> servers)
        {
            var deletes = servers.Select(x => x.DeleteAsync()).ToArray();
            Task.WaitAll(deletes);
        }
        #endregion

        #region Images
        public void DeleteImages(IEnumerable<Image> images)
        {
            var deletes = images.Select(x => x.DeleteAsync()).ToArray();
            Task.WaitAll(deletes);
        }
        #endregion

        #region Security Groups
        public SecurityGroupDefinition BuildSecurityGroup()
        {
            string name = TestData.GenerateName();
            return new SecurityGroupDefinition(name, "ci test data");
        }

        public async Task<SecurityGroup> CreateSecurityGroup()
        {
            var definition = BuildSecurityGroup();
            return await CreateSecurityGroup(definition);
        }

        public async Task<SecurityGroup> CreateSecurityGroup(SecurityGroupDefinition definition)
        {
            var securityGroup = await _compute.CreateSecurityGroupAsync(definition);
            Register(securityGroup);
            return securityGroup;
        }
        
        public void DeleteSecurityGroups(IEnumerable<SecurityGroup> securityGroups)
        {
            var deletes = securityGroups.Select(x => x.DeleteAsync()).ToArray();
            Task.WaitAll(deletes);
        }
        #endregion

        #region Server Groups
        public ServerGroupDefinition BuildServerGroup()
        {
            string name = TestData.GenerateName();
            return new ServerGroupDefinition(name, "affinity");
        }

        public async Task<ServerGroup> CreateServerGroup()
        {
            var definition = BuildServerGroup();
            return await CreateServerGroup(definition);
        }

        public async Task<ServerGroup> CreateServerGroup(ServerGroupDefinition definition)
        {
            var serverGroup = await _compute.CreateServerGroupAsync(definition);
            Register(serverGroup);
            return serverGroup;
        }

        public void DeleteServerGroups(IEnumerable<ServerGroup> serverGroups)
        {
            var deletes = serverGroups.Select(x => x.DeleteAsync()).ToArray();
            Task.WaitAll(deletes);
        }
        #endregion

        #region Volumes
        public VolumeDefinition BuildVolume()
        {
            return new VolumeDefinition(1)
            {
                Name = TestData.GenerateName()
            };
        }

        public async Task<Volume> CreateVolume()
        {
            var definition = BuildVolume();
            return await CreateVolume(definition);
        }

        public async Task<Volume> CreateVolume(VolumeDefinition definition)
        {
            var volume = await _compute.CreateVolumeAsync(definition);
            Register(volume);
            return volume;
        }

        public void DeleteVolumes(IEnumerable<Volume> volumes)
        {
            var gets = volumes.Select(v => _compute.GetVolumeAsync(v.Id)).ToArray();
            Task.WaitAll(gets);

            var serverDeletes = gets.SelectMany(v => v.Result.Attachments.Select(a => a.ServerId))
                .Select(serverId => _compute.WaitUntilServerIsDeletedAsync(serverId)).ToArray();
            Task.WaitAll(serverDeletes);

            var deletes = volumes.Select(x => x.DeleteAsync()).ToArray();
            Task.WaitAll(deletes);
        }

        public void DeleteVolumeSnapshots(IEnumerable<VolumeSnapshot> snapshots)
        {
            var deletes = snapshots.Select(x => x.DeleteAsync()).ToArray();
            Task.WaitAll(deletes);

            // workaround 500 errors when deleting a volume which still has snapshots
            var waits = snapshots.Select(x => x.WaitUntilDeletedAsync()).ToArray();
            Task.WaitAll(waits);
        }
        #endregion

        #region Key Pairs
        public KeyPairRequest BuildKeyPairRequest()
        {
            return new KeyPairRequest(TestData.GenerateName());
        }

        public Task<KeyPairResponse> CreateKeyPair()
        {
            return CreateKeyPair(BuildKeyPairRequest());
        }

        public async Task<KeyPairResponse> CreateKeyPair(KeyPairRequest request)
        {
            var keypair = await _compute.CreateKeyPairAsync(request);
            Register(keypair);
            return keypair;
        }

        public void DeleteKeyPairs(IEnumerable<KeyPairSummary> keypairs)
        {
            var deletes = keypairs.Select(x => x.DeleteAsync()).ToArray();
            Task.WaitAll(deletes);
        }
        #endregion
    }
}
