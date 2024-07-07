using DocumentProcessorDB;
using DocumentProcessorDB.Models;
using DocumentProcessorService.Services.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentProcessorService.Services.Implementation
{
    public class TaskManagerService : ITaskManagerService
    {
        private readonly DocumentProcessorContext _documentProcessorContext;

        public TaskManagerService(DocumentProcessorContext documentProcessorContext)
        {
            _documentProcessorContext = documentProcessorContext;
        }

        public async Task<TaskManager> SaveTaskManagerInfo(TaskRequest request)
        {
            try
            {
                var taskManager = new TaskManager
                {
                    CaseNumber = request?.FolderNameToCombine,
                    LockedBy = "Admin",
                    CreatedBy = "Admin",
                    CreatedOn = DateTime.Now,

                };

                _documentProcessorContext.TaskManager.Add(taskManager);
                await _documentProcessorContext.SaveChangesAsync();
                return taskManager;
            }
            catch (Exception ex)
            {
                return null;
            }
            
        }

        public async Task<TaskManager> GetTaskManagerById(long id)
        {
            return await _documentProcessorContext.TaskManager.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<TaskManager>> GetTaskManagerInfo()
        {
            return await _documentProcessorContext.TaskManager.Where(x => x.ReleaseOn == null).ToListAsync();
        }
    }
}
