using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using CoreModels = TaskManager.Core.Models;

namespace TaskManager.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            return await _taskRepository.GetAllTasksAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            return await _taskRepository.GetTaskByIdAsync(id);
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            // Validate task
            if (!await ValidateTaskAsync(task))
                throw new InvalidOperationException("Task validation failed");

            // Calculate initial metrics
            task = await CalculateTaskMetricsAsync(task);
            
            return await _taskRepository.AddTaskAsync(task);
        }

        public async Task<TaskItem> UpdateTaskAsync(TaskItem task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            // Validate task
            if (!await ValidateTaskAsync(task))
                throw new InvalidOperationException("Task validation failed");

            // Recalculate metrics
            task = await CalculateTaskMetricsAsync(task);

            return await _taskRepository.UpdateTaskAsync(task);
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            return await _taskRepository.DeleteTaskAsync(id);
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(CoreModels.TaskStatus status)
        {
            return await _taskRepository.GetTasksByStatusAsync(status);
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByPriorityAsync(TaskPriority priority)
        {
            return await _taskRepository.GetTasksByPriorityAsync(priority);
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByAssigneeAsync(string assignee)
        {
            if (string.IsNullOrWhiteSpace(assignee))
                return Enumerable.Empty<TaskItem>();

            return await _taskRepository.GetTasksByAssigneeAsync(assignee);
        }

        public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync()
        {
            return await _taskRepository.GetOverdueTasksAsync();
        }

        public async Task<IEnumerable<TaskItem>> SearchTasksAsync(string searchTerm)
        {
            return await _taskRepository.SearchTasksAsync(searchTerm);
        }

        public async Task<Dictionary<CoreModels.TaskStatus, int>> GetTaskStatisticsAsync()
        {
            var allTasks = await _taskRepository.GetAllTasksAsync();
            
            return Enum.GetValues<CoreModels.TaskStatus>()
                .ToDictionary(
                    status => status,
                    status => allTasks.Count(t => t.Status == status)
                );
        }

        public Task<bool> ValidateTaskAsync(TaskItem task)
        {
            if (task == null)
                return Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(task.Title))
                return Task.FromResult(false);

            if (task.EstimatedHours < 0 || task.ActualHours < 0)
                return Task.FromResult(false);

            if (task.CompletionPercentage < 0 || task.CompletionPercentage > 100)
                return Task.FromResult(false);

            if (task.DueDate.HasValue && task.DueDate.Value < task.CreatedDate)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task<TaskItem> CalculateTaskMetricsAsync(TaskItem task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            // Calculate completion percentage based on status
            switch (task.Status)
            {
                case CoreModels.TaskStatus.NotStarted:
                    task.CompletionPercentage = 0;
                    break;
                case CoreModels.TaskStatus.Completed:
                    task.CompletionPercentage = 100;
                    break;
                case CoreModels.TaskStatus.Cancelled:
                    // Keep existing percentage
                    break;
                case CoreModels.TaskStatus.InProgress:
                    // Calculate based on actual vs estimated hours if available
                    if (task.EstimatedHours > 0 && task.ActualHours > 0)
                    {
                        task.CompletionPercentage = Math.Min(100, 
                            (double)task.ActualHours / task.EstimatedHours * 100);
                    }
                    break;
            }

            return Task.FromResult(task);
        }
    }
} 