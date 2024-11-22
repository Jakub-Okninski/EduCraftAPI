using EduCraftAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace EduCraftAPI.Entities{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new DataDbContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<DataDbContext>>()))
            {

                if (!context.Category.Any())
                {
                context.Category.AddRange(
                     new Category.Category
                     {
                         Name = "IT"
                     },
                     new Category.Category
                     {
                         Name = "Math"
                     }

                );
                    context.SaveChanges();
                }

                if (!context.Roles.Any())
                {
                    context.Roles.AddRange(
                   new User.Role
                   {
                       Name = "Admin"
                   },
                   new User.Role
                   {
                       Name = "User"
                   }
                );
                    context.SaveChanges();
                }

                if (!context.Users.Any())
                {
                    context.Users.Add(new User.User()
                    {
                        Email = "Jakub@wp.pl",
                        FirstName = "Jakub",
                        LastName = "Okniński",
                        RoleID = 1,
                        Password = "AQAAAAIAAYagAAAAEPutQJKTy3Aemoqw8jepJSnmX9kyn0XzBdABnEavR2y+rSJ8cNYOJGRviAiX4g2MJQ=="
                    });
                    context.SaveChanges();
                }
            }
        }
    }
}
