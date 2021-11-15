using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using Moq;
using Xunit;
using Piipan.QueryTool;

public class WebServerFixture : IAsyncLifetime, IDisposable
{
    private readonly IHost host;
    private IPlaywright playwright { get; set; }
    public IBrowser browser { get; private set; }
    public string BaseUrl { get; } = $"https://localhost:{GetRandomUnusedPort()}";

    public WebServerFixture()
    {
        host = Piipan.QueryTool.Program
            .CreateHostBuilder(null)
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseUrls(BaseUrl);
                // optional to set path to static file assets
                // coming from tests/Piipan.QueryTool.Tests/bin/Debug/netcoreapp3.1
                webBuilder.UseContentRoot("../../../../../src/Piipan.QueryTool");
            })
            .ConfigureServices(services => {
                // override any services
            })
            .Build();
    }

    public async Task InitializeAsync()
    {
        playwright = await Playwright.CreateAsync();
        browser = await playwright.Chromium.LaunchAsync();
        await host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await host.StopAsync();
        host?.Dispose();
        playwright?.Dispose();
    }

    public void Dispose()
    {
        host?.Dispose();
        playwright?.Dispose();
    }

    private static int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Any, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
