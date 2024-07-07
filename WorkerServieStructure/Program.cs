
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using DocumentProcessorDB;
using DocumentProcessorDB.Models;
using WorkerServieStructure.DTOS;
using WorkerServie;
using System;
using DocumentProcessorService.Services;
using DocumentProcessorService.Services.Implementation;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();


        var factory = new ConnectionFactory() { Uri = new Uri("amqp://guest:guest@localhost:5672") };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "task_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);


        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var task = JsonSerializer.Deserialize<TaskRequest>(message);
            Console.WriteLine($"Received task: {task.FolderNameToCombine}. Merging all the files in the folder...");
            // Create a scope to resolve scoped services
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var app = services.GetRequiredService<GemboxUtility>();
                app.ConvertAndMergeFilesToPDF(task.FolderNameToCombine);
            }



            // Wait for 10s to simulate a long task
            Thread.Sleep(5000);
            Console.WriteLine("All files merged successfully!");
            Console.WriteLine("Waiting for another task...");
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        channel.BasicConsume(queue: "task_queue",
            autoAck: false,
            consumer: consumer);

        Console.WriteLine("Worker is waiting for messages. To exit press CTRL+C");
        Console.ReadKey();
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<DocumentProcessorContext>(options =>
                        options.UseSqlServer(context.Configuration.GetConnectionString("DocumentProcessingContext")));

                    services.AddScoped<IWorkerNodeService, WorkerNodeService>();
                    services.AddScoped<ITaskManagerService, TaskManagerService>();
                    services.AddScoped<GemboxUtility>();
                });
}


