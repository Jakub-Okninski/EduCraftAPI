
using EduCraftAPI.Entities.Category;
using EduCraftAPI.Entities.Flashcards;
using EduCraftAPI.Entities.Presentation;
using EduCraftAPI.Entities.Quiz;
using EduCraftAPI.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace EduCraftAPI.Data
{
    public class DataDbContext : DbContext
    {
        public DataDbContext(DbContextOptions<DataDbContext> options) : base(options)
        {
        }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers{ get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Presentations> Presentation { get; set; }
        public DbSet<Flashcards> Flashcards { get; set; }
        public DbSet<Flashcard> Flashcard { get; set; }
        public DbSet<Category> Category { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quiz>().ToTable("Quizzes");
            modelBuilder.Entity<Question>().ToTable("Questions");
            modelBuilder.Entity<Answer>().ToTable("Answers");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Presentations>().ToTable("Presentations");
            modelBuilder.Entity<Flashcards>().ToTable("Flashcards");
            modelBuilder.Entity<Flashcard>().ToTable("Cards");
            modelBuilder.Entity<Category>().ToTable("Categories");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
