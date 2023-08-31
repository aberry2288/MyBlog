using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using Npgsql;

namespace MyBlog.Data
{
    public static class DataUtility
    {
        private const string? _adminRole = "Admin";

        private const string? _moderatorRole = "Moderator";

        public static string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) ? connectionString! : BuildConnectionString(databaseUrl);
        }

        private static string BuildConnectionString(string databaseUrl)
        {
            //Provides an object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            //Provides a simple way to create and manage the contents of connection strings used by the NpgsqlConnection class.
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            //Obtaining the necessary services based on the IServiceProvider parameter
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();

            var userManagerSvc = svcProvider.GetRequiredService<UserManager<BlogUser>>();

            var configurationSvc = svcProvider.GetRequiredService<IConfiguration>();

            var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //Align the database by checking migrations
            await dbContextSvc.Database.MigrateAsync();

            //Seed Application Roles
            await SeedRolesAsync(roleManagerSvc);

            //Seed User(s)
            await SeedBlogUsersAsync(userManagerSvc, configurationSvc);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(_adminRole!))
            {
                await roleManager.CreateAsync(new IdentityRole(_adminRole!));
            }

            if (!await roleManager.RoleExistsAsync(_moderatorRole!))
            {
                await roleManager.CreateAsync(new IdentityRole(_moderatorRole!));
            }
        }

        private static async Task SeedBlogUsersAsync(UserManager<BlogUser> userManager, IConfiguration configuration)
        {
            string? adminEmail = configuration["AdminLoginEmail"] ?? Environment.GetEnvironmentVariable("AdminLoginEmail");
            string? adminPassword = configuration["AdminPwd"] ?? Environment.GetEnvironmentVariable("AdminPwd");
            string? moderatorEmail = configuration["ModeratorLoginEmail"] ?? Environment.GetEnvironmentVariable("ModeratorLoginEmail");
            string? moderatorPassword = configuration["ModeratorPwd"] ?? Environment.GetEnvironmentVariable("ModeratorPwd");

            try
            {
                //seed the admin
                BlogUser? adminUser = new BlogUser()
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Adam",
                    LastName = "Berry",
                    EmailConfirmed = true
                };

                BlogUser? blogUser = await userManager.FindByEmailAsync(adminEmail!);

                if (blogUser == null)
                {
                    await userManager.CreateAsync(adminUser, adminPassword!);
                    await userManager.AddToRoleAsync(adminUser, _adminRole!);
                }

                //seed the moderator
                BlogUser? modUser = new BlogUser()
                {
                    UserName = moderatorEmail,
                    Email = moderatorEmail,
                    FirstName = "Antonio",
                    LastName = "Raynor",
                    EmailConfirmed = true
                };

                blogUser = await userManager.FindByEmailAsync(moderatorEmail!);

                if (blogUser == null)
                {
                    await userManager.CreateAsync(modUser, moderatorPassword!);
                    await userManager.AddToRoleAsync(modUser, _moderatorRole!);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("************ ERROR ************");
                Console.WriteLine("Error Seeding Default Blog Users");
                Console.WriteLine(ex.Message);
                Console.WriteLine("******************************");

                throw;
            }
        }

    }
}
