using Akka.HealthCheck.Hosting;
using Akka.HealthCheck.Hosting.Web;
using Akka.Streams.SignalR;
using Akka.Streams.SignalR.AspNetCore.Internals;
using AkkaClusterWebApi.App.BackgroundWorkers;
using AkkaClusterWebApi.App.Configuration;
using AkkaClusterWebApi.App.Hubs;

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

/*
 * CONFIGURATION SOURCES
 */
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.WithAkkaHealthCheck(HealthCheckType.All);
builder.Services.ConfigureWebApiAkka(builder.Configuration, (akkaConfigurationBuilder, serviceProvider) =>
{
    // we configure instrumentation separately from the internals of the ActorSystem
    akkaConfigurationBuilder.ConfigurePetabridgeCmd();
    akkaConfigurationBuilder.WithWebHealthCheck(serviceProvider);
}); 

// add signal r integration 

builder.Services.AddSingleton<IStreamDispatcher, DefaultStreamDispatcher>();
builder.Services.AddSingleton<IRemoteDeviceHubService, RemoteDeviceHubService>();
builder.Services.AddSingleton<HubService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = Int32.MaxValue;
});
builder.Services.AddSingleton<IConnectPropagator, StreamBackgroundWorker>();
builder.Services.AddSingleton<IEventPublisher, StreamBackgroundWorker>();
builder.Services.AddHostedService<StreamBackgroundWorker>(sp =>
    (StreamBackgroundWorker)sp.GetRequiredService<IConnectPropagator>());
// services.AddSingleton<IStreamDispatcher, DefaultStreamDispatcher>();
// Akka.Streams.SignalR.AkkaSignalRDependencyInjectionExtensions.AddSignalRAkkaStream(builder.Services);
// builder.Services.AddSignalRAkkaStream();

var app = builder.Build();
app.UseCors(opt => opt.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
// Configure the HTTP request pipeline.
if (true || app.Environment.IsDevelopment() || app.Environment.EnvironmentName.Equals("Azure"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.MapAkkaHealthCheckRoutes(optionConfigure: (_, opt) =>
{
    // Use a custom response writer to output a json of all reported statuses
    opt.ResponseWriter = Helper.JsonResponseWriter;
}); // needed for Akka.HealthCheck
// app.UseAuthorization();

app.MapControllers();
// app.MapHub<RemoteDeviceHub>(RemoteDeviceHub.HubUrl);
app.MapHub<NonStreamHub>(NonStreamHub.HubUrl);
Console.WriteLine($"Application environment: {app.Environment.EnvironmentName}, application name: {app.Environment.ApplicationName}");
app.Run();