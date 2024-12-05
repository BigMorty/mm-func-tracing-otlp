using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues;
// using Azure.Messaging.ServiceBus;

namespace BigMorty.Functions
{
    public class HttpTrigger1
    {
        private readonly ILogger<HttpTrigger1> _logger;
        private readonly QueueClient _queueClient;
        
        //  private readonly ServiceBusClient _serviceBusClient;
        //  private readonly ServiceBusSender _serviceBusSender;

        public HttpTrigger1(ILogger<HttpTrigger1> logger)
        {
            _logger = logger;

            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")!;
            _queueClient = new QueueClient(storageConnectionString, "myqueue"); // Replace "myqueue" with your queue name

            // string serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString")!;
            // _serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
            // _serviceBusSender = _serviceBusClient.CreateSender("mysbqueue"); // Replace "mysbqueue" with your queue name
        }

        [Function("HttpTrigger1")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")]
          HttpRequest req,
          string name)
        {
            _logger.LogInformation("INFO - C# HTTP trigger function processed a request.");
            _logger.LogWarning("WARNING - C# HTTP trigger function processed a request.");

            var returnValue = string.IsNullOrEmpty(name)
                ? "Hello, World."
                : $"Hello, {name}.";

            if (returnValue == "Hello, BigMorty.")
            {
                _logger.LogError("ERROR - C# HTTP trigger found BigMorty in name, erroring with teapot 418");
                return new StatusCodeResult(418); // 418 I'm a teapot
            }
            
            if (returnValue == "Hello, Mike.")
            {
                _logger.LogError("ERROR - C# HTTP trigger found Mike in name, erroring out with bad request 400");
                return new StatusCodeResult(400); // 400 Bad request
            }

            if (returnValue == "Hello, Morton.")
            {
                throw new InvalidOperationException("Just testing throwing exception");
            }

            // Write a message to the Azure Storage Queue
            await _queueClient.CreateIfNotExistsAsync();
            if (await _queueClient.ExistsAsync())
            {
                _logger.LogInformation("INFO - Azure Storage Queue exists, sending message");
                await _queueClient.SendMessageAsync("This is a message to the storage queue");
            }

            // Write a message to the Azure Service Bus Queue
            // _logger.LogInformation("INFO - Sending message to Azure Service Bus queue");
            // ServiceBusMessage message = new ServiceBusMessage("This is a message to the Service Bus queue");
            // await _serviceBusSender.SendMessageAsync(message);

            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}