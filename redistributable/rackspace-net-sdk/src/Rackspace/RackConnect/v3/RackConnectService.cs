using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Extensions;
using Flurl.Http;
using OpenStack.Authentication;
using OpenStack.Serialization;

namespace Rackspace.RackConnect.v3
{
    /// <summary>
    /// The Rackspace RackConnect (Hybrid Cloud) service.
    /// </summary>
    /// <seealso href="http://www.rackspace.com/cloud/hybrid/rackconnect">Rackspace Hybrid Cloud / RackConnect Overview</seealso>
    /// <seealso href="http://docs.rcv3.apiary.io/">RackConnect v3 API</seealso>
    /// <threadsafety static="true" instance="false"/>
    public class RackConnectService
    {
        /// <summary />
        private readonly IAuthenticationProvider _authenticationProvider;

        /// <summary />
        private readonly ServiceEndpoint _urlBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="RackConnectService"/> class.
        /// </summary>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="region">The region.</param>
        public RackConnectService(IAuthenticationProvider authenticationProvider, string region)
        {
            if (authenticationProvider == null)
                throw new ArgumentNullException("authenticationProvider");
            if (string.IsNullOrEmpty(region))
                throw new ArgumentException("region cannot be null or empty", "region");

            RackspaceNet.Configure();

            _authenticationProvider = authenticationProvider;
            _urlBuilder = new ServiceEndpoint(ServiceType.RackConnect, authenticationProvider, region, false);
        }

        private void SetOwner(IServiceResource<RackConnectService> resource)
        {
            resource.Owner = this;
        }

        #region Public IPs

        /// <summary>
        /// Lists all public IP addresses associated with the account.
        /// </summary>
        /// <param name="filter">Optional filter parameters.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// A collection of public IP addresses associated with the account.
        /// </returns>
        public async Task<IEnumerable<PublicIP>> ListPublicIPsAsync(ListPublicIPsFilter filter = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            filter = filter ?? new ListPublicIPsFilter();

            Url endpoint = await _urlBuilder.GetEndpoint(cancellationToken).ConfigureAwait(false);

            var ips = await endpoint
                .AppendPathSegments("public_ips")
                .SetQueryParams(new
                {
                    cloud_server_id = filter.ServerId,
                    retain = filter.IsRetained
                })
                .PrepareRequest()
                .GetJsonAsync<IEnumerable<PublicIP>>(cancellationToken)
                .ConfigureAwait(false);

            foreach (var ip in ips)
            {
                SetOwner(ip);
            }

            return ips;
        }

        /// <summary>
        /// Allocates a public IP address to the current account.
        /// </summary>
        /// <param name="definition">The public IP definition.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The identifer of the public IP address while it is being provisioned. Use <see cref="WaitUntilPublicIPIsActiveAsync"/> to wait for the IP address to be fully active.</returns>
        public async Task<PublicIP> CreatePublicIPAsync(PublicIPCreateDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            string endpoint = await _urlBuilder.GetEndpoint(cancellationToken).ConfigureAwait(false);

            Func<Task<PublicIP>> executeRequest = async () =>
            {
                var ip = await new Url(endpoint)
                    .AppendPathSegment("public_ips")
                    .PrepareRequest()
                    .PostJsonAsync(definition, cancellationToken)
                    .ReceiveJson<PublicIP>()
                    .ConfigureAwait(false);

                SetOwner(ip);

                return ip;
            };

            try
            {
                return await executeRequest();
            }
            catch (FlurlHttpException ex)
            {
                if (await AssignIPFailedDueToServerCreationRaceConditionAsync(ex))
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    return await executeRequest();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Check if a request to provision a Public IP failed because of a race condition
        /// between when we created the server and when we requested the IP
        /// i.e. The RackConnect API asked for the server details from the server API and got back a 404
        /// </summary>
        private static async Task<bool> AssignIPFailedDueToServerCreationRaceConditionAsync(FlurlHttpException ex)
        {
            if (ex.Call.HttpResponseMessage.StatusCode != HttpStatusCode.Conflict)
                return false;

            string errorMessage = await ex.GetResponseStringAsync();
            return Regex.IsMatch(errorMessage, "Cloud Server .* (unprocessable|exist).*");
        }

        /// <summary>
        /// Gets the specified public IP address.
        /// </summary>
        /// <param name="publicIPId">The public IP address identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<PublicIP> GetPublicIPAsync(Identifier publicIPId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await _urlBuilder.GetEndpoint(cancellationToken).ConfigureAwait(false);
            
            var ip = await endpoint
                .AppendPathSegments("public_ips", publicIPId)
                .PrepareRequest()
                .GetJsonAsync<PublicIP>(cancellationToken)
                .ConfigureAwait(false);

            SetOwner(ip);

            return ip;
        }

        /// <summary>
        /// Waits for the public IP address to become active.
        /// </summary>
        /// <param name="publicIPId">The public IP address identifier.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public async Task<PublicIP> WaitUntilPublicIPIsActiveAsync(Identifier publicIPId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(publicIPId))
                throw new ArgumentNullException("publicIPId");

            refreshDelay = refreshDelay ?? TimeSpan.FromSeconds(5);
            timeout = timeout ?? TimeSpan.FromMinutes(5);

            using (var timeoutSource = new CancellationTokenSource(timeout.Value))
            using (var rootCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token))
            {
                while (true)
                {
                    PublicIP ip = await GetPublicIPAsync(publicIPId, cancellationToken).ConfigureAwait(false);
                    if (ip.Status == PublicIPStatus.CreateFailed || ip.Status == PublicIPStatus.UpdateFailed)
                        throw new ServiceOperationFailedException(ip.StatusDetails);

                    bool complete = ip.Status == PublicIPStatus.Active;

                    progress?.Report(complete);

                    if (complete)
                        return ip;

                    try
                    {
                        await Task.Delay(refreshDelay.Value, rootCancellationToken.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (timeoutSource.IsCancellationRequested)
                            throw new TimeoutException($"The requested timeout of {timeout.Value.TotalSeconds} seconds has been reached while waiting for the public IP ({publicIPId}) to become active.", ex);

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Waits for the public IP address to be removed from the current account.
        /// </summary>
        /// <param name="publicIPId">The public IP address identifier.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public async Task WaitUntilPublicIPIsDeletedAsync(Identifier publicIPId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(publicIPId))
                throw new ArgumentNullException("publicIPId");

            refreshDelay = refreshDelay ?? TimeSpan.FromSeconds(5);
            timeout = timeout ?? TimeSpan.FromMinutes(5);

            using (var timeoutSource = new CancellationTokenSource(timeout.Value))
            using (var rootCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token))
            {
                while (true)
                {
                    bool complete;
                    try
                    {
                        PublicIP ip = await GetPublicIPAsync(publicIPId, cancellationToken).ConfigureAwait(false);
                        if(ip.Status == PublicIPStatus.DeleteFailed)
                            throw new ServiceOperationFailedException(ip.StatusDetails);

                        complete = ip.Status == PublicIPStatus.Deleted;
                    }
                    catch (FlurlHttpException httpError)
                    {
                        if (httpError.Call.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                            complete = true;
                        else
                            throw;
                    }

                    progress?.Report(complete);

                    if (complete)
                        return;

                    try
                    {
                        await Task.Delay(refreshDelay.Value, rootCancellationToken.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (timeoutSource.IsCancellationRequested)
                            throw new TimeoutException($"The requested timeout of {timeout.Value.TotalSeconds} seconds has been reached while waiting for the public IP ({publicIPId}) to be removed.", ex);

                        throw;
                    }
                }
            }
        }
        
        /// <summary>
        /// Removes the public IP address from the current account.
        /// </summary>
        /// <param name="publicIPId">The public IP address identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task DeletePublicIPAsync(Identifier publicIPId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (publicIPId == null)
                throw new ArgumentNullException("publicIPId");

            Url endpoint = await _urlBuilder.GetEndpoint(cancellationToken).ConfigureAwait(false);
            
            await endpoint
                .AppendPathSegments("public_ips", publicIPId)
                .PrepareRequest()
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .DeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the public IP address.
        /// </summary>
        /// <param name="publicIPId">The public IP address identifier.</param>
        /// <param name="definition">The updated public IP definition.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<PublicIP> UpdatePublicIPAsync(Identifier publicIPId, PublicIPUpdateDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            string endpoint = await _urlBuilder.GetEndpoint(cancellationToken).ConfigureAwait(false);

            Func<Task<PublicIP>> executeRequest = async () =>
            {
                var ip = await endpoint
                 .AppendPathSegments("public_ips", publicIPId)
                 .PrepareRequest()
                 .PatchJsonAsync(definition, cancellationToken)
                 .ReceiveJson<PublicIP>()
                 .ConfigureAwait(false);

                SetOwner(ip);
                return ip;
            };

            try
            {
                return await executeRequest();
            }
            catch (FlurlHttpException ex)
            {
                if (await AssignIPFailedDueToServerCreationRaceConditionAsync(ex))
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    return await executeRequest();
                }
                else
                {
                    throw;
                }
            }
        }
        #endregion

        #region Networks
        /// <summary>
        /// Lists Cloud Networks associated with a RackConnect Configuration.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// A collection of networks associated with the account.
        /// </returns>
        public async Task<IEnumerable<NetworkReference>> ListNetworksAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await _urlBuilder.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return await endpoint
                .AppendPathSegments("cloud_networks")
                .PrepareRequest()
                .GetJsonAsync<IEnumerable<NetworkReference>>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the specified RackConnect Cloud Network.
        /// </summary>
        /// <param name="networkId">The network identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<NetworkReference> GetNetworkAsync(Identifier networkId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await _urlBuilder.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return await endpoint
                .AppendPathSegments("cloud_networks", networkId)
                .PrepareRequest()
                .GetJsonAsync<NetworkReference>(cancellationToken)
                .ConfigureAwait(false);
        }
        #endregion

    }
}