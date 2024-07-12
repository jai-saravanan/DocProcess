namespace DocumentProcessorService.Services.DTOs
{
    public class TaskRequest
    {
        public string FolderNameToCombine { get; set; }

        public string SourceFolderName { get; set; }

        public string DestinationFolderName { get; set; }
    }
}
