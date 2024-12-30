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
                    new Category.Category { Name = "Informatyka" },
                    new Category.Category { Name = "Programowanie" },
                    new Category.Category { Name = "Matematyka" },
                    new Category.Category { Name = "Fizyka" },
                    new Category.Category { Name = "Języki Obce" },
                    new Category.Category { Name = "Angielski" },
                    new Category.Category { Name = "Zarządzanie Projektami" },
                    new Category.Category { Name = "Inne" },
                    new Category.Category { Name = "Historia" },
                    new Category.Category { Name = "Geografia" },
                    new Category.Category { Name = "Biologia" },
                    new Category.Category { Name = "Chemia" },
                    new Category.Category { Name = "Wiedza o Społeczeństwie" },
                    new Category.Category { Name = "Literatura" },
                    new Category.Category { Name = "Sztuka" },
                    new Category.Category { Name = "Muzyka" },
                    new Category.Category { Name = "Religia" },
                    new Category.Category { Name = "Przedsiębiorczość" },
                    new Category.Category { Name = "Edukacja dla Bezpieczeństwa" },
                    new Category.Category { Name = "Wychowanie Fizyczne" }
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
                        IsBlocked=false,
                        Password = "AQAAAAIAAYagAAAAEPutQJKTy3Aemoqw8jepJSnmX9kyn0XzBdABnEavR2y+rSJ8cNYOJGRviAiX4g2MJQ=="
                    });
                    context.SaveChanges();
                }
            }
        }
    }
}
