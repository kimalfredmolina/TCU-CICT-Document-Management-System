using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public List<string> UserRoles { get; set; } = new List<string>();

        public IActionResult OnGet(string id)
        {
            string connectionString = _configuration.GetConnectionString("Default");

            // First get user details
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string userQuery = @"SELECT 
                        u.UserName,
                        u.Email,
                        u.PhoneNumber,
                        u.FullName,
                        u.ProfileImage
                        FROM dbo.AspNetUsers u
                        WHERE u.Id = @Id";

                SqlCommand userCommand = new SqlCommand(userQuery, connection);
                userCommand.Parameters.Add("@Id", System.Data.SqlDbType.NVarChar).Value = id;

                connection.Open();
                using (SqlDataReader reader = userCommand.ExecuteReader())
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

            // Then get user roles
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string rolesQuery = @"SELECT r.Name 
                                    FROM dbo.AspNetUserRoles ur
                                    JOIN dbo.AspNetRoles r ON ur.RoleId = r.Id
                                    WHERE ur.UserId = @UserId";

                SqlCommand rolesCommand = new SqlCommand(rolesQuery, connection);
                rolesCommand.Parameters.Add("@UserId", System.Data.SqlDbType.NVarChar).Value = id;

                connection.Open();
                using (SqlDataReader reader = rolesCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserRoles.Add(reader["Name"].ToString());
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