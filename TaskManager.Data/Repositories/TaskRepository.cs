using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using TaskManager.Data.Context;

namespace TaskManager.Data.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskManagerDbContext _context;

        public TaskRepository(TaskManagerDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            return await _context.Tasks
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<TaskItem> AddTaskAsync(TaskItem task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.CreatedDate = DateTime.Now;
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem> UpdateTaskAsync(TaskItem task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            _context.Entry(task).State = EntityState.Modified;
            
            // Don't update CreatedDate
            _context.Entry(task).Property(x => x.CreatedDate).IsModified = false;
            
            if (task.Status == TaskStatus.Completed && !task.CompletedDate.HasValue)
            {
                task.CompletedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(TaskStatus status)
        {
            return await _context.Tasks
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.Priority)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByPriorityAsync(TaskPriority priority)
        {
            return await _context.Tasks
                .Where(t => t.Priority == priority)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByAssigneeAsync(string assignee)
        {
            return await _context.Tasks
                .Where(t => t.AssignedTo.Contains(assignee))
                .OrderByDescending(t => t.Priority)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync()
        {
            var now = DateTime.Now;
            return await _context.Tasks
                .Where(t => t.DueDate.HasValue && 
                           t.DueDate.Value < now && 
                           t.Status != TaskStatus.Completed &&
                           t.Status != TaskStatus.Cancelled)
                .OrderByDescending(t => t.Priority)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> SearchTasksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllTasksAsync();

            searchTerm = searchTerm.ToLower();

            return await _context.Tasks
                .Where(t => t.Title.ToLower().Contains(searchTerm) ||
                           t.Description.ToLower().Contains(searchTerm) ||
                           t.AssignedTo.ToLower().Contains(searchTerm) ||
                           t.Category.ToLower().Contains(searchTerm))
                .OrderByDescending(t => t.Priority)
                .ToListAsync();
        }
    }
} 