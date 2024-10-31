
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
            modelBuilder.Entity<Quiz>().ToTable("Quiz");
            modelBuilder.Entity<Question>().ToTable("Question");
            modelBuilder.Entity<Answer>().ToTable("Answer");
            modelBuilder.Entity<Role>().ToTable("Role");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Presentations>().ToTable("Presentation");
            modelBuilder.Entity<Flashcards>().ToTable("Flashcards");
            modelBuilder.Entity<Flashcard>().ToTable("Flashcard");
            modelBuilder.Entity<Category>().ToTable("Category");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
