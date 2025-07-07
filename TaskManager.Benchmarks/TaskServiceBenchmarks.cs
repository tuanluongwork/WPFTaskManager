using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using TaskManager.Services;
using CoreModels = TaskManager.Core.Models;

namespace TaskManager.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    [RankColumn]
    public class TaskServiceBenchmarks
    {
        private ITaskService _taskService;
        private ITaskRepository _mockRepository;
        private List<TaskItem> _sampleTasks;

        [GlobalSetup]
        public void Setup()
        {
            _mockRepository = new InMemoryTaskRepository();
            _taskService = new TaskService(_mockRepository);
            
            _sampleTasks = GenerateSampleTasks(1000);
            foreach (var task in _sampleTasks)
            {
                _mockRepository.AddTaskAsync(task).GetAwaiter().GetResult();
            }
        }

        [Benchmark]
        public async Task GetAllTasks()
        {
            await _taskService.GetAllTasksAsync();
        }

        [Benchmark]
        public async Task SearchTasks()
        {
            await _taskService.SearchTasksAsync("Task");
        }

        [Benchmark]
        public async Task GetTaskStatistics()
        {
            await _taskService.GetTaskStatisticsAsync();
        }

        [Benchmark]
        public async Task CreateNewTask()
        {
            var newTask = new TaskItem
            {
                Title = "Benchmark Task",
                Description = "This is a benchmark task",
                Priority = TaskPriority.Medium,
                Status = CoreModels.TaskStatus.NotStarted,
                EstimatedHours = 8
            };

            await _taskService.CreateTaskAsync(newTask);
        }

        [Benchmark]
        public async Task ValidateTask()
        {
            var task = new TaskItem
            {
                Title = "Valid Task",
                EstimatedHours = 8,
                ActualHours = 4,
                CompletionPercentage = 50,
                CreatedDate = DateTime.Now.AddDays(-1),
                DueDate = DateTime.Now.AddDays(7)
            };

            await _taskService.ValidateTaskAsync(task);
        }

        [Benchmark]
        public async Task CalculateMetrics()
        {
            var task = new TaskItem
            {
                Status = CoreModels.TaskStatus.InProgress,
                EstimatedHours = 10,
                ActualHours = 5
            };

            await _taskService.CalculateTaskMetricsAsync(task);
        }

        private List<TaskItem> GenerateSampleTasks(int count)
        {
            var tasks = new List<TaskItem>();
            var random = new Random();
            var statuses = Enum.GetValues<CoreModels.TaskStatus>();
            var priorities = Enum.GetValues<TaskPriority>();

            for (int i = 0; i < count; i++)
            {
                tasks.Add(new TaskItem
                {
                    Id = i + 1,
                    Title = $"Task {i + 1}",
                    Description = $"Description for task {i + 1}",
                    Status = statuses[random.Next(statuses.Length)],
                    Priority = priorities[random.Next(priorities.Length)],
                    CreatedDate = DateTime.Now.AddDays(-random.Next(30)),
                    DueDate = DateTime.Now.AddDays(random.Next(30)),
                    AssignedTo = $"User {random.Next(1, 10)}",
                    Category = $"Category {random.Next(1, 5)}",
                    EstimatedHours = random.Next(1, 40),
                    ActualHours = random.Next(0, 20),
                    CompletionPercentage = random.Next(0, 101)
                });
            }

            return tasks;
        }
    }

    // Simple in-memory implementation for benchmarking
    public class InMemoryTaskRepository : ITaskRepository
    {
        private readonly List<TaskItem> _tasks = new();
        private int _nextId = 1;

        public Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            return Task.FromResult<IEnumerable<TaskItem>>(_tasks.ToList());
        }

        public Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            return Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id));
        }

        public Task<TaskItem> AddTaskAsync(TaskItem task)
        {
            task.Id = _nextId++;
            _tasks.Add(task);
            return Task.FromResult(task);
        }

        public Task<TaskItem> UpdateTaskAsync(TaskItem task)
        {
            var index = _tasks.FindIndex(t => t.Id == task.Id);
            if (index >= 0)
            {
                _tasks[index] = task;
            }
            return Task.FromResult(task);
        }

        public Task<bool> DeleteTaskAsync(int id)
        {
            var removed = _tasks.RemoveAll(t => t.Id == id) > 0;
            return Task.FromResult(removed);
        }

        public Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(CoreModels.TaskStatus status)
        {
            return Task.FromResult<IEnumerable<TaskItem>>(
                _tasks.Where(t => t.Status == status).ToList());
        }

        public Task<IEnumerable<TaskItem>> GetTasksByPriorityAsync(TaskPriority priority)
        {
            return Task.FromResult<IEnumerable<TaskItem>>(
                _tasks.Where(t => t.Priority == priority).ToList());
        }

        public Task<IEnumerable<TaskItem>> GetTasksByAssigneeAsync(string assignee)
        {
            return Task.FromResult<IEnumerable<TaskItem>>(
                _tasks.Where(t => t.AssignedTo.Contains(assignee)).ToList());
        }

        public Task<IEnumerable<TaskItem>> GetOverdueTasksAsync()
        {
            var now = DateTime.Now;
            return Task.FromResult<IEnumerable<TaskItem>>(
                _tasks.Where(t => t.DueDate.HasValue && 
                                 t.DueDate.Value < now && 
                                 t.Status != CoreModels.TaskStatus.Completed &&
                                 t.Status != CoreModels.TaskStatus.Cancelled).ToList());
        }

        public Task<IEnumerable<TaskItem>> SearchTasksAsync(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();
            return Task.FromResult<IEnumerable<TaskItem>>(
                _tasks.Where(t => t.Title.ToLower().Contains(searchTerm) ||
                                 t.Description.ToLower().Contains(searchTerm)).ToList());
        }
    }
} 