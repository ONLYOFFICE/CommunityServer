using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OpenStack.Authentication;

namespace Rackspace
{
    /// <summary>
    /// Default stubs for unit testing
    /// </summary>
    public static class Stubs
    {
        public static readonly IAuthenticationProvider IdentityService;

        static Stubs()
        {
            var identityServiceStub = CreateIdentityService();
            IdentityService = identityServiceStub.Object;
        }

        public static Mock<IAuthenticationProvider> CreateIdentityService()
        {
            RackspaceNet.Configure();
            
            var stub = new Mock<IAuthenticationProvider>();

            stub.Setup(provider => provider.GetToken(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult("mock-token"));

            stub.Setup(provider => provider.GetEndpoint(It.IsAny<IServiceType>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult("http://api.com"));

            return stub;
        }
    }
}