using DocumentProcessorDB;
using DocumentProcessorDB.Models;
using DocumentProcessorService.Services;
using DocumentProcessorService.Services.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;


namespace DocumentProcessorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentProcessorController : ControllerBase
    {
        private readonly ITaskManagerService _taskManagerService;
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<DocumentProcessorController> _logger;

        public DocumentProcessorController(ITaskManagerService taskManagerService, IConnectionFactory connectionFactory,
            ILogger<DocumentProcessorController> logger)
        {
            _taskManagerService = taskManagerService;
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskManager>>> GetTaskManager()
        {
            _logger.LogInformation("GetTaskManager() Invoked");
            return await _taskManagerService.GetTaskManagerInfo();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskManager>> GetTaskManager(long id)
        {
            var taskManager = await _taskManagerService.GetTaskManagerById(id);

            if (taskManager == null)
            {
                return NotFound();
            }

            return taskManager;
        }

        [HttpPost]
        public async Task<ActionResult<TaskManager>> PostTaskManager([FromBody] TaskRequest request)
        {
            var taskManager = await _taskManagerService.SaveTaskManagerInfo(request);

            var factory = _connectionFactory;
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "task_queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                                 routingKey: "task_queue",
                                 basicProperties: properties,
                                 body: body);

            return CreatedAtAction("GetTaskManager", new { id = taskManager.Id }, taskManager);
        }
    }
}



