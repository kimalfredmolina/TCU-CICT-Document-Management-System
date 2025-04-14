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
using System.Reflection.Metadata;

namespace Document_Management_System.Pages.AdminPage
{
    public class AdminFilesModel : PageModel
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly AppDbContext _dbContext;

        public AdminFilesModel(IWebHostEnvironment hostEnvironment, AppDbContext dbContext)
        {
            _hostEnvironment = hostEnvironment;
            _dbContext = dbContext;
        }

        // Specify the namespace explicitly to resolve ambiguity
        public List<Document_Management_System.Data.Document> Documents { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Fetch data from the Documents table
            Documents = await _dbContext.Documents.ToListAsync();
        }


        // Method to handle GET requests
        //public void OnGet()
        //{
        //    // Initialization logic (if needed)
        //}

        // Method to handle file uploads
        [HttpPost]
        public async Task<IActionResult> OnPostUploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Define the folder to save the file
            string uploadsFolder = Path.Combine(_hostEnvironment.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate a unique filename to avoid overwriting
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new JsonResult(new { FileName = fileName, FilePath = filePath });
        }

        // Method to handle document edits
        [HttpPost]
        public IActionResult OnPostEditDocument(string id, string name, string categories, string contentType)
        {
            // Logic to update the document in the database or storage
            // For now, we'll just return a success response
            return new JsonResult(new { Success = true, Message = "Document updated successfully." });
        }

        // Method to handle document deletions
        [HttpPost]
        public IActionResult OnPostDeleteDocument(string id)
        {
            // Logic to delete the document from the database or storage
            // For now, we'll just return a success response
            return new JsonResult(new { Success = true, Message = "Document deleted successfully." });
        }
    }
}