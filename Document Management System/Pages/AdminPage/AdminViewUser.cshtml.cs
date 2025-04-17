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

        public IActionResult OnGet(string id)
        {
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

                        if (reader["ProfileImage"] != DBNull.Value)
                        {
                            byte[] imageData = (byte[])reader["ProfileImage"];
                            UserProfileImage = $"data:image/png;base64,{Convert.ToBase64String(imageData)}";
                        }
                        else
                        {
                            UserProfileImage = null;
                        }
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