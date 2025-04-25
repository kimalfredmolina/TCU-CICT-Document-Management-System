using Document_Management_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Document_Management_System.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Add the missing DbSet for Documents  
        public DbSet<Document> Documents { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<YearFolder> YearFolders { get; set; }
    }
    public class Document
    {
        public int Id { get; set; } // Primary Key
        public string Filename { get; set; } // NOT NULL
        public long? Filesize { get; set; } // instead of int? nullable
        public string FileType { get; set; } // NOT NULL
        public int? CategoryId { get; set; } // Nullable
        public string UploadedBy { get; set; } // NOT NULL
        public DateTime? UploadedDate { get; set; } // Nullable
        public string ContentType { get; set; } // NOT NULL
        public byte[]? FileData { get; set; } //NULLABLE
        public string FolderPath { get; set; } // property to track which folder this document belongs to

    }


}