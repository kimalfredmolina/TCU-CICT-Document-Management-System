using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;

namespace Document_Management_System.Pages.AdminPage
{
    public class AdminViewUserModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public AdminViewUserModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public UserDetailsModel UserDetails { get; set; }
        public string UserProfileImage { get; set; }

        public IActionResult OnGet(string id)  // Changed from int to string
        {
            // Retrieve the connection string from appsettings.json
            string connectionString = _configuration.GetConnectionString("Default");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                                u.UserName,
                                u.Email,
                                u.PhoneNumber,
                                p.FullName,
                                p.ProfileImage
                                FROM dbo.AspNetUsers u
                                LEFT JOIN dbo.AspNetUsers p ON u.Id = p.Id
                                WHERE u.Id = @Id";

                SqlCommand command = new SqlCommand(query, connection);
                // Use SqlDbType.UniqueIdentifier for GUID parameters
                command.Parameters.Add("@Id", System.Data.SqlDbType.NVarChar).Value = id;

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        UserDetails = new UserDetailsModel
                        {
                            UserName = reader["UserName"].ToString(),
                            Email = reader["Email"].ToString(),
                            PhoneNumber = reader["PhoneNumber"]?.ToString(),
                            FullName = reader["FullName"]?.ToString()
                        };
                        UserProfileImage = reader["ProfileImage"]?.ToString();
                    }
                    else
                    {
                        return RedirectToPage("/AdminPage/AdminUserTable");
                    }
                }
            }

            return Page();
        }
    }

    public class UserDetailsModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
    }
}