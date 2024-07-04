using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentProcessorDB.Models
{
    public class FileDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid FileId { get; set; }

        [ForeignKey(nameof(FolderDetails))]
        public Guid FolderDetailsId { get; set; }

        public string SourceFileName { get; set; }

        public byte Status { get; set; }

        public string? ErrorMessage { get; set; }

        public FolderDetails FolderDetails { get; set; }
    }
}
