using Azure.Messaging.ServiceBus;
using DelMaguey.Consumer;
using DelMaguey.Consumer.Models;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ServiceBusClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("ServiceBusConn");
    return new ServiceBusClient(connectionString);
});

// Program.cs


builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FinanceConnection")));

// or for background services consider:
//builder.Services.AddDbContextFactory<FinanceDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("FinanceConnection")));

builder.Services.AddHostedService<ServiceBusConsumer>();

builder.Configuration.GetConnectionString("ConnectionsStrings:FinanceConnection");



var host = builder.Build();
host.Run();
