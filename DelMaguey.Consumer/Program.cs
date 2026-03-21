using Azure.Messaging.ServiceBus;
using DelMaguey.Consumer;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ServiceBusClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("FinanceConnection");
    return new ServiceBusClient(connectionString);
});

builder.Services.AddHostedService<ServiceBusConsumer>();

builder.Configuration.GetConnectionString("ConnectionsStrings:FinanceConnection");



var host = builder.Build();
host.Run();
