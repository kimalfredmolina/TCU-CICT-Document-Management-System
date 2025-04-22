using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Threading.Tasks;

namespace Document_Management_System.Pages.AdminPage
{
    public class AdminFilesAddModel : PageModel
    {
        [BindProperty]
        public string FileName { get; set; }

        [BindProperty]
        public string Categories { get; set; }

        [BindProperty]
        public string ContentType { get; set; }

        [BindProperty]
        public IFormFile FormFile { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || FormFile == null)
            {
                return Page();
            }

            // Create uploads directory if it doesn't exist
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Sanitize filename
            var fileName = Path.GetFileName(FormFile.FileName);
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save file
            using (var stream = System.IO.File.Create(filePath))
            {
                await FormFile.CopyToAsync(stream);
            }

            // Here you would typically save to database
            // await _documentService.AddDocumentAsync(FileName, Categories, ContentType, filePath);

            return RedirectToPage("./AdminFiles");
        }

    }
}
