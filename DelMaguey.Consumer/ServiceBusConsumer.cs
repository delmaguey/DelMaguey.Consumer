using Microsoft.EntityFrameworkCore;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
//using DelMaguey.Consumer.Data;
using DelMaguey.Consumer.Models;

namespace DelMaguey.Consumer
{
    public class ServiceBusConsumer : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ServiceBusConsumer> _logger;
        
        ServiceBusReceiver receiver;

        private readonly string topic = "orders-topic";
        private readonly string _queueName = "transactions";
        private readonly string subscriptionName = "dantest";

        private const int MaxRetryAttempsts = 1;



        public ServiceBusConsumer(ServiceBusClient client, IServiceScopeFactory scopeFactory, ILogger<ServiceBusConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            receiver = client.CreateReceiver(_queueName, subscriptionName);


            _processor = client.CreateProcessor(_queueName, subscriptionName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 5,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5),
            });

            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;
        }


        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = message.Body.ToString();


            try
            {
                _logger.LogInformation($"Processing message: {message.MessageId}");

                var dto = JsonSerializer.Deserialize<Transaction>(body);

                

                await ExecuteWithRetryAsync(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();

                    var entity = new Transaction
                    {
                        Id = 1296675,
                        Category = "Order",
                        State = "Processed",
                        City = dto?.City ?? string.Empty,
                        Amt = dto?.Amt ?? 0,
                        Email = dto?.Email ?? string.Empty,
                        First = dto?.First ?? string.Empty,
                        Gender = dto?.Gender ?? string.Empty,
                        Job = dto?.Job ?? string.Empty,
                        TransNum = dto?.TransNum ?? string.Empty,
                        Last = dto?.Last ?? string.Empty,
                        Merchant = dto?.Merchant ?? string.Empty,
                        Street = dto?.Street ?? string.Empty,
                        TransDateTransTime = DateTime.UtcNow
                    };

                    dbContext.Transactions.Add(entity);

                    await dbContext.SaveChangesAsync();

                });



                await args.CompleteMessageAsync(message);

            }
            catch (Exception)
            {
                _logger.LogWarning($"Error processing message: {message.MessageId}");
            }
        }



        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "ServiceBus processing error. Entity: {entity}, Operation: {op}",
                             args.EntityPath, args.ErrorSource);
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Service Bus Consumer...");
            await _processor.StartProcessingAsync(stoppingToken);



            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    if (_logger.IsEnabled(LogLevel.Information))
            //    {
            //        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    }
            //    await Task.Delay(1000, stoppingToken);
            //}
        }

        private async Task ExecuteWithRetryAsync(Func<Task> action)
        {
            int retryCount = 0;
            while (retryCount < MaxRetryAttempsts)
            {
                try
                {
                    await action();
                    return; // Success, exit the method
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning($"Attempt {retryCount} failed: {ex.Message}");
                    if (retryCount >= MaxRetryAttempsts)
                    {
                        _logger.LogError($"All {MaxRetryAttempsts} attempts failed.");
                        throw; // Rethrow the exception after max attempts
                    }
                    await Task.Delay(2000); // Wait before retrying
                }
            }
        }
    }
}
