using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.Core.Models;

namespace TaskManager.Core.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItem>> GetAllTasksAsync();
        Task<TaskItem?> GetTaskByIdAsync(int id);
        Task<TaskItem> CreateTaskAsync(TaskItem task);
        Task<TaskItem> UpdateTaskAsync(TaskItem task);
        Task<bool> DeleteTaskAsync(int id);
        Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(TaskStatus status);
        Task<IEnumerable<TaskItem>> GetTasksByPriorityAsync(TaskPriority priority);
        Task<IEnumerable<TaskItem>> GetTasksByAssigneeAsync(string assignee);
        Task<IEnumerable<TaskItem>> GetOverdueTasksAsync();
        Task<IEnumerable<TaskItem>> SearchTasksAsync(string searchTerm);
        Task<Dictionary<TaskStatus, int>> GetTaskStatisticsAsync();
        Task<bool> ValidateTaskAsync(TaskItem task);
        Task<TaskItem> CalculateTaskMetricsAsync(TaskItem task);
    }
} 