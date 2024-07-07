using System.Diagnostics;
using AkaClusterConsole;
using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Remote.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Cluster.Sharding;
using Petabridge.Cmd.Host;
using Petabridge.Cmd.Remote;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

var host = new HostBuilder()
    .ConfigureHostConfiguration(builder =>
        builder.AddEnvironmentVariables()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional:true))
    .ConfigureServices((hostContext, services) =>
    {
        var akkaConfig = hostContext.Configuration.GetRequiredSection(nameof(AkkaClusterConfig))
            .Get<AkkaClusterConfig>();
        services.AddLogging();
        services.AddAkka(akkaConfig.ActorSystemName, (builder, provider) =>
        {
            Debug.Assert(akkaConfig.Port != null, "akkaConfig.Port != null");
            builder.AddHoconFile("app.conf", HoconAddMode.Prepend)
                .WithRemoting(akkaConfig.Hostname, akkaConfig.Port.Value)
                .WithClustering(new ClusterOptions()
                {
                    Roles = akkaConfig.Roles,
                    SeedNodes = akkaConfig.SeedNodes
                })
                .WithActors((system, registry) =>
                {
                    var listener = system.ActorOf(Props.Create(() => new SimpleClusterEventsListener()), "simpleclusterlistener");
                    var pubSubSubscriber = system.ActorOf(Props.Create(() => new DistributedPubSubSubscriber()),
                        "framestopicsubscriber");
                    registry.Register<SimpleClusterEventsListener>(listener);
                    registry.Register<DistributedPubSubSubscriber>(pubSubSubscriber);
                }) 
                .WithShardRegion<Applicant>("Applicant", (s, r, dr) =>
                {
                    return (str) => Props.Create(() => new Applicant());
                }, new ApplicantShardMessageRouter(100), new ShardOptions(){})
                .WithSingleton<MyClusterSingleton>("simplesingleton", (system,registry,resolver) => MyClusterSingleton.Props, new ClusterSingletonOptions(){ Role = "pricing-engine"})
                .AddPetabridgeCmd(cmd =>
                {
                    cmd.RegisterCommandPalette(new RemoteCommands());
                    cmd.RegisterCommandPalette(ClusterCommands.Instance);

                    // sharding commands, although the app isn't configured to host any by default
                    cmd.RegisterCommandPalette(ClusterShardingCommands.Instance);
                });
        });
    })
    .ConfigureLogging((hostContext, configLogging) => { configLogging.AddConsole(); })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();