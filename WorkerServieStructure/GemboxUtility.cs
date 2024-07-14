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
            string combinedFileName = "Combined.pdf";
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


                        ConvertNonPdfFilesToPDF(folderToCombine, folderDetails.FolderId);


                        var combinedFolder = Path.Combine(targetRoot, pathParts[^2], pathParts[^1]);

                        Directory.CreateDirectory(combinedFolder);
                        var files = Directory.EnumerateFiles(folderToCombine)
                                             .Where(x => x.EndsWith(".pdf"))
                                             .OrderBy(x => x)
                                             .ToList();
                        _logger.LogInformation(string.Format($"Total pdf file count. Folder Name: {folderToCombine}, Files Count: {files?.Count()}"));
                        if(files == null || !files.Any())
                        {
                            _logger.LogInformation("No pdf files found. Folder Name: " + folderToCombine);
                            return;
                        }
                        if (files.Count() == 1)
                        {
                            File.Copy(files.First(),
                                      Path.Combine(combinedFolder, combinedFileName),
                                      true);
                        }
                        else if (files.Count() > 1)
                        {
                            using (var document = new PdfDocument())
                            {
                                int fileCounter = 0;
                                int chunkSize = 50;
                                foreach (var fileName in files)
                                {
                                    using (var source = PdfDocument.Load(fileName))
                                    {
                                        document.Pages.Kids.AddClone(source.Pages);
                                    }
                                    ++fileCounter;
                                    if (fileCounter % chunkSize == 0)
                                    {
                                        // Save the new pages that were added after the document was last saved
                                        document.Save();
                                        // Clear previously parsed pages and thus free memory necessary for further processing
                                        document.Unload();
                                    }
                                }
                                document.Save();
                            }
                        }

                        _workerNodeService.UpdateFolderStatus(folderDetails.FolderId, pathParts[^1], combinedFileName);
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

        public async void ConvertNonPdfFilesToPDF(string sourceDirectory, Guid folderId)
        {
            var nonPdfFiles = Directory.EnumerateFiles(sourceDirectory)
                                        .Where(x => !x.EndsWith(".pdf"))
                                        .ToList();
            if (nonPdfFiles == null || !nonPdfFiles.Any())
            {
                _logger.LogInformation("No non pdf files found inside the directory. directory name: " + sourceDirectory);
                return;
            }
            foreach (var nonPdfFile in nonPdfFiles)
            {
                var status = Status.NotStarted;
                string errorMessage = null;
                if (!IsFileEmpty(nonPdfFile))
                {
                    try
                    {
                        var outputFileName = nonPdfFile.Split(".")[0] + ".pdf";
                        DocumentModel document = DocumentModel.Load(nonPdfFile);
                        document.Save(outputFileName);
                        status = Status.Completed;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Getting error while converting pdf. Error: " + JsonSerializer.Serialize(ex));
                        status = Status.Error;
                        errorMessage = ex.Message;
                    }
                }
                else
                {
                    _logger.LogInformation("File is empty. File path: " + nonPdfFile);
                    status = Status.Error;
                    errorMessage = "file is empty";
                }
                _workerNodeService.SaveFileDetails(folderId, status, nonPdfFile, errorMessage);
            }
        }

        public bool IsFileEmpty(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length == 0;
        }
    }
}
