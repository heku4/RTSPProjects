using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MockCamera.Options;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configurationBuilder =>
    {
        configurationBuilder
            .AddJsonFile("appsettings.json")
            .Build();
    })
    .ConfigureServices((builderContext, services) =>
    {
        services.AddSingleton<RtspListener>();

        var servereOptions = new ServerOptions();
        builderContext.Configuration
            .GetSection(nameof(ServerOptions))
            .Bind(servereOptions);
        
        services.AddSingleton(servereOptions);
    })
    .Build();

var listener = host.Services.GetRequiredService<RtspListener>();

var tokenSource = new CancellationTokenSource();
await listener.ExecuteAsync(tokenSource.Token);