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

public class UpdateConfigManagerService : IHostedService, IDisposable
{
    private readonly ILogger<UpdateConfigManagerService> _logger;
    private readonly IUpdateConfig _proxyConfigProvider;
    private readonly IConfiguration _configuration;
    private Timer _timer = null!;

    public UpdateConfigManagerService(ILogger<UpdateConfigManagerService> logger,
                              IConfiguration configuration,
                              IUpdateConfig proxyConfigProvider)
    {
        _logger = logger;
        _proxyConfigProvider = proxyConfigProvider;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Update Config Manager Service Service running.");

        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(15));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _proxyConfigProvider.Update(GetRoutes(), GetClusters());

        _logger.LogDebug("Update Config Manager Service is working.");
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Update Config Manager Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private RouteConfig[] GetRoutes()
    {
        return new[]
        {
                new RouteConfig()
                {
                    RouteId = "DefaultRoute",
                    ClusterId = "DefaultCluster",
                    Match = new RouteMatch
                    {
                        Path = "{**catch-all}"
                    }
                }
        };
    }

    private ClusterConfig[] GetClusters()
    {
        var defaultClusterDestinations = new Dictionary<string, DestinationConfig>();
        var connectionString = _configuration.GetConnectionString("MySQL");

        using (var db = new MySqlConnection(connectionString))
        {
            db.Open();

            using (var command = new MySqlCommand("SELECT id, url as address, default_version FROM tenants_version", db))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetString("id");
                    var defaultVersion = reader.GetBoolean("default_version");
                    var destinationId = defaultVersion ? "Default" : id;

                    defaultClusterDestinations.Add($"defaultCluster/destination{destinationId}", new DestinationConfig
                    {
                        Address = reader.GetString("address")
                    });
                }
            }

            db.Close();
        }

        return new[] {
            new ClusterConfig()
            {
                ClusterId = "DefaultCluster",
                Destinations = defaultClusterDestinations
            }
        };
    }
}
