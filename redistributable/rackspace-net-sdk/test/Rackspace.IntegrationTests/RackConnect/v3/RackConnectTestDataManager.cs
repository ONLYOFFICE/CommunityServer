using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using net.openstack.Core.Domain;
using net.openstack.Providers.Rackspace;
using Rackspace.RackConnect.v3;
using Rackspace.Synchronous;

namespace Rackspace.CloudServers.v2
{
    public class RackConnectTestDataManager : IDisposable
    {
        private readonly RackConnectService _rackConnectService;
        private readonly CloudServersTestDataManager _serverTestData;
        private readonly HashSet<object> _testData;

        public RackConnectTestDataManager(RackConnectService rackConnectService, CloudIdentityProvider authenticationProvider)
        {
            _rackConnectService = rackConnectService;
            _testData = new HashSet<object>();
            _serverTestData = new CloudServersTestDataManager(authenticationProvider);
        }
        
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
                DeletePublicIPs(_testData.OfType<PublicIP>());
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            try
            {
                _serverTestData.Dispose();
            }
            catch (AggregateException ex)
            {
                errors.AddRange(ex.InnerExceptions);
            }

            if (errors.Any())
                throw new AggregateException("Unable to remove all test data!", errors);
        }

        public async Task<PublicIP> CreatePublicIP(PublicIPCreateDefinition definition)
        {
            var ip = await _rackConnectService.CreatePublicIPAsync(definition);
            Register(ip);
            return ip;
        }

        private void DeletePublicIPs(IEnumerable<PublicIP> ips)
        {
            var deletes = ips.Select(x =>
                Task.Run(() =>
                    {
                        _rackConnectService.DeletePublicIP(x.Id);
                        _rackConnectService.WaitUntilPublicIPIsDeleted(x.Id);
                    })
                ).ToArray();
            Task.WaitAll(deletes);
        }

        public Server CreateServer(Identifier networkId)
        {
            return _serverTestData.CreateServer(networkId);
        }
    }
}
