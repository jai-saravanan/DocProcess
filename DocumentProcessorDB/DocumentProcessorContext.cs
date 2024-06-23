using DocumentProcessorDB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentProcessorDB
{
    public class DocumentProcessorContext : DbContext
    {
        public DocumentProcessorContext(DbContextOptions<DocumentProcessorContext> options) : base(options)
        {
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Server=localhost\\sqltest,1401;Database=DocumentProcessing;User Id=sa;Password=Saran@123;TrustServerCertificate=True;");
        //}

        public DbSet<TaskManager> TaskManager { get; set; } = null!;
        public DbSet<WorkerNode> WorkerNode { get; set; } = null!;
    }
}
