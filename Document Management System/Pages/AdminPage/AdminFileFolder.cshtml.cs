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
    using Microsoft.AspNetCore.Identity;
    using System.Security.Claims;

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
            // List to store accessible categories for the current user
            public List<int> AccessibleCategoryIds { get; set; } = new List<int>();
            public bool IsAdmin { get; set; } = false;

        public async Task OnGetAsync()
        {
            // Create the FileStorage directory if it doesn't exist
            string fileStoragePath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage");
            if (!Directory.Exists(fileStoragePath))
            {
                Directory.CreateDirectory(fileStoragePath);
            }

            // Get accessible categories for the current user
            await LoadAccessibleCategoriesAsync();

            // Ensure categories exist in the database
            await EnsureCategoriesExistAsync();

            // Build the folder hierarchy from the database
            await BuildFolderHierarchy();

            if (!string.IsNullOrEmpty(CurrentCategory))
            {
                // Check if user has access to this category
                var categoryId = await _context.Categories
                    .Where(c => c.Name == CurrentCategory)
                    .Select(c => c.CategoryId)
                    .FirstOrDefaultAsync();

                if (categoryId > 0 && !AccessibleCategoryIds.Contains(categoryId) && !IsAdmin)
                {
                    TempData["ErrorMessage"] = "You do not have access to this category";
                    CurrentCategory = null;
                    return;
                }

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

        private async Task LoadAccessibleCategoriesAsync()
        {
            // Clear the list
            AccessibleCategoryIds.Clear();

            // Check if user is admin
            IsAdmin = User.IsInRole("Admin");

            if (IsAdmin)
            {
                // Admin has access to all categories
                var allCategoryIds = await _context.Categories
                    .Select(c => c.CategoryId)
                    .ToListAsync();
                AccessibleCategoryIds.AddRange(allCategoryIds);
                return;
            }

            // Get the current user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If not admin and we have a user ID, get categories from FolderAccess table
            if (!string.IsNullOrEmpty(userId))
            {
                var accessibleCategories = await _context.FolderAccess
                    .Where(fa => fa.UserId == userId)
                    .Select(fa => fa.CategoryId)
                    .ToListAsync();

                AccessibleCategoryIds.AddRange(accessibleCategories);
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
            var query = _context.Categories
                .Include(c => c.Areas)
                    .ThenInclude(a => a.Courses)
                        .ThenInclude(c => c.YearFolders)
                .AsQueryable();

            // Filter by accessible categories if not admin
            if (!IsAdmin && AccessibleCategoryIds.Any())
            {
                query = query.Where(c => AccessibleCategoryIds.Contains(c.CategoryId));
            }

            var categories = await query.ToListAsync();
            string fileStoragePath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage");

            foreach (var category in categories)
            {
                var categoryFolders = new List<FolderModel>();
                FolderHierarchy[category.Name] = categoryFolders;

                // Get category directory info for last modified date and item count
                string categoryPath = Path.Combine(fileStoragePath, category.Name);
                DateTime categoryModifiedDate = Directory.Exists(categoryPath)
                    ? Directory.GetLastWriteTime(categoryPath)
                    : DateTime.Now;

                // Add areas as subfolders
                foreach (var area in category.Areas)
                {
                    string areaPath = Path.Combine(categoryPath, area.Name);
                    DateTime areaModifiedDate = Directory.Exists(areaPath)
                        ? Directory.GetLastWriteTime(areaPath)
                        : DateTime.Now;

                    // Calculate actual item count for area (files + directories)
                    int areaItemCount = Directory.Exists(areaPath)
                        ? Directory.GetFiles(areaPath).Length + Directory.GetDirectories(areaPath).Length
                        : area.Courses.Count;

                    var areaFolder = new FolderModel
                    {
                        Name = area.Name,
                        Path = $"{category.Name}/{area.Name}",
                        Category = category.Name,
                        ModifiedDate = areaModifiedDate,
                        ItemCount = areaItemCount
                    };

                    // Add courses as sub-subfolders
                    foreach (var course in area.Courses)
                    {
                        string coursePath = Path.Combine(areaPath, course.Name);
                        DateTime courseModifiedDate = Directory.Exists(coursePath)
                            ? Directory.GetLastWriteTime(coursePath)
                            : DateTime.Now;

                        // Calculate actual item count for course (files + directories)
                        int courseItemCount = Directory.Exists(coursePath)
                            ? Directory.GetFiles(coursePath).Length + Directory.GetDirectories(coursePath).Length
                            : course.YearFolders.Count;

                        var courseFolder = new FolderModel
                        {
                            Name = course.Name,
                            Path = $"{category.Name}/{area.Name}/{course.Name}",
                            Category = category.Name,
                            ModifiedDate = courseModifiedDate,
                            ItemCount = courseItemCount
                        };

                        // Add year folders
                        foreach (var yearFolder in course.YearFolders)
                        {
                            string yearPath = Path.Combine(coursePath, yearFolder.Year.ToString());
                            DateTime yearModifiedDate = Directory.Exists(yearPath)
                                ? Directory.GetLastWriteTime(yearPath)
                                : yearFolder.CreatedAt;

                            // Calculate actual item count for year folder (files + directories)
                            int yearItemCount = Directory.Exists(yearPath)
                                ? Directory.GetFiles(yearPath).Length + Directory.GetDirectories(yearPath).Length
                                : 0;

                            courseFolder.SubFolders.Add(new FolderModel
                            {
                                Name = yearFolder.Year.ToString(),
                                Path = $"{category.Name}/{area.Name}/{course.Name}/{yearFolder.Year}",
                                Category = category.Name,
                                ModifiedDate = yearModifiedDate,
                                ItemCount = yearItemCount
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

            // Count both files and folders at the top level
            int fileCount = Directory.GetFiles(categoryPath).Length;
            int folderCount = Directory.GetDirectories(categoryPath).Length;

            // Count files in subdirectories
            foreach (var dir in Directory.GetDirectories(categoryPath))
            {
                fileCount += CountFilesInDirectory(dir);
            }

            return fileCount + folderCount;
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
                    // Check if user has admin rights
                    if (!User.IsInRole("Admin"))
                    {
                        TempData["ErrorMessage"] = "You do not have permission to create categories";
                        return RedirectToPage();
                    }

                    // Check if category already exists
                    if (await _context.Categories.AnyAsync(c => c.Name == categoryName))
                    {
                        TempData["ErrorMessage"] = $"Category '{categoryName}' already exists";
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

                    // Load accessible categories for permission check
                    await LoadAccessibleCategoriesAsync();

                    // Check if user has access to this category
                    if (!IsAdmin && !AccessibleCategoryIds.Contains(category.CategoryId))
                    {
                        TempData["ErrorMessage"] = "You do not have permission to add areas to this category";
                        return RedirectToPage();
                    }

                    // Check if area already exists in this category
                    if (await _context.Areas.AnyAsync(a => a.CategoryId == category.CategoryId && a.Name == areaName))
                    {
                        TempData["ErrorMessage"] = $"Area '{areaName}' already exists in this category";
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

                        // Set the last write time to now for just this directory
                        Directory.SetLastWriteTime(areaPath, DateTime.Now);
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

        // Handler for adding a new course
        public async Task<IActionResult> OnPostAddCourse(string courseName, string areaPath, string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(courseName) && !string.IsNullOrWhiteSpace(areaPath) && !string.IsNullOrWhiteSpace(categoryName))
            {
                try
                {
                    // Get area name from the path
                    string areaName = areaPath.Split('/').Last();

                    // Find the area
                    var area = await _context.Areas
                        .Include(a => a.Category)
                        .FirstOrDefaultAsync(a => a.Name == areaName && a.Category.Name == categoryName);

                    if (area == null)
                    {
                        TempData["ErrorMessage"] = "Area not found";
                        return RedirectToPage();
                    }

                    // Load accessible categories for permission check
                    await LoadAccessibleCategoriesAsync();

                    // Check if user has access to this category
                    if (!IsAdmin && !AccessibleCategoryIds.Contains(area.CategoryId))
                    {
                        TempData["ErrorMessage"] = "You do not have permission to add courses to this area";
                        return RedirectToPage();
                    }

                    // Check if course already exists in this area
                    if (await _context.Courses.AnyAsync(c => c.AreaId == area.AreaId && c.Name == courseName))
                    {
                        TempData["ErrorMessage"] = $"Course '{courseName}' already exists in this area";
                        return RedirectToPage(new { category = categoryName });
                    }

                    // Add the course to the database
                    var course = new Course
                    {
                        Name = courseName,
                        AreaId = area.AreaId
                    };
                    _context.Courses.Add(course);
                    await _context.SaveChangesAsync();

                    // Create directory in the file system
                    string coursePath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", areaPath, courseName);
                    if (!Directory.Exists(coursePath))
                    {
                        Directory.CreateDirectory(coursePath);
                    }

                    TempData["SuccessMessage"] = "Course created successfully";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating course: {ex.Message}";
                }

                return RedirectToPage(new { category = categoryName });
            }

            TempData["ErrorMessage"] = "Course name cannot be empty";
            return RedirectToPage();
        }

        // Handler for adding a new year
        public async Task<IActionResult> OnPostAddYear(int year, string coursePath, string categoryName, string areaPath)
        {
            if (year >= 2000 && year <= 2100 &&
                !string.IsNullOrWhiteSpace(coursePath) &&
                !string.IsNullOrWhiteSpace(categoryName) &&
                !string.IsNullOrWhiteSpace(areaPath))
            {
                try
                {
                    // Extract course and area names from the paths
                    string courseName = coursePath.Split('/').Last();
                    string areaName = areaPath.Split('/').Last();

                    // Find the course in the database
                    var course = await _context.Courses
                        .Include(c => c.Area)
                            .ThenInclude(a => a.Category)
                        .FirstOrDefaultAsync(c =>
                            c.Name == courseName &&
                            c.Area.Name == areaName &&
                            c.Area.Category.Name == categoryName);

                    if (course == null)
                    {
                        TempData["ErrorMessage"] = "Course not found";
                        return RedirectToPage();
                    }

                    // Load accessible categories for permission check
                    await LoadAccessibleCategoriesAsync();

                    // Check if user has access to this category
                    if (!IsAdmin && !AccessibleCategoryIds.Contains(course.Area.CategoryId))
                    {
                        TempData["ErrorMessage"] = "You do not have permission to add years to this course";
                        return RedirectToPage();
                    }

                    // Check if the year folder already exists
                    if (await _context.YearFolders.AnyAsync(y => y.CourseId == course.CourseId && y.Year == year))
                    {
                        TempData["ErrorMessage"] = $"Year '{year}' already exists in this course";
                        return RedirectToPage(new { category = categoryName });
                    }

                    // Create the year folder in the database
                    var yearFolder = new YearFolder
                    {
                        Year = year,
                        CourseId = course.CourseId,
                        CreatedAt = DateTime.Now
                    };
                    _context.YearFolders.Add(yearFolder);
                    await _context.SaveChangesAsync();

                    // Create the corresponding directory in the file system
                    string yearPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", coursePath, year.ToString());
                    if (!Directory.Exists(yearPath))
                    {
                        Directory.CreateDirectory(yearPath);
                    }

                    TempData["SuccessMessage"] = "Year created successfully";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating year: {ex.Message}";
                }

                return RedirectToPage(new { category = categoryName, folderPath = areaPath });
            }

            TempData["ErrorMessage"] = "Year is invalid";
            return RedirectToPage();
        }


        // Handler for adding a new folder
        public async Task<IActionResult> OnPostAddFolder(string folderName, string category, string currentPath)
        {
            if (!string.IsNullOrWhiteSpace(folderName) && !string.IsNullOrWhiteSpace(category))
            {
                try
                {
                    // Load accessible categories for permission check
                    await LoadAccessibleCategoriesAsync();

                    // Check if user has access to this category
                    var categoryId = await _context.Categories
                        .Where(c => c.Name == category)
                        .Select(c => c.CategoryId)
                        .FirstOrDefaultAsync();

                    if (!IsAdmin && !AccessibleCategoryIds.Contains(categoryId))
                    {
                        TempData["ErrorMessage"] = "You do not have permission to add folders to this category";
                        return RedirectToPage();
                    }

                    string newFolderPath;

                    if (string.IsNullOrWhiteSpace(currentPath) || currentPath == category)
                    {
                        newFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", category, folderName);
                    }
                    else
                    {
                        newFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", currentPath, folderName);
                    }

                    // Check if folder already exists
                    if (Directory.Exists(newFolderPath))
                    {
                        TempData["ErrorMessage"] = $"Folder '{folderName}' already exists in this location";
                        return RedirectToPage(new { category, folderPath = currentPath });
                    }

                    Directory.CreateDirectory(newFolderPath);
                    TempData["SuccessMessage"] = "Folder created successfully";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating folder: {ex.Message}";
                }

                return RedirectToPage(new { category, folderPath = currentPath });
            }

            TempData["ErrorMessage"] = "Folder name cannot be empty";
            return RedirectToPage();
        }

        // Handler for uploading a file
        public async Task<IActionResult> OnPostUploadFileAsync(IFormFile file, string category, string currentPath)
        {
            if (file != null && file.Length > 0)
            {
                try
                {
                    // Load accessible categories for permission check
                    await LoadAccessibleCategoriesAsync();

                    // Check if user has access to this category
                    var categoryId = await _context.Categories
                        .Where(c => c.Name == category)
                        .Select(c => c.CategoryId)
                        .FirstOrDefaultAsync();

                    if (!IsAdmin && !AccessibleCategoryIds.Contains(categoryId))
                    {
                        TempData["ErrorMessage"] = "You do not have permission to upload files to this category";
                        return RedirectToPage();
                    }

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

                    // Check if file already exists
                    if (System.IO.File.Exists(filePath))
                    {
                        TempData["ErrorMessage"] = $"File '{file.FileName}' already exists in this location";
                        return RedirectToPage(new { category, folderPath = currentPath });
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    TempData["SuccessMessage"] = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error uploading file: {ex.Message}";
                }

                return RedirectToPage(new { category, folderPath = currentPath });
            }

            TempData["ErrorMessage"] = "No file selected for upload";
            return RedirectToPage();
        }

        // Handler for deleting a folder
        public async Task<IActionResult> OnPostDeleteFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return new JsonResult(new { success = false, message = "Folder path is undefined" });

            try
            {
                // Load accessible categories for permission check
                await LoadAccessibleCategoriesAsync();

                // Extract category from folderPath to check permissions
                string[] pathParts = folderPath.Split('/');
                if (pathParts.Length > 0)
                {
                    string categoryName = pathParts[0];
                    var categoryId = await _context.Categories
                        .Where(c => c.Name == categoryName)
                        .Select(c => c.CategoryId)
                        .FirstOrDefaultAsync();

                    if (!IsAdmin && !AccessibleCategoryIds.Contains(categoryId))
                    {
                        return new JsonResult(new { success = false, message = "You do not have permission to delete this folder" });
                    }
                }

                // Parse the folder path to determine what type of folder it is
                if (pathParts.Length == 1)
                {
                    // This is a category - only admins can delete categories
                    if (!IsAdmin)
                    {
                        return new JsonResult(new { success = false, message = "Only administrators can delete categories" });
                    }

                    var category = await _context.Categories
                        .Include(c => c.Areas)
                            .ThenInclude(a => a.Courses)
                                .ThenInclude(c => c.YearFolders)
                        .FirstOrDefaultAsync(c => c.Name == pathParts[0]);

                    if (category != null)
                    {
                        // Delete all related entities in the database
                        foreach (var area in category.Areas)
                        {
                            foreach (var course in area.Courses)
                            {
                                _context.YearFolders.RemoveRange(course.YearFolders);
                            }
                            _context.Courses.RemoveRange(area.Courses);
                        }
                        _context.Areas.RemoveRange(category.Areas);
                        _context.Categories.Remove(category);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (pathParts.Length == 2)
                {
                    // This is an area
                    var area = await _context.Areas
                        .Include(a => a.Courses)
                            .ThenInclude(c => c.YearFolders)
                        .FirstOrDefaultAsync(a => a.Name == pathParts[1] && a.Category.Name == pathParts[0]);

                    if (area != null)
                    {
                        // Delete all related entities in the database
                        foreach (var course in area.Courses)
                        {
                            _context.YearFolders.RemoveRange(course.YearFolders);
                        }
                        _context.Courses.RemoveRange(area.Courses);
                        _context.Areas.Remove(area);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (pathParts.Length == 3)
                {
                    // This is a course
                    var course = await _context.Courses
                        .Include(c => c.YearFolders)
                        .FirstOrDefaultAsync(c => c.Name == pathParts[2] &&
                                                 c.Area.Name == pathParts[1] &&
                                                 c.Area.Category.Name == pathParts[0]);

                    if (course != null)
                    {
                        // Delete all related entities in the database
                        _context.YearFolders.RemoveRange(course.YearFolders);
                        _context.Courses.Remove(course);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (pathParts.Length == 4)
                {
                    // This is a year folder
                    var yearFolder = await _context.YearFolders
                        .FirstOrDefaultAsync(y => y.Year.ToString() == pathParts[3] &&
                                                 y.Course.Name == pathParts[2] &&
                                                 y.Course.Area.Name == pathParts[1] &&
                                                 y.Course.Area.Category.Name == pathParts[0]);

                    if (yearFolder != null)
                    {
                        // Delete the year folder entity from the database
                        _context.YearFolders.Remove(yearFolder);
                        await _context.SaveChangesAsync();
                    }
                }

                // Now handle the file system deletion
                string directoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", folderPath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (Directory.Exists(directoryPath))
                {
                    // Delete the directory and all its contents
                    Directory.Delete(directoryPath, true);

                    // If this was a category, recreate the empty directory
                    if (pathParts.Length == 1)
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    return new JsonResult(new { success = true });
                }

                return new JsonResult(new { success = false, message = "Directory not found" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error deleting folder: {ex.Message}" });
            }
        }
        public DateTime GetCategoryLastModifiedDate(string categoryName)
        {
            string categoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "FileStorage", categoryName);
            return Directory.Exists(categoryPath) ? Directory.GetLastWriteTime(categoryPath) : DateTime.Now;
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
