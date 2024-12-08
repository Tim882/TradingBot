using Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Logging;
using TradingBot;

Console.Title = "Trading bot";

var endpointConfiguration = new EndpointConfiguration("Rabbit.TradingBot");
endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.EnableInstallers();
var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
transport.ConnectionString("host=localhost");
transport.UseDirectRoutingTopology(QueueType.Classic);

var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

await RunLoop(endpointInstance);

await endpointInstance.Stop().ConfigureAwait(false);

static async Task RunLoop(IEndpointInstance endpointInstance)
{
    while (true)
    {
        Console.WriteLine("Press 'P' to place an order, or 'Q' to quit.");
        var key = Console.ReadKey();
        Console.WriteLine();

        List<string> values = new List<string> { "SBER", "LKOH", "GAZP", "YDEX" };

        Random rnd = new Random();

        switch (key.Key)
        {
            case ConsoleKey.P:
                // Instantiate the command
                var positionEvent = new OpenedPosition
                {
                    Id = Guid.NewGuid().ToString(),
                    Ticker = values[rnd.Next(0, values.Count - 1)]
                };

                // Send the event
                Console.WriteLine($"Sending PlaceOrder command, OrderId = {positionEvent.Id}");
                await endpointInstance.Publish(positionEvent);

                break;

            case ConsoleKey.Q:
                return;

            default:
                Console.WriteLine("Unknown input. Please try again.");
                break;
        }
    }
}