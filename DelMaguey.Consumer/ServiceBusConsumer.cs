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

        private readonly string topic = "orders-topic";

        private const int MaxRetryAttempsts = 3;


        public ServiceBusConsumer(ServiceBusClient client, IServiceScopeFactory scopeFactory, ILogger<ServiceBusConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            _processor = client.CreateProcessor(topic, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 5,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
            });

            _processor.ProcessMessageAsync += ProcessMessageAsync;
            //_processor.ProcessErrorAsync += ProcessErrorAsync;
        }


        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = message.Body.ToString();


            try
            {
                _logger.LogInformation($"Processing message: {message.MessageId}");

                var dto = JsonSerializer.Deserialize<Transaction>(body);

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();

                await ExecuteWithRetryAsync(async () =>
                {
                    var entity = new Transaction
                    {
                        TransId = dto?.TransId ?? 0,
                        Category = "Order",
                        State = "Processed",
                        Amt = dto?.Amt ?? 0,
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
