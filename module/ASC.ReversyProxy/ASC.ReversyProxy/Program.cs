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


using Yarp.ReverseProxy.Forwarder;

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
};

var builder = WebApplication.CreateBuilder(options);

var logger = LogManager.Setup()
                       .LoadConfigurationFromAppSettings()
                       .GetCurrentClassLogger();

try
{
    logger.Info("Configuring web host ({applicationContext})...", AppName);

    builder.Host.UseSystemd()
                .UseWindowsService()
                .UseNLog();

    var startup = new Startup(builder.Configuration, builder.Environment);

    startup.ConfigureServices(builder.Services);

    var app = builder.Build();

    var httpForwarder = app.Services.GetService<IHttpForwarder>();
    var loggerStartup = app.Services.GetService<ILogger<Startup>>();   

    startup.Configure(app, httpForwarder, loggerStartup);

    logger.Info("Starting web host ({applicationContext})...", AppName);

    await app.RunAsync();
}
catch (Exception ex)
{
    if (logger != null)
    {
        logger.Error(ex, "Program terminated unexpectedly ({applicationContext})!", AppName);
    }

    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    LogManager.Shutdown();
}

public partial class Program
{
    public static string Namespace = typeof(Startup).Namespace;
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.') + 1);
}