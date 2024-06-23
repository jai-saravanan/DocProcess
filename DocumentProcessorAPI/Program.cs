using DocumentProcessorDB;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<DocumentProcessorContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DocumentProcessingContext")));
builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory() { Uri = new Uri("amqp://guest:guest@localhost:5672") });
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
