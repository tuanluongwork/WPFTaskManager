using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Models;
using System;

namespace TaskManager.Data.Context
{
    public class TaskManagerDbContext : DbContext
    {
        public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaskItem> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TaskItem entity
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(t => t.Id);
                
                entity.Property(t => t.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(t => t.Description)
                    .HasMaxLength(1000);

                entity.Property(t => t.AssignedTo)
                    .HasMaxLength(100);

                entity.Property(t => t.Category)
                    .HasMaxLength(50);

                entity.Property(t => t.Priority)
                    .HasConversion<string>();

                entity.Property(t => t.Status)
                    .HasConversion<string>();

                // Create indexes for better performance
                entity.HasIndex(t => t.Status);
                entity.HasIndex(t => t.Priority);
                entity.HasIndex(t => t.AssignedTo);
                entity.HasIndex(t => t.DueDate);
            });

            // Seed initial data
            modelBuilder.Entity<TaskItem>().HasData(
                new TaskItem
                {
                    Id = 1,
                    Title = "Setup Development Environment",
                    Description = "Install Visual Studio, .NET SDK, and configure Git",
                    Priority = TaskPriority.High,
                    Status = TaskManager.Core.Models.TaskStatus.Completed,
                    CreatedDate = DateTime.Now.AddDays(-7),
                    CompletedDate = DateTime.Now.AddDays(-5),
                    AssignedTo = "John Doe",
                    Category = "Setup",
                    EstimatedHours = 4,
                    ActualHours = 3,
                    CompletionPercentage = 100
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Implement User Authentication",
                    Description = "Add login and registration functionality with JWT tokens",
                    Priority = TaskPriority.Critical,
                    Status = TaskManager.Core.Models.TaskStatus.InProgress,
                    CreatedDate = DateTime.Now.AddDays(-3),
                    DueDate = DateTime.Now.AddDays(2),
                    AssignedTo = "Jane Smith",
                    Category = "Security",
                    EstimatedHours = 16,
                    ActualHours = 8,
                    CompletionPercentage = 50
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "Design Database Schema",
                    Description = "Create ER diagram and define table structures",
                    Priority = TaskPriority.High,
                    Status = TaskManager.Core.Models.TaskStatus.NotStarted,
                    CreatedDate = DateTime.Now.AddDays(-1),
                    DueDate = DateTime.Now.AddDays(5),
                    AssignedTo = "Bob Johnson",
                    Category = "Database",
                    EstimatedHours = 8,
                    ActualHours = 0,
                    CompletionPercentage = 0
                }
            );
        }
    }
} 