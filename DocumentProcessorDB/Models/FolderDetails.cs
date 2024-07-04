using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentProcessorDB.Models
{
    public class FolderDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid FolderId { get; set; }

        [ForeignKey(nameof(WorkerNode))]
        public Guid WorkerId { get; set; }

        public string SourceSubFolderName { get; set; }

        public string DestinationSubFolderName { get; set; }

        public string MergedFileName { get; set; }

        public byte Status { get; set; }

        public string ErrorMessage { get; set; }

        public ICollection<FileDetails> FileDetails { get; set; }

        public WorkerNode WorkerNode { get; set; }

    }
}
