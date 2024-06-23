using DocumentProcessorAPI.DTOs;
using DocumentProcessorDB;
using DocumentProcessorDB.Models;
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
        private readonly DocumentProcessorContext _context;
        private readonly IConnectionFactory _connectionFactory;

        public DocumentProcessorController(DocumentProcessorContext context, IConnectionFactory connectionFactory)
        {
            _context = context;
            _connectionFactory = connectionFactory;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskManager>>> GetTaskManager()
        {
            return await _context.TaskManager.Where(x => x.ReleaseOn == null).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskManager>> GetTaskManager(long id)
        {
            var taskManager = await _context.TaskManager.FindAsync(id);

            if (taskManager == null)
            {
                return NotFound();
            }

            return taskManager;
        }

        [HttpPost]
        public async Task<ActionResult<TaskManager>> PostTaskManager([FromBody] TaskRequest request)
        {
            var taskManager = new TaskManager
            {
                CaseNumber = request.FolderNameToCombine,
                LockedBy = "Admin",
                CreatedBy = "Admin",
                CreatedOn = DateTime.Now,

            };

            _context.TaskManager.Add(taskManager);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest("Requested folder is already in queue.");
            }

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



