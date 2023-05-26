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


using System.Collections;
using System.Collections.Specialized;
using System.Net;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Hosting;

using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

using static Yarp.ReverseProxy.Transforms.PathStringTransform;

namespace ASC.ReversyProxy;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;

    public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddReverseProxy()
                .LoadFromMemory()
                .AddTransforms(builderContext =>
                {
                    builderContext.AddRequestTransform(async transformContext =>
                    {
                        transformContext.ProxyRequest.Headers.Add("X-REWRITER-URL", $"{transformContext.HttpContext.Request.Scheme}://{transformContext.HttpContext.Request.Host}");
                    });
                });

        services.AddHttpForwarder();

        services.AddMemoryCache();
        services.AddHostedService<UpdateConfigManagerService>();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });


    }

    public void Configure(IApplicationBuilder app,
                          IHttpForwarder httpForwarder,
                          ILogger<Startup> logger)
    {
        var httpMessageInvoker = new HttpMessageInvoker(new SocketsHttpHandler()
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false
        });

        bool enableDirectForwarding = _configuration.GetValue<bool>("core:enable-direct-forwarding");

        app.UseForwardedHeaders();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            if (enableDirectForwarding)
            {
                endpoints.Map("/http-poll/{**any}", async httpContext =>
                            await DefaultDelegateRequest(httpForwarder, httpContext, "http://localhost:5280/http-poll/", httpMessageInvoker, new ListDictionary(), logger));

                endpoints.Map("/socketio/{**any}", async httpContext =>
                            await DefaultDelegateRequest(httpForwarder, httpContext, "http://localhost:9899/", httpMessageInvoker, new ListDictionary
                            {
                            {"X-REWRITER-URL-INTERNAL", $"http://{httpContext.Request.Host}" }
                            }, logger));

                endpoints.Map("/caldav/{**any}", async httpContext =>
                            await DefaultDelegateRequest(httpForwarder, httpContext, "http://localhost:5232/", httpMessageInvoker, new ListDictionary
                            {
                            {"X-SCRIPT-NAME", "/caldav" }
                            }, logger));

                endpoints.Map("/cardav/{**any}", async httpContext =>
                        await DefaultDelegateRequest(httpForwarder, httpContext, "http://localhost:5232/", httpMessageInvoker, new ListDictionary
                        {
                                    {"X-SCRIPT-NAME", "/cardav" }
                        }, logger));

                endpoints.Map("/webdav/{**any}", async httpContext =>
                            await DefaultDelegateRequest(httpForwarder, httpContext, "http://localhost:9889/", httpMessageInvoker, new ListDictionary(), logger));

                endpoints.Map("/sso/{**any}", async httpContext =>
                            await DefaultDelegateRequest(httpForwarder, httpContext, "http://localhost:9834/", httpMessageInvoker, new ListDictionary(), logger));
            }

            endpoints.MapReverseProxy(proxyPipeline =>
            {
                proxyPipeline.UseMiddleware<FilterProxyDestinationsMiddleware>();
            });
        });
    }

    private async Task DefaultDelegateRequest(IHttpForwarder httpForwarder,
                                              HttpContext httpContext,
                                              string destinationPrefix,
                                              HttpMessageInvoker httpMessageInvoker,
                                              ListDictionary proxyRequestHeaders,
                                              ILogger<Startup> logger)
    {
        var proxyRequestPath = httpContext.Request.Path;

        if (proxyRequestPath.HasValue)
        {
            var pathPrefixToRemove = new PathString("/" + proxyRequestPath.Value.Split("/", System.StringSplitOptions.RemoveEmptyEntries)[0]);
            var requestContext = new RequestTransformContext() { Path = proxyRequestPath };
            var pathTransform = new PathStringTransform(PathTransformMode.RemovePrefix, pathPrefixToRemove);

            await pathTransform.ApplyAsync(requestContext);

            proxyRequestPath = requestContext.Path;
        }

        var error = await httpForwarder.SendAsync(httpContext, destinationPrefix, httpMessageInvoker, ForwarderRequestConfig.Empty,
        (httpContext, proxyRequest) =>
        {
            proxyRequest.Headers.Add("X-REWRITER-URL", $"{httpContext.Request.Scheme}://{httpContext.Request.Host}");

            foreach (DictionaryEntry header in proxyRequestHeaders)
            {
                proxyRequest.Headers.Add((string)header.Key, header.Value as string);
            }

            proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(destinationPrefix, proxyRequestPath, httpContext.Request.QueryString);

            return default;
        });

        // Check if the operation was successful
        if (error != ForwarderError.None)
        {
            var errorFeature = httpContext.GetForwarderErrorFeature();
            if (errorFeature != null)
            {
                var exception = errorFeature.Exception;

                if (exception != null)
                {
                    logger.LogError(exception, exception.Message);
                }
            }
        }

    }
}
