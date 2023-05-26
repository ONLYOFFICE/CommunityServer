/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


namespace ASC.ReversyProxy;

public class FilterProxyDestinationsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<FilterProxyDestinationsMiddleware> _logger;

    public FilterProxyDestinationsMiddleware(RequestDelegate next,
                                             IConfiguration configuration,
                                             IMemoryCache memoryCache,
                                             ILogger<FilterProxyDestinationsMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var proxyFeature = httpContext.GetReverseProxyFeature();
        proxyFeature.AvailableDestinations = Filter(httpContext, proxyFeature.AvailableDestinations);

        await _next(httpContext);
    }

    private IReadOnlyList<DestinationState> Filter(HttpContext httpContext, IReadOnlyList<DestinationState> availableDestinations)
    {
        var tenantVersion = _memoryCache.GetOrCreate(CacheKeys.TENANT_VERSION, ResetCacheEntry);

        var destinationId = "Default";

        var coreBaseDomain = _configuration["core:base-domain"];

        _logger.LogTrace($"core base domain: {coreBaseDomain}");

        var host = httpContext.Request.Host.Host;

        _logger.LogTrace($"request host: {host}");

        var alias = host;

        if (host.EndsWith("." + coreBaseDomain))
        {
            alias = host.Remove(host.IndexOf(coreBaseDomain) - 1);
        }

        _logger.LogTrace($"alias: {alias}");

        if (tenantVersion.TryGetValue(alias, out int version))
        {
            destinationId = version.ToString();

            _logger.LogTrace($"change destinationId: {destinationId}");

        }

        _logger.LogTrace($"destinationId: {destinationId}");

        return new List<DestinationState>()
                {
                    availableDestinations.Single(x => x.DestinationId.EndsWith(destinationId))
                };
    }

    public Dictionary<string, int> ResetCacheEntry(ICacheEntry cacheEntry)
    {
        if (!int.TryParse(_configuration["core:cache-expiration"], out int cacheEntryExpiration))
        {
            cacheEntryExpiration = 5;
        }

        cacheEntry.SetSlidingExpiration(TimeSpan.FromSeconds(cacheEntryExpiration));

        var connectionString = _configuration.GetConnectionString("MySql");

        var result = new Dictionary<string, int>();

        using (var db = new MySqlConnection(connectionString))
        {
            db.Open();

            int defaultVersion;

            using (var command = new MySqlCommand("SELECT id FROM tenants_version WHERE default_version=1 LIMIT 1", db))
            {
                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    defaultVersion = 2;

                    if (reader.HasRows)
                    {
                        defaultVersion = reader.GetInt32("id");
                    }
                }
            }

            using (var command = new MySqlCommand($"SELECT alias, mappeddomain, version FROM tenants_tenants WHERE status!=2 and version!={defaultVersion}", db))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var alias = reader.GetString("alias");
                    var mappeddomain = reader.GetValue(1).ToString();
                    var version = reader.GetInt32("version");

                    result.TryAdd(alias, version);
                    result.TryAdd(mappeddomain, version);
                }
            }

            db.Close();
        }

        return result;
    }
}