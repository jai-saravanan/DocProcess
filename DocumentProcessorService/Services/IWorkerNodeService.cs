using DocumentProcessorDB.Models;
using DocumentProcessorService.Services.DTOs;

namespace DocumentProcessorService.Services
{
    public interface IWorkerNodeService
    {
        WorkerNode SaveWorkerNodeInfo(TaskRequest taskRequest);

        FolderDetails SaveFolderDetails(Guid workerId, string sourceSubFolderName);
        FileDetails SaveFileDetails(Guid folderId, Status status, string fileName, string errorMessage);

        FolderDetails UpdateFolderStatus(Guid folderId, string destinationFolderName, string mergedFileName);
    }
}
