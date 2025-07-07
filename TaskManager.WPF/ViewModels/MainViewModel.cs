using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Serilog;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using TaskManager.WPF.Commands;
using CoreModels = TaskManager.Core.Models;

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
        private readonly Dictionary<int, CancellationTokenSource> _saveDebounceTimers = new();

        public MainViewModel(ITaskService taskService, ILogger logger)
        {
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Tasks = new ObservableCollection<TaskItemViewModel>();
            
            // Initialize commands
            LoadTasksCommand = new AsyncRelayCommand(async _ => await LoadTasksAsync());
            AddTaskCommand = new AsyncRelayCommand(async _ => await AddTaskAsync());
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
        public ICommand DeleteTaskCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }

        // Statistics properties
        public int TotalTasks => Tasks.Count;
        public int CompletedTasks => Tasks.Count(t => t.Status == CoreModels.TaskStatus.Completed);
        public int OverdueTasks => Tasks.Count(t => t.IsOverdue);
        public int InProgressTasks => Tasks.Count(t => t.Status == CoreModels.TaskStatus.InProgress);

        private async Task LoadTasksAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading tasks...";
                _logger.Information("Loading tasks");

                var tasks = await _taskService.GetAllTasksAsync();
                
                // Clean up existing tasks
                foreach (var existingTask in Tasks)
                {
                    existingTask.TaskModified -= OnTaskModified;
                    existingTask.StatisticsChanged -= OnStatisticsChanged;
                }
                
                // Cancel all pending saves
                foreach (var kvp in _saveDebounceTimers)
                {
                    kvp.Value.Cancel();
                    kvp.Value.Dispose();
                }
                _saveDebounceTimers.Clear();
                
                Tasks.Clear();
                foreach (var task in tasks)
                {
                    var taskViewModel = new TaskItemViewModel(task);
                    taskViewModel.TaskModified += OnTaskModified;
                    taskViewModel.StatisticsChanged += OnStatisticsChanged;
                    Tasks.Add(taskViewModel);
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

        private void OnStatisticsChanged(object? sender, EventArgs e)
        {
            // Update statistics immediately when relevant properties change
            UpdateStatistics();
        }

        private async void OnTaskModified(object? sender, EventArgs e)
        {
            if (sender is TaskItemViewModel taskViewModel)
            {
                // Cancel any existing save operation for this task
                if (_saveDebounceTimers.TryGetValue(taskViewModel.Id, out var existingCts))
                {
                    existingCts.Cancel();
                    existingCts.Dispose();
                }

                // Create a new cancellation token for this save operation
                var cts = new CancellationTokenSource();
                _saveDebounceTimers[taskViewModel.Id] = cts;

                try
                {
                    // Wait 1 second before saving (debounce)
                    await Task.Delay(1000, cts.Token);
                    
                    // If we get here without being cancelled, perform the save
                    await AutoSaveTaskAsync(taskViewModel);
                    
                    // Remove the timer after successful save
                    _saveDebounceTimers.Remove(taskViewModel.Id);
                }
                catch (TaskCanceledException)
                {
                    // This is expected when a new change comes in before the delay expires
                }
            }
        }

        private async Task AutoSaveTaskAsync(TaskItemViewModel taskViewModel)
        {
            try
            {
                // Recalculate task metrics before saving
                var updatedTask = await _taskService.CalculateTaskMetricsAsync(taskViewModel.Model);
                await _taskService.UpdateTaskAsync(updatedTask);
                
                taskViewModel.ResetDirtyFlag();
                StatusMessage = $"Auto-saved: {taskViewModel.Title}";
                _logger.Information("Auto-saved task: {TaskId}", taskViewModel.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error auto-saving task");
                StatusMessage = $"Auto-save failed: {ex.Message}";
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
                    Status = CoreModels.TaskStatus.NotStarted,
                    AssignedTo = "Unassigned",
                    Category = "General",
                    EstimatedHours = 8,
                    CreatedDate = DateTime.Now
                };

                var createdTask = await _taskService.CreateTaskAsync(newTask);
                var taskViewModel = new TaskItemViewModel(createdTask);
                taskViewModel.TaskModified += OnTaskModified;
                taskViewModel.StatisticsChanged += OnStatisticsChanged;
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

        private async Task DeleteTaskAsync(TaskItemViewModel? taskViewModel)
        {
            if (taskViewModel == null) return;

            try
            {
                var result = await _taskService.DeleteTaskAsync(taskViewModel.Id);
                if (result)
                {
                    // Unsubscribe from events to avoid memory leaks
                    taskViewModel.TaskModified -= OnTaskModified;
                    taskViewModel.StatisticsChanged -= OnStatisticsChanged;
                    
                    // Cancel any pending save operations
                    if (_saveDebounceTimers.TryGetValue(taskViewModel.Id, out var cts))
                    {
                        cts.Cancel();
                        cts.Dispose();
                        _saveDebounceTimers.Remove(taskViewModel.Id);
                    }
                    
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

                // Clean up existing tasks
                foreach (var existingTask in Tasks)
                {
                    existingTask.TaskModified -= OnTaskModified;
                    existingTask.StatisticsChanged -= OnStatisticsChanged;
                }
                
                // Cancel all pending saves
                foreach (var kvp in _saveDebounceTimers)
                {
                    kvp.Value.Cancel();
                    kvp.Value.Dispose();
                }
                _saveDebounceTimers.Clear();

                Tasks.Clear();
                foreach (var task in tasks)
                {
                    var taskViewModel = new TaskItemViewModel(task);
                    taskViewModel.TaskModified += OnTaskModified;
                    taskViewModel.StatisticsChanged += OnStatisticsChanged;
                    Tasks.Add(taskViewModel);
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