using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Document_Management_System.Data;
using Document_Management_System.Models;

namespace Document_Management_System.Pages.AdminPage
{
    public class AdminFilesModel : PageModel
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly AppDbContext _dbContext;

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        public AdminFilesModel(IWebHostEnvironment hostEnvironment, AppDbContext dbContext)
        {
            _hostEnvironment = hostEnvironment;
            _dbContext = dbContext;
        }

        public List<Document> Documents { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Fetch data from the Documents table with search functionality
            var query = _dbContext.Documents.AsQueryable();

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(d =>
                    d.Filename.Contains(SearchQuery) ||
                    (d.ContentType != null && d.ContentType.Contains(SearchQuery)) ||
                    (d.FileType != null && d.FileType.Contains(SearchQuery)) ||
                    (d.UploadedBy != null && d.UploadedBy.Contains(SearchQuery)));
            }

            Documents = await query.ToListAsync();
        }

        public async Task<IActionResult> OnPostUploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".png", ".doc", ".docx" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file type. Only PDF, PNG, DOC, and DOCX files are allowed.");
            }

            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(uploadsFolder, fileName);
            string relativePath = Path.Combine("uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save to database
            var document = new Document
            {
                Filename = Path.GetFileNameWithoutExtension(file.FileName),
                FileType = fileExtension,
                ContentType = file.ContentType,
                Filesize = file.Length,
                UploadedDate = DateTime.Now,
                UploadedBy = User.Identity?.Name ?? "System"
            };

            await _dbContext.Documents.AddAsync(document);
            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                Success = true,
                FileName = document.Filename,
                DocumentId = document.Id
            });
        }

        public async Task<IActionResult> OnPostEditDocument(int id, string name, string categories, string contentType, IFormFile file)
        {
            var document = await _dbContext.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound("Document not found.");
            }

            // Update basic properties
            document.Filename = name;
            if (int.TryParse(categories, out var categoryId))
            {
                document.CategoryId = categoryId;
            }
            document.ContentType = contentType;

            // Handle file update if provided
            if (file != null && file.Length > 0)
            {
                // Validate new file type
                var allowedExtensions = new[] { ".pdf", ".png", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Only PDF, PNG, DOC, and DOCX files are allowed.");
                }

                // Update file properties
                document.FileType = fileExtension;
                document.ContentType = file.ContentType;
                document.Filesize = file.Length;
            }

            _dbContext.Documents.Update(document);
            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                Success = true,
                Message = "Document updated successfully."
            });
        }

        public async Task<IActionResult> OnPostDeleteDocument(int id)
        {
            var document = await _dbContext.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound("Document not found.");
            }

            _dbContext.Documents.Remove(document);
            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                Success = true,
                Message = "Document deleted successfully."
            });
        }
    }
}