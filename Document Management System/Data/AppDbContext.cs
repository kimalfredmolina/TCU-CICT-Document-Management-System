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

        public DbSet<Document> Documents { get; set; }
        public DbSet<AssignTask> AssignTask { get; set; }
        public DbSet<Category> Categories { get; set; } 
        public DbSet<Area> Areas { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<YearFolder> YearFolders { get; set; }

        // For adding an assign user to a folder maximum of 3 users
        public DbSet<FolderAccess> FolderAccess { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FolderAccess>(entity =>
            {
                entity.HasOne(f => f.Category)
                      .WithMany()
                      .HasForeignKey(f => f.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.User)
                      .WithMany()
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.AssignedByUser)
                      .WithMany()
                      .HasForeignKey(f => f.AssignedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AssignTask>(entity =>
            {
                entity.HasOne(a => a.Category)
                      .WithMany()
                      .HasForeignKey(a => a.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.User)
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(a => a.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }

    public class Document
    {
        public int Id { get; set; }
        public string Filename { get; set; }
        public long? Filesize { get; set; }
        public string FileType { get; set; }
        public int? CategoryId { get; set; }
        public string UploadedBy { get; set; }
        public DateTime? UploadedDate { get; set; }
        public string ContentType { get; set; }
        public byte[]? FileData { get; set; }
        public string FolderPath { get; set; }
    }
}