
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

var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json");
var configuration = configurationBuilder.Build();
var connectionString = configuration.GetConnectionString("DocumentProcessingContext");
var optionsBuilder = new DbContextOptionsBuilder<DocumentProcessorContext>();
optionsBuilder.UseSqlServer(connectionString);
var workerContext = new DocumentProcessorContext(optionsBuilder.Options);

var factory = new ConnectionFactory() { Uri = new Uri("amqp://guest:guest@localhost:5672") };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(
    queue: "task_queue",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

// Worker will add an entry in database by generating GUID to save it in DB to distinguish between other worker nodes.
//var workerid = Guid.NewGuid();
//using (var workerContext = new DocumentProcessorContext(optionsBuilder.Options))
//{
//    var workerNode = new WorkerNode
//    {
//        WorkerID = workerid,
//        WorkingFolderName = "TEST_FOLDER",
//        TaskAssignedDateTime = DateTime.Now,
//        LastActiveDateTime = DateTime.Now,
//        Status = 0
//    };
//    workerContext.WorkerNode.Add(workerNode);
//    workerContext.SaveChanges();
//}

// End of worker node initialization

var consumer = new EventingBasicConsumer(channel);
consumer.Received += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var task = JsonSerializer.Deserialize<TaskRequest>(message);
    Console.WriteLine($"Received task: {task.FolderNameToCombine}. Merging all the files in the folder...");
    var utility = new GemboxUtility(workerContext);
    utility.ConvertAndMergeFilesToPDF(task.FolderNameToCombine);

    // After each operation is successful, worker will update some timestamp in the DB
    //using (var context = new DocumentProcessorContext(optionsBuilder.Options))
    //{
    //    var dbTask = await context.TaskManager.FirstOrDefaultAsync(x => x.CaseNumber == task.FolderNameToCombine);
    //    if (dbTask != null)
    //    {
    //        dbTask.ReleaseOn = DateTime.Now;
    //        dbTask.ModifiedOn = DateTime.Now;
    //        dbTask.ModifiedBy = "Admin";
    //        context.SaveChanges();
    //    }
    //}

    // Update worker node status
    //using (var context = new DocumentProcessorContext(optionsBuilder.Options))
    //{
    //    var workerNode = context.WorkerNode.FirstOrDefault(w => w.WorkerID == workerid);
    //    if (workerNode != null)
    //    {
    //        workerNode.LastActiveDateTime = DateTime.Now;
    //        workerNode.Status = 0;
    //        context.SaveChanges();
    //    }
    //}

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