    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;
    using Document_Management_System.Models;
    using Document_Management_System.Data;
    using Microsoft.EntityFrameworkCore;

    namespace Document_Management_System.Pages.AdminPage
    {
        public class AdminFileFolderModel : PageModel
        {
            private readonly IWebHostEnvironment _webHostEnvironment;
            private readonly AppDbContext _context;

            public AdminFileFolderModel(IWebHostEnvironment webHostEnvironment, AppDbContext context)
            {
                _webHostEnvironment = webHostEnvironment;
                _context = context;
            }

            [BindProperty(SupportsGet = true)]
            public string CurrentCategory { get; set; }

            [BindProperty(SupportsGet = true)]
            public string FolderPath { get; set; }

            public string CurrentPath => string.IsNullOrEmpty(FolderPath) ? CurrentCategory : FolderPath;

            public List<FolderModel> CurrentFolders { get; set; } = new List<FolderModel>();
            public List<FileModel> CurrentFiles { get; set; } = new List<FileModel>();
            public List<BreadcrumbItem> BreadcrumbFolders { get; set; } = new List<BreadcrumbItem>();

            // Dictionary to store hierarchy of folders
            public Dictionary<string, List<FolderModel>> FolderHierarchy { get; set; } = new Dictionary<string, List<FolderModel>>();

        public async Task OnGetAsync()
        {
            // Create the FileStorage directory if it doesn't exist
            string fileStoragePath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage");
            if (!Directory.Exists(fileStoragePath))
            {
                Directory.CreateDirectory(fileStoragePath);
            }

            // Ensure categories exist in the database
            await EnsureCategoriesExistAsync();

            // Build the folder hierarchy from the database
            await BuildFolderHierarchy();

            if (!string.IsNullOrEmpty(CurrentCategory))
            {
                // Navigate to the selected folder
                string currentFolderPath = Path.Combine(fileStoragePath, CurrentPath);

                // Create breadcrumb items
                BuildBreadcrumbs();

                // Load folders in current directory
                LoadFolders(currentFolderPath);

                // Load files in current directory
                LoadFiles(currentFolderPath);
            }
        }

        private void BuildBreadcrumbs()
            {
                BreadcrumbFolders.Clear();

                if (string.IsNullOrEmpty(FolderPath) || FolderPath == CurrentCategory)
                    return;

                string[] parts = FolderPath.Split('/');
                string currentPath = CurrentCategory;

                for (int i = 1; i < parts.Length; i++)  // Start at 1 to skip the category
                {
                    currentPath += "/" + parts[i];
                    BreadcrumbFolders.Add(new BreadcrumbItem
                    {
                        Name = parts[i],
                        Path = currentPath
                    });
                }
            }

            private void LoadFolders(string path)
            {
                CurrentFolders.Clear();

                if (!Directory.Exists(path))
                    return;

                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                foreach (var dir in directoryInfo.GetDirectories())
                {
                    string relativePath = GetRelativePath(dir.FullName);
                    int itemCount = dir.GetFiles().Length + dir.GetDirectories().Length;

                    CurrentFolders.Add(new FolderModel
                    {
                        Name = dir.Name,
                        Path = relativePath,
                        Category = CurrentCategory,
                        ModifiedDate = dir.LastWriteTime,
                        ItemCount = itemCount
                    });
                }
            }

            private void LoadFiles(string path)
            {
                CurrentFiles.Clear();

                if (!Directory.Exists(path))
                    return;

                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                foreach (var file in directoryInfo.GetFiles())
                {
                    string fileId = Path.Combine(GetRelativePath(path), file.Name);

                    CurrentFiles.Add(new FileModel
                    {
                        Id = fileId,
                        Name = file.Name,
                        DateModified = file.LastWriteTime,
                        Type = GetFileType(file.Extension),
                        Size = FormatFileSize(file.Length),
                        DownloadUrl = $"/AdminPage/AdminFileFolder?handler=DownloadFile&fileId={Uri.EscapeDataString(fileId)}"
                    });
                }
            }

            private string GetRelativePath(string fullPath)
            {
                string fileStoragePath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage");
                return fullPath.Replace(fileStoragePath + Path.DirectorySeparatorChar, "")
                              .Replace("\\", "/");
            }

            private string GetFileType(string extension)
            {
                switch (extension.ToLower())
                {
                    case ".pdf":
                        return "PDF Document";
                    case ".doc":
                    case ".docx":
                        return "Word Document";
                    case ".xls":
                    case ".xlsx":
                        return "Excel Spreadsheet";
                    case ".ppt":
                    case ".pptx":
                        return "PowerPoint Presentation";
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".gif":
                        return "Image";
                    case ".txt":
                        return "Text Document";
                    default:
                        return $"{extension.TrimStart('.')} File";
                }
            }

            private string FormatFileSize(long bytes)
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                int order = 0;
                double len = bytes;

                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }

                return $"{len:0.##} {sizes[order]}";
            }

            private void EnsureCategoryExists(string categoryName)
            {
                string categoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", categoryName);

                if (!Directory.Exists(categoryPath))
                {
                    Directory.CreateDirectory(categoryPath);
                    // Create standard folders for each category
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 1"));
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 1", "BSCS"));
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 1", "BSCS", "2025"));
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 1", "BSCS", "2026"));
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 1", "BSIT"));
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 1", "BSIT", "2025"));
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 1", "BSIT", "2026"));

                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 2"));
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 2", "BSCS"));
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 2", "BSCS", "2025"));
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 2", "BSCS", "2026"));
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 2", "BSIT"));
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 2", "BSIT", "2025"));
                    Directory.CreateDirectory(Path.Combine(categoryPath, "Area 2", "BSIT", "2026"));
                }
            }

        private async Task BuildFolderHierarchy()
        {
            FolderHierarchy.Clear();

            // Get all categories from the database
            var categories = await _context.Categories
                .Include(c => c.Areas)
                    .ThenInclude(a => a.Courses)
                        .ThenInclude(c => c.YearFolders)
                .ToListAsync();

            foreach (var category in categories)
            {
                var categoryFolders = new List<FolderModel>();
                FolderHierarchy[category.Name] = categoryFolders;

                // Add areas as subfolders
                foreach (var area in category.Areas)
                {
                    var areaFolder = new FolderModel
                    {
                        Name = area.Name,
                        Path = $"{category.Name}/{area.Name}",
                        Category = category.Name,
                        ModifiedDate = DateTime.Now,
                        ItemCount = area.Courses.Count
                    };

                    // Add courses as sub-subfolders
                    foreach (var course in area.Courses)
                    {
                        var courseFolder = new FolderModel
                        {
                            Name = course.Name,
                            Path = $"{category.Name}/{area.Name}/{course.Name}",
                            Category = category.Name,
                            ModifiedDate = DateTime.Now,
                            ItemCount = course.YearFolders.Count
                        };

                        // Add year folders
                        foreach (var yearFolder in course.YearFolders)
                        {
                            courseFolder.SubFolders.Add(new FolderModel
                            {
                                Name = yearFolder.Year.ToString(),
                                Path = $"{category.Name}/{area.Name}/{course.Name}/{yearFolder.Year}",
                                Category = category.Name,
                                ModifiedDate = yearFolder.CreatedAt,
                                ItemCount = 0 // You might want to add a Files collection to YearFolder to count files
                            });
                        }

                        areaFolder.SubFolders.Add(courseFolder);
                    }

                    categoryFolders.Add(areaFolder);
                }
            }
        }

        private async Task EnsureCategoriesExistAsync()
        {
            // Check if we have any categories
            if (!await _context.Categories.AnyAsync())
            {
                // Create Thesis category
                var thesis = new Category { Name = "Thesis" };
                _context.Categories.Add(thesis);

                // Create Areas for Thesis
                var area1 = new Area { Name = "Area 1", Category = thesis };
                var area2 = new Area { Name = "Area 2", Category = thesis };
                _context.Areas.AddRange(area1, area2);

                // Create Courses for Area 1
                var bscsArea1 = new Course { Name = "BSCS", Area = area1 };
                var bsisArea1 = new Course { Name = "BSIS", Area = area1 };
                _context.Courses.AddRange(bscsArea1, bsisArea1);

                // Create Courses for Area 2
                var bscsArea2 = new Course { Name = "BSCS", Area = area2 };
                var bsisArea2 = new Course { Name = "BSIS", Area = area2 };
                _context.Courses.AddRange(bscsArea2, bsisArea2);

                // Create Year Folders for each Course
                var yearFolders = new List<YearFolder>
        {
            new YearFolder { Year = 2025, Course = bscsArea1, CreatedAt = DateTime.Now },
            new YearFolder { Year = 2026, Course = bscsArea1, CreatedAt = DateTime.Now },
            new YearFolder { Year = 2025, Course = bsisArea1, CreatedAt = DateTime.Now },
            new YearFolder { Year = 2026, Course = bsisArea1, CreatedAt = DateTime.Now },
            new YearFolder { Year = 2025, Course = bscsArea2, CreatedAt = DateTime.Now },
            new YearFolder { Year = 2026, Course = bscsArea2, CreatedAt = DateTime.Now },
            new YearFolder { Year = 2025, Course = bsisArea2, CreatedAt = DateTime.Now },
            new YearFolder { Year = 2026, Course = bsisArea2, CreatedAt = DateTime.Now }
        };
                _context.YearFolders.AddRange(yearFolders);

                // Create ALCU category
                var alcu = new Category { Name = "ALCU" };
                _context.Categories.Add(alcu);

                // Create Memo category
                var memo = new Category { Name = "Memo" };
                _context.Categories.Add(memo);

                await _context.SaveChangesAsync();
            }
        }

        // File Folder Structure

        public IActionResult OnGetGetFolderStructure(string category)
            {
                if (string.IsNullOrEmpty(category))
                    return new JsonResult(new { success = false });

                string categoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", category);

                if (!Directory.Exists(categoryPath))
                    return new JsonResult(new { success = false });

                var folders = new List<object>();
                DirectoryInfo directoryInfo = new DirectoryInfo(categoryPath);

                foreach (var dir in directoryInfo.GetDirectories())
                {
                    string relativePath = GetRelativePath(dir.FullName);
                    int itemCount = dir.GetFiles().Length + dir.GetDirectories().Length;

                    folders.Add(new
                    {
                        name = dir.Name,
                        path = relativePath,
                        modifiedDate = dir.LastWriteTime,
                        itemCount = itemCount
                    });
                }

                return new JsonResult(new { success = true, folders });
            }

            public int GetFileCount(string category)
            {
                string categoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", category);

                if (!Directory.Exists(categoryPath))
                    return 0;

                return CountFilesInDirectory(categoryPath);
            }

            private int CountFilesInDirectory(string path)
            {
                int count = 0;

                // Count files directly in this directory
                count += Directory.GetFiles(path).Length;

                // Count files in subdirectories
                foreach (var dir in Directory.GetDirectories(path))
                {
                    count += CountFilesInDirectory(dir);
                }

                return count;
            }

        // Handler for adding a new category
        public async Task<IActionResult> OnPostAddCategory(string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                try
                {
                    // Check if category already exists
                    if (await _context.Categories.AnyAsync(c => c.Name == categoryName))
                    {
                        TempData["ErrorMessage"] = "Category already exists";
                        return RedirectToPage();
                    }

                    // Add the category to the database
                    var category = new Category { Name = categoryName };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();

                    // Create the directory in the file system
                    string categoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", categoryName);
                    if (!Directory.Exists(categoryPath))
                    {
                        Directory.CreateDirectory(categoryPath);
                        // Create standard subfolders
                        EnsureCategoryExists(categoryName);
                    }

                    TempData["SuccessMessage"] = "Category created successfully";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating category: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Category name cannot be empty";
            }

            return RedirectToPage();
        }

        // Handler for adding a new area
        public async Task<IActionResult> OnPostAddArea(string areaName, string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(areaName) && !string.IsNullOrWhiteSpace(categoryName))
            {
                try
                {
                    // Find the category
                    var category = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name == categoryName);

                    if (category == null)
                    {
                        TempData["ErrorMessage"] = "Category not found";
                        return RedirectToPage();
                    }

                    // Check if area already exists in this category
                    if (await _context.Areas.AnyAsync(a => a.CategoryId == category.CategoryId && a.Name == areaName))
                    {
                        TempData["ErrorMessage"] = "Area already exists in this category";
                        return RedirectToPage(new { category = categoryName });
                    }

                    // Add the area to the database
                    var area = new Area
                    {
                        Name = areaName,
                        CategoryId = category.CategoryId
                    };
                    _context.Areas.Add(area);
                    await _context.SaveChangesAsync();

                    // Create the directory in the file system
                    string areaPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", categoryName, areaName);
                    if (!Directory.Exists(areaPath))
                    {
                        Directory.CreateDirectory(areaPath);
                    }

                    TempData["SuccessMessage"] = "Area created successfully";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating area: {ex.Message}";
                }

                return RedirectToPage(new { category = categoryName });
            }

            TempData["ErrorMessage"] = "Area name cannot be empty";
            return RedirectToPage();
        }

        // Handler for adding a new folder
        public IActionResult OnPostAddFolder(string folderName, string category, string currentPath)
            {
                if (!string.IsNullOrWhiteSpace(folderName) && !string.IsNullOrWhiteSpace(category))
                {
                    string newFolderPath;

                    if (string.IsNullOrWhiteSpace(currentPath) || currentPath == category)
                    {
                        newFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", category, folderName);
                    }
                    else
                    {
                        newFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", currentPath, folderName);
                    }

                    if (!Directory.Exists(newFolderPath))
                    {
                        Directory.CreateDirectory(newFolderPath);
                    }

                    return RedirectToPage(new { category, folderPath = currentPath });
                }

                return RedirectToPage();
            }

            // Handler for uploading a file
            public async Task<IActionResult> OnPostUploadFileAsync(IFormFile file, string category, string currentPath)
            {
                if (file != null && file.Length > 0)
                {
                    string uploadPath;

                    if (string.IsNullOrWhiteSpace(currentPath) || currentPath == category)
                    {
                        uploadPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", category);
                    }
                    else
                    {
                        uploadPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", currentPath);
                    }

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string filePath = Path.Combine(uploadPath, file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return RedirectToPage(new { category, folderPath = currentPath });
                }

                return RedirectToPage();
            }

            // Handler for downloading a file
            public IActionResult OnGetDownloadFile(string fileId)
            {
                if (string.IsNullOrEmpty(fileId))
                    return NotFound();

                string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", fileId.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (!System.IO.File.Exists(filePath))
                    return NotFound();

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);

                return File(fileBytes, "application/octet-stream", fileName);
            }

            // Handler for deleting a file
            public IActionResult OnPostDeleteFile(string fileId)
            {
                if (string.IsNullOrEmpty(fileId))
                    return new JsonResult(new { success = false });

                string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", fileId.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return new JsonResult(new { success = true });
                }

                return new JsonResult(new { success = false });
            }

            // Handler for deleting a folder
            public IActionResult OnPostDeleteFolder(string folderPath)
            {
                if (string.IsNullOrEmpty(folderPath))
                    return new JsonResult(new { success = false });

                string directoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", folderPath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (Directory.Exists(directoryPath))
                {
                    try
                    {
                        // Only allow deletion of subfolders, not main categories
                        if (folderPath.Contains("/"))
                        {
                            Directory.Delete(directoryPath, true);
                            return new JsonResult(new { success = true });
                        }
                        else
                        {
                            // For main categories, just clear the contents
                            DirectoryInfo directory = new DirectoryInfo(directoryPath);
                            foreach (var file in directory.GetFiles())
                            {
                                file.Delete();
                            }
                            foreach (var dir in directory.GetDirectories())
                            {
                                dir.Delete(true);
                            }
                            // Recreate standard folders
                            EnsureCategoryExists(folderPath);
                            return new JsonResult(new { success = true });
                        }
                    }
                    catch (Exception)
                    {
                        return new JsonResult(new { success = false, message = "Could not delete folder. Make sure all files are closed." });
                    }
                }

                return new JsonResult(new { success = false });
            }
        }

        public class BreadcrumbItem
        {
            public string Name { get; set; }
            public string Path { get; set; }
        }

        public class FileModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime DateModified { get; set; }
            public string Type { get; set; }
            public string Size { get; set; }
            public string DownloadUrl { get; set; }

            public string GetFileIcon()
            {
                switch (Type.ToLower())
                {
                    case "pdf document":
                        return "fas fa-file-pdf";
                    case "word document":
                        return "fas fa-file-word";
                    case "excel spreadsheet":
                        return "fas fa-file-excel";
                    case "powerpoint presentation":
                        return "fas fa-file-powerpoint";
                    case "image":
                        return "fas fa-file-image";
                    case "text document":
                        return "fas fa-file-alt";
                    default:
                        return "fas fa-file";
                }
            }
        }
    }
