using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Document_Management_System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Document_Management_System.Models;

namespace Document_Management_System.Pages.AdminPage
{
    public class AdminFilesModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<Users> _userManager;

        public AdminFilesModel(AppDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<Users> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public List<Document> Documents { get; set; } = new List<Document>();

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FolderPath { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Category { get; set; }

        public async Task OnGetAsync()
        {
            // Start with an empty list
            Documents = new List<Document>();

            // Create a query to filter documents
            IQueryable<Document> query = _context.Documents;

            // Filter by folder path if provided
            if (!string.IsNullOrEmpty(FolderPath))
            {
                query = query.Where(d => d.FolderPath == FolderPath);
            }

            // Filter by category if provided
            if (!string.IsNullOrEmpty(Category))
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == Category);
                if (category != null)
                {
                    query = query.Where(d => d.CategoryId == category.CategoryId);
                }
            }
            // Filter by search query if provided
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(d =>
                    (d.Filename != null && d.Filename.Contains(SearchQuery)) ||
                    (d.FileType != null && d.FileType.Contains(SearchQuery)) ||
                    (d.ContentType != null && d.ContentType.Contains(SearchQuery)));
            }

            try
            {
                // Load the documents - execute the query only once
                Documents = await query.ToListAsync();
            }
            catch (Exception ex)
            {
                // Handle the error gracefully
                Documents = new List<Document>();
                TempData["ErrorMessage"] = "An error occurred while loading files. Please try again.";
                // You could log the actual error here
            }

            // If no documents found in the specified folder and no search is active, check if there are files in the filesystem
            if (!Documents.Any() && !string.IsNullOrEmpty(FolderPath) && string.IsNullOrEmpty(SearchQuery))
            {
                await ImportFilesFromFolder();
            }
        }

        private async Task ImportFilesFromFolder()
        {
            // Check if the folder exists in the filesystem
            string folderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage",
                FolderPath?.Replace("/", Path.DirectorySeparatorChar.ToString()) ?? "");
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            // Get all files in the directory
            var files = Directory.GetFiles(folderPath);
            if (!files.Any())
            {
                return;
            }

            // Get category information
            var categoryName = FolderPath.Split('/').FirstOrDefault();
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);

            // Import each file to the database
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var document = new Document
                {
                    Filename = fileInfo.Name,
                    Filesize = fileInfo.Length,
                    FileType = fileInfo.Extension,
                    CategoryId = category?.CategoryId,
                    UploadedBy = User.Identity.Name ?? "System",
                    UploadedDate = fileInfo.CreationTime,
                    ContentType = GetContentType(fileInfo.Extension),
                    FolderPath = FolderPath
                };

                _context.Documents.Add(document);
            }

            await _context.SaveChangesAsync();

            // Reload documents
            Documents = await _context.Documents.Where(d => d.FolderPath == FolderPath).ToListAsync();
        }

        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                _ => "application/octet-stream",
            };
        }

        // Handler for uploading a new file to the folder
        public async Task<IActionResult> OnPostUploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToPage(new { folderPath = FolderPath, category = Category });
            }

            // Make sure the folder exists
            string folderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", FolderPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Generate a unique filename to prevent overwriting
            string fileName = file.FileName;
            string filePath = Path.Combine(folderPath, fileName);

            // Save the file to the filesystem
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Get category information
            var categoryName = FolderPath.Split('/').FirstOrDefault();
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);

            // Create a document entry in the database
            var document = new Document
            {
                Filename = fileName,
                Filesize = file.Length,
                FileType = Path.GetExtension(fileName),
                CategoryId = category?.CategoryId,
                UploadedBy = User.Identity.Name ?? "System",
                UploadedDate = DateTime.Now,
                ContentType = file.ContentType,
                FolderPath = FolderPath
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "File uploaded successfully.";
            return RedirectToPage(new { folderPath = FolderPath, category = Category });
        }

        // Handler for deleting a file
        public async Task<IActionResult> OnPostDeleteFile(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Delete the file from the filesystem
            string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage",
                document.FolderPath?.Replace("/", Path.DirectorySeparatorChar.ToString()) ?? "",
                document.Filename ?? "");

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Remove from the database
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "File deleted successfully.";
            return RedirectToPage(new { folderPath = FolderPath, category = Category });
        }

        // Handler for downloading a file
        public async Task<IActionResult> OnGetDownloadFileAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage",
                document.FolderPath.Replace("/", Path.DirectorySeparatorChar.ToString()),
                document.Filename);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, document.ContentType, document.Filename);
        }

        public async Task<IActionResult> OnGetPreviewFileAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage",
                document.FolderPath.Replace("/", Path.DirectorySeparatorChar.ToString()),
                document.Filename);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // For text files, return the content
            if (document.ContentType.StartsWith("text/"))
            {
                string content = await System.IO.File.ReadAllTextAsync(filePath);
                return new JsonResult(new { success = true, content, type = "text" });
            }
            // For images, return the file as base64
            else if (document.ContentType.StartsWith("image/"))
            {
                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                string base64 = Convert.ToBase64String(fileBytes);
                return new JsonResult(new { success = true, content = base64, type = "image", contentType = document.ContentType });
            }
            // For PDFs, return the file path for embedding
            else if (document.ContentType == "application/pdf")
            {
                return new JsonResult(new { success = true, type = "pdf", filename = document.Filename, id = document.Id });
            }
            // For Office documents, use Google Docs Viewer or Office Online
            else if (document.ContentType.Contains("officedocument") ||
                     document.ContentType == "application/msword" ||
                     document.ContentType == "application/vnd.ms-excel" ||
                     document.ContentType == "application/vnd.ms-powerpoint")
            {
                // We'll use the document ID to generate a URL for the document viewer
                return new JsonResult(new
                {
                    success = true,
                    type = "office",
                    filename = document.Filename,
                    id = document.Id,
                    fileType = document.FileType
                });
            }
            // For other file types, return file info
            else
            {
                return new JsonResult(new { success = true, type = "other", filename = document.Filename });
            }
        }
    }
}
