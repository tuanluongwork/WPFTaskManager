using System;
using TaskManager.Core.Models;
using CoreModels = TaskManager.Core.Models;

namespace TaskManager.WPF.ViewModels
{
    public class TaskItemViewModel : ViewModelBase
    {
        private readonly TaskItem _taskItem;
        private bool _isDirty;

        public event EventHandler? TaskModified;
        public event EventHandler? StatisticsChanged;

        public TaskItemViewModel(TaskItem taskItem)
        {
            _taskItem = taskItem ?? throw new ArgumentNullException(nameof(taskItem));
        }

        public TaskItem Model => _taskItem;

        public int Id => _taskItem.Id;

        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    if (_isDirty)
                    {
                        TaskModified?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public string Title
        {
            get => _taskItem.Title;
            set
            {
                if (_taskItem.Title != value)
                {
                    _taskItem.Title = value;
                    OnPropertyChanged();
                    IsDirty = true;
                }
            }
        }

        public string Description
        {
            get => _taskItem.Description;
            set
            {
                if (_taskItem.Description != value)
                {
                    _taskItem.Description = value;
                    OnPropertyChanged();
                    IsDirty = true;
                }
            }
        }

        public TaskPriority Priority
        {
            get => _taskItem.Priority;
            set
            {
                if (_taskItem.Priority != value)
                {
                    _taskItem.Priority = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PriorityColor));
                    IsDirty = true;
                }
            }
        }

        public CoreModels.TaskStatus Status
        {
            get => _taskItem.Status;
            set
            {
                if (_taskItem.Status != value)
                {
                    var previousStatus = _taskItem.Status;
                    _taskItem.Status = value;
                    
                    // Auto-calculate completion percentage based on status
                    switch (value)
                    {
                        case CoreModels.TaskStatus.NotStarted:
                            _taskItem.CompletionPercentage = 0;
                            break;
                        case CoreModels.TaskStatus.Completed:
                            _taskItem.CompletionPercentage = 100;
                            _taskItem.CompletedDate = DateTime.Now;
                            break;
                        case CoreModels.TaskStatus.InProgress:
                            if (_taskItem.CompletionPercentage == 0 || _taskItem.CompletionPercentage == 100)
                            {
                                _taskItem.CompletionPercentage = 50; // Default to 50% for in progress
                            }
                            break;
                    }
                    
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusColor));
                    OnPropertyChanged(nameof(IsCompleted));
                    OnPropertyChanged(nameof(IsOverdue));
                    OnPropertyChanged(nameof(CompletionPercentage));
                    
                    // Notify statistics change immediately
                    StatisticsChanged?.Invoke(this, EventArgs.Empty);
                    
                    IsDirty = true;
                }
            }
        }

        public DateTime CreatedDate => _taskItem.CreatedDate;

        public DateTime? DueDate
        {
            get => _taskItem.DueDate;
            set
            {
                if (_taskItem.DueDate != value)
                {
                    _taskItem.DueDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsOverdue));
                    
                    // If overdue status might have changed, notify statistics
                    StatisticsChanged?.Invoke(this, EventArgs.Empty);
                    
                    IsDirty = true;
                }
            }
        }

        public DateTime? CompletedDate => _taskItem.CompletedDate;

        public string AssignedTo
        {
            get => _taskItem.AssignedTo;
            set
            {
                if (_taskItem.AssignedTo != value)
                {
                    _taskItem.AssignedTo = value;
                    OnPropertyChanged();
                    IsDirty = true;
                }
            }
        }

        public string Category
        {
            get => _taskItem.Category;
            set
            {
                if (_taskItem.Category != value)
                {
                    _taskItem.Category = value;
                    OnPropertyChanged();
                }
            }
        }

        public int EstimatedHours
        {
            get => _taskItem.EstimatedHours;
            set
            {
                if (_taskItem.EstimatedHours != value)
                {
                    _taskItem.EstimatedHours = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ActualHours
        {
            get => _taskItem.ActualHours;
            set
            {
                if (_taskItem.ActualHours != value)
                {
                    _taskItem.ActualHours = value;
                    OnPropertyChanged();
                }
            }
        }

        public double CompletionPercentage
        {
            get => _taskItem.CompletionPercentage;
            set
            {
                if (_taskItem.CompletionPercentage != value)
                {
                    _taskItem.CompletionPercentage = value;
                    OnPropertyChanged();
                    IsDirty = true;
                }
            }
        }

        public void ResetDirtyFlag()
        {
            _isDirty = false;
        }

        // UI-specific properties
        public bool IsCompleted => Status == CoreModels.TaskStatus.Completed;

        public bool IsOverdue => DueDate.HasValue && 
                                 DueDate.Value < DateTime.Now && 
                                 Status != CoreModels.TaskStatus.Completed && 
                                 Status != CoreModels.TaskStatus.Cancelled;

        public string PriorityColor => Priority switch
        {
            TaskPriority.Critical => "#D32F2F",
            TaskPriority.High => "#F57C00",
            TaskPriority.Medium => "#388E3C",
            TaskPriority.Low => "#1976D2",
            _ => "#757575"
        };

        public string StatusColor => Status switch
        {
            CoreModels.TaskStatus.Completed => "#388E3C",
            CoreModels.TaskStatus.InProgress => "#1976D2",
            CoreModels.TaskStatus.NotStarted => "#757575",
            CoreModels.TaskStatus.OnHold => "#F57C00",
            CoreModels.TaskStatus.Cancelled => "#D32F2F",
            _ => "#757575"
        };
    }
} 