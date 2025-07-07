using System;
using TaskManager.Core.Models;
using CoreModels = TaskManager.Core.Models;

namespace TaskManager.WPF.ViewModels
{
    public class TaskItemViewModel : ViewModelBase
    {
        private readonly TaskItem _taskItem;

        public TaskItemViewModel(TaskItem taskItem)
        {
            _taskItem = taskItem ?? throw new ArgumentNullException(nameof(taskItem));
        }

        public TaskItem Model => _taskItem;

        public int Id => _taskItem.Id;

        public string Title
        {
            get => _taskItem.Title;
            set
            {
                if (_taskItem.Title != value)
                {
                    _taskItem.Title = value;
                    OnPropertyChanged();
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
                    _taskItem.Status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusColor));
                    OnPropertyChanged(nameof(IsCompleted));
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
                }
            }
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