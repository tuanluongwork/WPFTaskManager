using System;

namespace TaskManager.Core.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int EstimatedHours { get; set; }
        public int ActualHours { get; set; }
        public double CompletionPercentage { get; set; }
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Cancelled,
        OnHold
    }
} 