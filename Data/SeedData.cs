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
                if (!context.Quizzes.Any())
                {
                context.Quizzes.AddRange(
                    new Quiz.Quiz
                    {
                        Name = "Quiz 1"
                    },
                    new Quiz.Quiz
                    {
                        Name = "Quiz 2"
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
            }
        }
    }
}
