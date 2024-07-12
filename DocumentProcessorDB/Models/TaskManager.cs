using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentProcessorDB.Models
{
    public class TaskManager
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [MaxLength(250)]
        public string CaseNumber { get; set; }

        [MaxLength(250)]
        public string SourceFolderName { get; set; }

        [MaxLength(250)]
        public string DestinationFolderName { get; set; }

        [MaxLength(50)]
        public string LockedBy { get; set; }

        public int? SPID { get; set; }

        public DateTime? ReleaseOn { get; set; }

        public DateTime CreatedOn { get; set; }

        [MaxLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }
    }
}
