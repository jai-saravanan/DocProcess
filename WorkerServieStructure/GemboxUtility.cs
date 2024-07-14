using DocumentProcessorDB;
using DocumentProcessorDB.Models;
using DocumentProcessorService;
using DocumentProcessorService.Services;
using DocumentProcessorService.Services.DTOs;
using GemBox.Document;
using GemBox.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json;

namespace WorkerServie
{
    public class GemboxUtility
    {
        private readonly ILogger<GemboxUtility> _logger;
        private IWorkerNodeService _workerNodeService;
        public GemboxUtility(IWorkerNodeService workerNodeService, ILogger<GemboxUtility> logger)
        {
            GemBox.Document.ComponentInfo.SetLicense(CommonConstants.LicenceKeyFree);
            GemBox.Pdf.ComponentInfo.SetLicense(CommonConstants.LicenceKeyFree);
            _workerNodeService = workerNodeService;
            _logger = logger;
        }


        public void ConvertAndMergeFilesToPDF(TaskRequest taskRequest, bool deleteSource = false)
        {
            _logger.LogInformation("File processing started for : " + JsonSerializer.Serialize(taskRequest));
            if (taskRequest == null || string.IsNullOrWhiteSpace(taskRequest.FolderNameToCombine) || string.IsNullOrWhiteSpace(taskRequest.FolderNameToCombine) ||
                string.IsNullOrWhiteSpace(taskRequest.FolderNameToCombine))
            {
                _logger.LogInformation("Request payload doesn't have valid information");
                return;
            }
            var workerNode = _workerNodeService.SaveWorkerNodeInfo(taskRequest);


            string sourceRoot = @"D:\Freelance\Harshitha\DocProcessFolder\" + taskRequest.SourceFolderName;
            string targetRoot = @"D:\Freelance\Harshitha\DocProcessFolder\" + taskRequest.DestinationFolderName;
            var subjectDirectory = Path.Combine(sourceRoot, taskRequest.FolderNameToCombine);

            if (Directory.Exists(subjectDirectory))
            {
                var foldersToCombine = Directory.EnumerateDirectories(subjectDirectory);
                if (foldersToCombine == null || !foldersToCombine.Any())
                {
                    _logger.LogInformation("No folders or file found inside the specific folder. Folder name: " + taskRequest, foldersToCombine);
                    return;
                }

                foreach (var folderToCombine in foldersToCombine)
                {
                    try
                    {
                        var pathParts = folderToCombine.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                        var folderDetails = _workerNodeService.SaveFolderDetails(workerNode.WorkerID, pathParts[^1]);
                        var combinedFolder = Path.Combine(targetRoot, pathParts[^2], pathParts[^1]);

                        Directory.CreateDirectory(combinedFolder);

                        ConvertNonPdfFilesToPDF(folderToCombine, combinedFolder, folderDetails.FolderId, taskRequest.MaxPageCount);

                        _workerNodeService.UpdateFolderStatus(folderDetails.FolderId, pathParts[^1], "merged.pdf");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Getting error while processing the document. Error: " + JsonSerializer.Serialize(ex));
                    }

                }

                if (deleteSource)
                {
                    Directory.Delete(subjectDirectory, true);
                }
            }
            else
            {
                _logger.LogInformation("Folder name not exist");
                return;
            }
        }

        public async void ConvertNonPdfFilesToPDF(string sourceDirectory, string combinedFolder, Guid folderId, int maxPageCount)
        {
            var nonPdfFiles = Directory.EnumerateFiles(sourceDirectory)
                                        .Where(x => !x.EndsWith(".pdf"))
                                        .ToList();

            if (nonPdfFiles == null || !nonPdfFiles.Any())
            {
                _logger.LogInformation("No non pdf files found inside the directory. directory name: " + sourceDirectory);
                return;
            }

            var combinedDocument = new DocumentModel();
            foreach (var filePath in nonPdfFiles)
            {
                var status = Status.NotStarted;
                string errorMessage = null;
                if (!IsFileEmpty(filePath))
                {
                    // Load each Word document.
                    var document = DocumentModel.Load(filePath);

                    // Import the content of the loaded document into the combined document.
                    combinedDocument.Content.End.InsertRange(document.Content);
                    status = Status.Completed;
                }
                else
                {
                    _logger.LogInformation("File is empty. File path: " + filePath);
                    status = Status.Error;
                    errorMessage = "file is empty";
                }
                _workerNodeService.SaveFileDetails(folderId, status, filePath, errorMessage);
            }
            var paginator = combinedDocument.GetPaginator();
            if(paginator.Pages == null || !paginator.Pages.Any())
            {
                _logger.LogInformation("No pdf file created");
            }
            else
            {
                // take 100 pages or if pdf file contains only 2, then it will take 2 page
                paginator.GetRange(0, Math.Min(paginator.Pages.Count, maxPageCount)).Save(combinedFolder + "\\merged.pdf");
            }
            

        }

        public bool IsFileEmpty(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length == 0;
        }
    }
}
