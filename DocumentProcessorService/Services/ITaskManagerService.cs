using DocumentProcessorDB.Models;
using DocumentProcessorService.Services.DTOs;

namespace DocumentProcessorService.Services
{
    public interface ITaskManagerService
    {
        Task<TaskManager> SaveTaskManagerInfo(TaskRequest request);

        Task<TaskManager> GetTaskManagerById(long id);

        Task<List<TaskManager>> GetTaskManagerInfo();
    }
}
