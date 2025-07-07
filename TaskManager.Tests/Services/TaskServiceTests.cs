using FluentAssertions;
using Moq;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using TaskManager.Services;
using Xunit;

namespace TaskManager.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _mockRepository;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _mockRepository = new Mock<ITaskRepository>();
            _taskService = new TaskService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetAllTasksAsync_ShouldReturnAllTasks()
        {
            // Arrange
            var expectedTasks = new List<TaskItem>
            {
                new TaskItem { Id = 1, Title = "Task 1" },
                new TaskItem { Id = 2, Title = "Task 2" }
            };
            _mockRepository.Setup(r => r.GetAllTasksAsync())
                .ReturnsAsync(expectedTasks);

            // Act
            var result = await _taskService.GetAllTasksAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedTasks);
            _mockRepository.Verify(r => r.GetAllTasksAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WhenTaskExists_ShouldReturnTask()
        {
            // Arrange
            var taskId = 1;
            var expectedTask = new TaskItem { Id = taskId, Title = "Test Task" };
            _mockRepository.Setup(r => r.GetTaskByIdAsync(taskId))
                .ReturnsAsync(expectedTask);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

            // Assert
            result.Should().BeEquivalentTo(expectedTask);
        }

        [Fact]
        public async Task CreateTaskAsync_WithValidTask_ShouldCreateAndReturnTask()
        {
            // Arrange
            var newTask = new TaskItem
            {
                Title = "New Task",
                Description = "Description",
                Priority = TaskPriority.High,
                Status = TaskStatus.NotStarted,
                EstimatedHours = 8
            };

            _mockRepository.Setup(r => r.AddTaskAsync(It.IsAny<TaskItem>()))
                .ReturnsAsync((TaskItem t) => 
                {
                    t.Id = 1;
                    return t;
                });

            // Act
            var result = await _taskService.CreateTaskAsync(newTask);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Title.Should().Be("New Task");
            _mockRepository.Verify(r => r.AddTaskAsync(It.IsAny<TaskItem>()), Times.Once);
        }

        [Fact]
        public async Task CreateTaskAsync_WithInvalidTask_ShouldThrowException()
        {
            // Arrange
            var invalidTask = new TaskItem
            {
                Title = "", // Invalid: empty title
                EstimatedHours = -5 // Invalid: negative hours
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _taskService.CreateTaskAsync(invalidTask));
        }

        [Fact]
        public async Task ValidateTaskAsync_WithValidTask_ShouldReturnTrue()
        {
            // Arrange
            var validTask = new TaskItem
            {
                Title = "Valid Task",
                EstimatedHours = 8,
                ActualHours = 4,
                CompletionPercentage = 50,
                CreatedDate = DateTime.Now.AddDays(-1),
                DueDate = DateTime.Now.AddDays(7)
            };

            // Act
            var result = await _taskService.ValidateTaskAsync(validTask);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateTaskAsync_WithEmptyTitle_ShouldReturnFalse()
        {
            // Arrange
            var task = new TaskItem { Title = "" };

            // Act
            var result = await _taskService.ValidateTaskAsync(task);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateTaskAsync_WithNegativeHours_ShouldReturnFalse()
        {
            // Arrange
            var task = new TaskItem
            {
                Title = "Task",
                EstimatedHours = -1
            };

            // Act
            var result = await _taskService.ValidateTaskAsync(task);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CalculateTaskMetricsAsync_ForCompletedTask_ShouldSet100PercentCompletion()
        {
            // Arrange
            var task = new TaskItem
            {
                Status = TaskStatus.Completed,
                CompletionPercentage = 50 // Should be overridden
            };

            // Act
            var result = await _taskService.CalculateTaskMetricsAsync(task);

            // Assert
            result.CompletionPercentage.Should().Be(100);
        }

        [Fact]
        public async Task CalculateTaskMetricsAsync_ForNotStartedTask_ShouldSet0PercentCompletion()
        {
            // Arrange
            var task = new TaskItem
            {
                Status = TaskStatus.NotStarted,
                CompletionPercentage = 50 // Should be overridden
            };

            // Act
            var result = await _taskService.CalculateTaskMetricsAsync(task);

            // Assert
            result.CompletionPercentage.Should().Be(0);
        }

        [Fact]
        public async Task GetTaskStatisticsAsync_ShouldReturnCorrectCounts()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem { Status = TaskStatus.NotStarted },
                new TaskItem { Status = TaskStatus.InProgress },
                new TaskItem { Status = TaskStatus.InProgress },
                new TaskItem { Status = TaskStatus.Completed },
                new TaskItem { Status = TaskStatus.Completed },
                new TaskItem { Status = TaskStatus.Completed }
            };

            _mockRepository.Setup(r => r.GetAllTasksAsync())
                .ReturnsAsync(tasks);

            // Act
            var statistics = await _taskService.GetTaskStatisticsAsync();

            // Assert
            statistics[TaskStatus.NotStarted].Should().Be(1);
            statistics[TaskStatus.InProgress].Should().Be(2);
            statistics[TaskStatus.Completed].Should().Be(3);
            statistics[TaskStatus.Cancelled].Should().Be(0);
            statistics[TaskStatus.OnHold].Should().Be(0);
        }

        [Fact]
        public async Task DeleteTaskAsync_WhenTaskExists_ShouldReturnTrue()
        {
            // Arrange
            var taskId = 1;
            _mockRepository.Setup(r => r.DeleteTaskAsync(taskId))
                .ReturnsAsync(true);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(r => r.DeleteTaskAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task SearchTasksAsync_ShouldCallRepositorySearch()
        {
            // Arrange
            var searchTerm = "test";
            var expectedTasks = new List<TaskItem>
            {
                new TaskItem { Title = "Test Task 1" },
                new TaskItem { Title = "Test Task 2" }
            };

            _mockRepository.Setup(r => r.SearchTasksAsync(searchTerm))
                .ReturnsAsync(expectedTasks);

            // Act
            var result = await _taskService.SearchTasksAsync(searchTerm);

            // Assert
            result.Should().BeEquivalentTo(expectedTasks);
            _mockRepository.Verify(r => r.SearchTasksAsync(searchTerm), Times.Once);
        }
    }
} 