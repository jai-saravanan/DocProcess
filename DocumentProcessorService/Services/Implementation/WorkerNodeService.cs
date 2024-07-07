using DocumentProcessorDB;
using DocumentProcessorDB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentProcessorService.Services.Implementation
{
    public class WorkerNodeService : IWorkerNodeService
    {
        private readonly DocumentProcessorContext _documentProcessorContext;
        public WorkerNodeService(DocumentProcessorContext documentProcessorContext)
        {
            _documentProcessorContext = documentProcessorContext;
        }

        public WorkerNode SaveWorkerNodeInfo(string folderName)
        {
            WorkerNode workerNode = new WorkerNode();
            try
            {
                workerNode.WorkerID = Guid.NewGuid();
                workerNode.WorkingFolderName = folderName;
                workerNode.TaskAssignedDateTime = DateTime.Now;
                workerNode.LastActiveDateTime = DateTime.Now;
                workerNode.Status = (byte)Status.NotStarted;
                _documentProcessorContext.WorkerNode.Add(workerNode);
                _documentProcessorContext.SaveChanges();
                return workerNode;
            }
            catch (Exception Ex)
            {
                return null;
            }
            
        }

        public FolderDetails SaveFolderDetails(Guid workerId, string sourceSubFolderName)
        {
            // insert folder details
            FolderDetails folderDetails = new FolderDetails();
            try
            {
                folderDetails.FolderId = Guid.NewGuid();
                folderDetails.WorkerId = workerId;
                folderDetails.SourceSubFolderName = sourceSubFolderName;
                folderDetails.Status = (byte)Status.NotStarted;
                _documentProcessorContext.FolderDetails.Add(folderDetails);
                _documentProcessorContext.SaveChanges();
                return folderDetails;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public FileDetails SaveFileDetails(Guid folderId, Status status, string filePath, string errorMessage)
        {
            FileDetails fileDetails = new FileDetails();
            try
            {
                fileDetails.FileId = Guid.NewGuid();
                fileDetails.FolderDetailsId = folderId;
                fileDetails.SourceFileName = Path.GetFileName(filePath);
                fileDetails.Status = (byte)status;
                fileDetails.ErrorMessage = errorMessage;
                _documentProcessorContext.FileDetails.Add(fileDetails);
                _documentProcessorContext.SaveChanges();
                return fileDetails;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public FolderDetails UpdateFolderStatus(Guid folderId, string destinationFolderName, string mergedFileName)
        {
            try
            {
                // update destination folder & pdf filepath
                var folderUpdate = _documentProcessorContext.FolderDetails.FirstOrDefault(x => x.FolderId == folderId);
                folderUpdate.Status = (byte)Status.Completed;
                folderUpdate.DestinationSubFolderName = destinationFolderName;
                folderUpdate.MergedFileName = mergedFileName;
                _documentProcessorContext.SaveChanges();
                return folderUpdate;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
