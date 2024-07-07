using DocumentProcessorDB.Models;

namespace DocumentProcessorService.Services
{
    public interface IWorkerNodeService
    {
        WorkerNode SaveWorkerNodeInfo(string folderName);

        FolderDetails SaveFolderDetails(Guid workerId, string sourceSubFolderName);
        FileDetails SaveFileDetails(Guid folderId, Status status, string fileName, string errorMessage);

        FolderDetails UpdateFolderStatus(Guid folderId, string destinationFolderName, string mergedFileName);
    }
}
