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
using System.Threading.Tasks;

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
            _logger.LogInformation("log started");
            var workerNode = _workerNodeService.SaveWorkerNodeInfo(taskRequest);

            string sourceRoot = @"D:\Freelance\Harshitha\DocProcessFolder\" + taskRequest.SourceFolderName;
            string targetRoot = @"D:\Freelance\Harshitha\DocProcessFolder\" + taskRequest.DestinationFolderName;
            string combinedFileName = "Combined.pdf";
            var subjectDirectory = Path.Combine(sourceRoot, taskRequest.FolderNameToCombine);

            if (Directory.Exists(subjectDirectory))
            {
                var foldersToCombine = Directory.EnumerateDirectories(subjectDirectory);
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
                    }

                }

                if (deleteSource)
                {
                    Directory.Delete(subjectDirectory, true);
                }
            }
        }

        public async void ConvertNonPdfFilesToPDF(string sourceDirectory, Guid folderId)
        {
            var nonPdfFiles = Directory.EnumerateFiles(sourceDirectory)
                                        .Where(x => !x.EndsWith(".pdf"))
                                        .ToList();
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
                        status = Status.Error;
                        errorMessage = ex.Message;
                    }
                }
                else
                {
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
