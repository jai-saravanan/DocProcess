using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentProcessorDB.Models
{
    public class WorkerNode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid WorkerID { get; set; }
        public string WorkingFolderName { get; set; }

        public string SourceFolderName { get; set; }

        public string DestinationFolderName { get; set; }

        public DateTime TaskAssignedDateTime { get; set; }
        public DateTime LastActiveDateTime { get; set; }

        public byte Status { get; set; }

        public ICollection<FolderDetails> FolderDetails { get; set; }

    }
}
