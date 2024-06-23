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
        public GemboxUtility()
        {
            GemBox.Document.ComponentInfo.SetLicense(CommonConstants.LicenceKeyFree);
            GemBox.Pdf.ComponentInfo.SetLicense(CommonConstants.LicenceKeyFree);
        }

        public void ConvertAndMergeFilesToPDF(string folderName, bool deleteSource = false)
        {
            string sourceRoot = "SourceFolder";
            string targetRoot = "TargetFolder";
            string combinedFileName = "Combined.pdf";
            var subjectDirectory = Path.Combine(sourceRoot, folderName);
            if (Directory.Exists(subjectDirectory))
            {
                var foldersToCombine = Directory.EnumerateDirectories(subjectDirectory);
                foreach (var folderToCombine in foldersToCombine)
                {
                    ConvertNonPdfFilesToPDF(folderToCombine);

                    var pathParts = folderToCombine.Split(new char[] { '\\' });
                    var combinedFolder = Path.Combine(targetRoot, pathParts[1], pathParts[2]);
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
                }

                if (deleteSource)
                {
                    Directory.Delete(subjectDirectory, true);
                }
            }
        }

        public void ConvertNonPdfFilesToPDF(string sourceDirectory)
        {
            var nonPdfFiles = Directory.EnumerateFiles(sourceDirectory)
                                        .Where(x => !x.EndsWith(".pdf"))
                                        .ToList();
            foreach (var nonPdfFile in nonPdfFiles)
            {
                var outputFileName = nonPdfFile.Split(".")[0] + ".pdf";
                DocumentModel document = DocumentModel.Load(nonPdfFile);
                document.Save(outputFileName);
            }
        }
    }
}
