using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerServieStructure.DTOS
{
    public class TaskRequest
    {
        public string FolderNameToCombine { get; set; }

        public string SourceFolderName { get; set; }

        public string DestinationFolderName { get; set; }
    }
}
