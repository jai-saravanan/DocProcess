using DocumentProcessorDB;
using DocumentProcessorService.Services;
using DocumentProcessorService.Services.Implementation;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
builder.Host.UseNLog();


builder.Services.AddControllers();
builder.Services.AddDbContext<DocumentProcessorContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DocumentProcessingContext")));
builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory() { Uri = new Uri("amqp://guest:guest@localhost:5672") });
builder.Services.AddScoped<IWorkerNodeService,WorkerNodeService>();
builder.Services.AddScoped<ITaskManagerService,TaskManagerService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
