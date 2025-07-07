using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Serilog;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using TaskManager.WPF.Commands;

namespace TaskManager.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger _logger;
        private TaskItemViewModel? _selectedTask;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private string _statusMessage = string.Empty;

        public MainViewModel(ITaskService taskService, ILogger logger)
        {
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Tasks = new ObservableCollection<TaskItemViewModel>();
            
            // Initialize commands
            LoadTasksCommand = new AsyncRelayCommand(async _ => await LoadTasksAsync());
            AddTaskCommand = new AsyncRelayCommand(async _ => await AddTaskAsync());
            EditTaskCommand = new AsyncRelayCommand<TaskItemViewModel>(async task => await EditTaskAsync(task), 
                task => task != null);
            DeleteTaskCommand = new AsyncRelayCommand<TaskItemViewModel>(async task => await DeleteTaskAsync(task), 
                task => task != null);
            SearchCommand = new AsyncRelayCommand(async _ => await SearchTasksAsync());
            RefreshCommand = new AsyncRelayCommand(async _ => await LoadTasksAsync());
            
            // Load tasks on startup
            _ = LoadTasksAsync();
        }

        public ObservableCollection<TaskItemViewModel> Tasks { get; }

        public TaskItemViewModel? SelectedTask
        {
            get => _selectedTask;
            set => SetProperty(ref _selectedTask, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Commands
        public ICommand LoadTasksCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }

        // Statistics properties
        public int TotalTasks => Tasks.Count;
        public int CompletedTasks => Tasks.Count(t => t.Status == TaskStatus.Completed);
        public int OverdueTasks => Tasks.Count(t => t.IsOverdue);
        public int InProgressTasks => Tasks.Count(t => t.Status == TaskStatus.InProgress);

        private async Task LoadTasksAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading tasks...";
                _logger.Information("Loading tasks");

                var tasks = await _taskService.GetAllTasksAsync();
                
                Tasks.Clear();
                foreach (var task in tasks)
                {
                    Tasks.Add(new TaskItemViewModel(task));
                }

                UpdateStatistics();
                StatusMessage = $"Loaded {Tasks.Count} tasks";
                _logger.Information("Loaded {Count} tasks", Tasks.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading tasks");
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddTaskAsync()
        {
            try
            {
                var newTask = new TaskItem
                {
                    Title = "New Task",
                    Description = "Task description",
                    Priority = TaskPriority.Medium,
                    Status = TaskStatus.NotStarted,
                    AssignedTo = "Unassigned",
                    Category = "General",
                    EstimatedHours = 8
                };

                var createdTask = await _taskService.CreateTaskAsync(newTask);
                var taskViewModel = new TaskItemViewModel(createdTask);
                Tasks.Insert(0, taskViewModel);
                SelectedTask = taskViewModel;

                UpdateStatistics();
                StatusMessage = "Task added successfully";
                _logger.Information("Added new task: {TaskId}", createdTask.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding task");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task EditTaskAsync(TaskItemViewModel? taskViewModel)
        {
            if (taskViewModel == null) return;

            try
            {
                await _taskService.UpdateTaskAsync(taskViewModel.Model);
                UpdateStatistics();
                StatusMessage = "Task updated successfully";
                _logger.Information("Updated task: {TaskId}", taskViewModel.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating task");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task DeleteTaskAsync(TaskItemViewModel? taskViewModel)
        {
            if (taskViewModel == null) return;

            try
            {
                var result = await _taskService.DeleteTaskAsync(taskViewModel.Id);
                if (result)
                {
                    Tasks.Remove(taskViewModel);
                    UpdateStatistics();
                    StatusMessage = "Task deleted successfully";
                    _logger.Information("Deleted task: {TaskId}", taskViewModel.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting task");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task SearchTasksAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Searching...";

                var tasks = string.IsNullOrWhiteSpace(SearchText)
                    ? await _taskService.GetAllTasksAsync()
                    : await _taskService.SearchTasksAsync(SearchText);

                Tasks.Clear();
                foreach (var task in tasks)
                {
                    Tasks.Add(new TaskItemViewModel(task));
                }

                UpdateStatistics();
                StatusMessage = $"Found {Tasks.Count} tasks";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching tasks");
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateStatistics()
        {
            OnPropertyChanged(nameof(TotalTasks));
            OnPropertyChanged(nameof(CompletedTasks));
            OnPropertyChanged(nameof(OverdueTasks));
            OnPropertyChanged(nameof(InProgressTasks));
        }
    }
} 