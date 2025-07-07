using FluentAssertions;
using Moq;
using Serilog;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using TaskManager.WPF.ViewModels;
using Xunit;
using CoreModels = TaskManager.Core.Models;

namespace TaskManager.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly Mock<ILogger> _mockLogger;
        private readonly MainViewModel _viewModel;

        public MainViewModelTests()
        {
            _mockTaskService = new Mock<ITaskService>();
            _mockLogger = new Mock<ILogger>();
            _viewModel = new MainViewModel(_mockTaskService.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldInitializeCommands()
        {
            // Assert
            _viewModel.LoadTasksCommand.Should().NotBeNull();
            _viewModel.AddTaskCommand.Should().NotBeNull();
            _viewModel.EditTaskCommand.Should().NotBeNull();
            _viewModel.DeleteTaskCommand.Should().NotBeNull();
            _viewModel.SearchCommand.Should().NotBeNull();
            _viewModel.RefreshCommand.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldInitializeTasksCollection()
        {
            // Assert
            _viewModel.Tasks.Should().NotBeNull();
            _viewModel.Tasks.Should().BeEmpty();
        }

        [Fact]
        public async Task LoadTasksCommand_ShouldPopulateTasksCollection()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem { Id = 1, Title = "Task 1", Status = CoreModels.TaskStatus.NotStarted },
                new TaskItem { Id = 2, Title = "Task 2", Status = CoreModels.TaskStatus.Completed }
            };

            _mockTaskService.Setup(s => s.GetAllTasksAsync())
                .ReturnsAsync(tasks);

            // Act
            _viewModel.LoadTasksCommand.Execute(null);
            await Task.Delay(100); // Give async command time to complete

            // Assert
            _viewModel.Tasks.Should().HaveCount(2);
            _viewModel.Tasks[0].Title.Should().Be("Task 1");
            _viewModel.Tasks[1].Title.Should().Be("Task 2");
            _viewModel.StatusMessage.Should().Be("Loaded 2 tasks");
        }

        [Fact]
        public void TotalTasks_ShouldReturnCorrectCount()
        {
            // Arrange
            _viewModel.Tasks.Add(new TaskItemViewModel(new TaskItem { Id = 1 }));
            _viewModel.Tasks.Add(new TaskItemViewModel(new TaskItem { Id = 2 }));

            // Act & Assert
            _viewModel.TotalTasks.Should().Be(2);
        }

        [Fact]
        public void CompletedTasks_ShouldReturnCorrectCount()
        {
            // Arrange
            _viewModel.Tasks.Add(new TaskItemViewModel(new TaskItem { Status = CoreModels.TaskStatus.Completed }));
            _viewModel.Tasks.Add(new TaskItemViewModel(new TaskItem { Status = CoreModels.TaskStatus.InProgress }));
            _viewModel.Tasks.Add(new TaskItemViewModel(new TaskItem { Status = CoreModels.TaskStatus.Completed }));

            // Act & Assert
            _viewModel.CompletedTasks.Should().Be(2);
        }

        [Fact]
        public void OverdueTasks_ShouldReturnCorrectCount()
        {
            // Arrange
            var overdueTask = new TaskItem
            {
                DueDate = DateTime.Now.AddDays(-1),
                Status = CoreModels.TaskStatus.InProgress
            };
            var notOverdueTask = new TaskItem
            {
                DueDate = DateTime.Now.AddDays(1),
                Status = CoreModels.TaskStatus.InProgress
            };

            _viewModel.Tasks.Add(new TaskItemViewModel(overdueTask));
            _viewModel.Tasks.Add(new TaskItemViewModel(notOverdueTask));

            // Act & Assert
            _viewModel.OverdueTasks.Should().Be(1);
        }

        [Fact]
        public async Task AddTaskCommand_ShouldAddNewTaskToCollection()
        {
            // Arrange
            var newTask = new TaskItem
            {
                Id = 1,
                Title = "New Task",
                Status = CoreModels.TaskStatus.NotStarted
            };

            _mockTaskService.Setup(s => s.CreateTaskAsync(It.IsAny<TaskItem>()))
                .ReturnsAsync(newTask);

            // Act
            _viewModel.AddTaskCommand.Execute(null);
            await Task.Delay(100); // Give async command time to complete

            // Assert
            _viewModel.Tasks.Should().HaveCount(1);
            _viewModel.Tasks[0].Title.Should().Be("New Task");
            _viewModel.SelectedTask.Should().Be(_viewModel.Tasks[0]);
            _viewModel.StatusMessage.Should().Be("Task added successfully");
        }

        [Fact]
        public async Task DeleteTaskCommand_ShouldRemoveTaskFromCollection()
        {
            // Arrange
            var task = new TaskItem { Id = 1, Title = "Task to Delete" };
            var taskViewModel = new TaskItemViewModel(task);
            _viewModel.Tasks.Add(taskViewModel);

            _mockTaskService.Setup(s => s.DeleteTaskAsync(1))
                .ReturnsAsync(true);

            // Act
            _viewModel.DeleteTaskCommand.Execute(taskViewModel);
            await Task.Delay(100); // Give async command time to complete

            // Assert
            _viewModel.Tasks.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Be("Task deleted successfully");
        }

        [Fact]
        public async Task SearchCommand_WithSearchText_ShouldFilterTasks()
        {
            // Arrange
            _viewModel.SearchText = "test";
            var searchResults = new List<TaskItem>
            {
                new TaskItem { Id = 1, Title = "Test Task" }
            };

            _mockTaskService.Setup(s => s.SearchTasksAsync("test"))
                .ReturnsAsync(searchResults);

            // Act
            _viewModel.SearchCommand.Execute(null);
            await Task.Delay(100); // Give async command time to complete

            // Assert
            _viewModel.Tasks.Should().HaveCount(1);
            _viewModel.Tasks[0].Title.Should().Be("Test Task");
            _viewModel.StatusMessage.Should().Be("Found 1 tasks");
        }

        [Fact]
        public void SelectedTask_WhenSet_ShouldRaisePropertyChanged()
        {
            // Arrange
            var task = new TaskItemViewModel(new TaskItem { Id = 1 });
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.SelectedTask))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.SelectedTask = task;

            // Assert
            propertyChangedRaised.Should().BeTrue();
            _viewModel.SelectedTask.Should().Be(task);
        }

        [Fact]
        public void IsLoading_WhenSet_ShouldRaisePropertyChanged()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.IsLoading))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.IsLoading = true;

            // Assert
            propertyChangedRaised.Should().BeTrue();
            _viewModel.IsLoading.Should().BeTrue();
        }
    }
} 