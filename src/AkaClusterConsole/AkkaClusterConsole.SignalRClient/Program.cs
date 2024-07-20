using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace AkkaClusterConsole.SignalRClient;

class Program
{
    static async Task<int> Main(string[] args)
    {
        HubConnection _hubConnection = null;
        try
        {
            _hubConnection = new HubConnectionBuilder().WithUrl("http://localhost:5000/hubs/nonstream/devicehub")
                .ConfigureLogging(b =>
                {
                    b.SetMinimumLevel(LogLevel.Debug);
                })
                .WithAutomaticReconnect().Build();
            _hubConnection.On<object>("Received", HandleAsync);
            // _hubConnection.ON
            // _hubConnection.HandshakeTimeout = TimeSpan.FromMicroseconds(30);
            // _hubConnection.
            await _hubConnection.StartAsync();
            Console.WriteLine("hub started: ...");
            
            /*await foreach (object next in _hubConnection.StreamAsync<object>("Received"))
            {
                Console.WriteLine($"received object: {next}");
            }*/
            Console.ReadLine();

        }
        catch (Exception e)
        {
            Console.WriteLine($"error running signalr client: {e}");
            return -1;
        }
        finally
        {
            _hubConnection?.StopAsync();
            await _hubConnection.DisposeAsync();
        }

        return 0;
    }

    private static  Task HandleAsync(object next)
    {
        Console.WriteLine($"received object via handler: {next}");
        return Task.CompletedTask;
    }
}