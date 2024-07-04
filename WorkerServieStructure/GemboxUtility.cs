using DocumentProcessorDB;
using DocumentProcessorDB.Models;
using GemBox.Document;
using GemBox.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerServie
{
    public class GemboxUtility
    {
        private DocumentProcessorContext _documentProcessorContext;
        public GemboxUtility(DocumentProcessorContext documentProcessorContext)
        {
            GemBox.Document.ComponentInfo.SetLicense(CommonConstants.LicenceKeyFree);
            GemBox.Pdf.ComponentInfo.SetLicense(CommonConstants.LicenceKeyFree);
            _documentProcessorContext = documentProcessorContext;
        }

        public void ConvertAndMergeFilesToPDF(string folderName, bool deleteSource = false)
        {
            WorkerNode workerNode = new WorkerNode();
            workerNode.WorkerID = Guid.NewGuid();
            workerNode.WorkingFolderName = folderName;
            workerNode.TaskAssignedDateTime = DateTime.Now;
            workerNode.LastActiveDateTime = DateTime.Now;
            workerNode.Status = 0;
            _documentProcessorContext.WorkerNode.Add(workerNode);
            _documentProcessorContext.SaveChanges();

            string sourceRoot = @"D:\Freelance\Harshitha\DocProcessFolder\SouceFolder";
            string targetRoot = @"D:\Freelance\Harshitha\DocProcessFolder\TargetFolder";
            string combinedFileName = "Combined.pdf";
            var subjectDirectory = Path.Combine(sourceRoot, folderName);

            if (Directory.Exists(subjectDirectory))
            {
                var foldersToCombine = Directory.EnumerateDirectories(subjectDirectory);
                foreach (var folderToCombine in foldersToCombine)
                {
                    try
                    {
                        var pathParts = folderToCombine.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                        // insert folder details
                        FolderDetails folderDetails = new FolderDetails();
                        folderDetails.FolderId = Guid.NewGuid();
                        folderDetails.WorkerId = workerNode.WorkerID;
                        folderDetails.SourceSubFolderName = pathParts[^1];
                        folderDetails.Status = 0;
                        _documentProcessorContext.FolderDetails.Add(folderDetails);
                        _documentProcessorContext.SaveChanges();


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

                        // update destination folder & pdf filepath
                        var folderUpdate = _documentProcessorContext.FolderDetails.FirstOrDefault(x => x.FolderId == folderDetails.FolderId);
                        folderUpdate.Status = 1;
                        folderUpdate.DestinationSubFolderName = pathParts[^1];
                        _documentProcessorContext.SaveChanges();
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

        public void ConvertNonPdfFilesToPDF(string sourceDirectory, Guid folderId)
        {
            var nonPdfFiles = Directory.EnumerateFiles(sourceDirectory)
                                        .Where(x => !x.EndsWith(".pdf"))
                                        .ToList();
            foreach (var nonPdfFile in nonPdfFiles)
            {
                byte status = 0;
                string errorMessage = null;
                if (!IsFileEmpty(nonPdfFile))
                {
                    try
                    {
                        var outputFileName = nonPdfFile.Split(".")[0] + ".pdf";
                        DocumentModel document = DocumentModel.Load(nonPdfFile);
                        document.Save(outputFileName);
                        status = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.Message;
                    }
                }
                else
                {
                    status = 2;
                    errorMessage = "file is empty";
                }
                FileDetails fileDetails = new FileDetails();
                fileDetails.FileId = Guid.NewGuid();
                fileDetails.FolderDetailsId = folderId;
                fileDetails.SourceFileName = Path.GetFileName(nonPdfFile);
                fileDetails.Status = status;
                fileDetails.ErrorMessage = errorMessage;
                _documentProcessorContext.FileDetails.Add(fileDetails);
                _documentProcessorContext.SaveChanges();


            }
        }

        public bool IsFileEmpty(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length == 0;
        }
    }
}
