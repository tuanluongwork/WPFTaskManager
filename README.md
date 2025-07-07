# WPF Task Management System

A modern WPF application demonstrating MVVM pattern, IoC container, and enterprise-level best practices.

## 🚀 Features

- **MVVM Architecture** with proper separation of concerns
- **Dependency Injection** using Microsoft.Extensions.DependencyInjection
- **Command Pattern** implementation
- **Data Binding** with INotifyPropertyChanged
- **Unit Testing** with xUnit and Moq
- **Performance Optimized** with virtualization and async operations
- **Material Design** UI with modern aesthetics
- **Entity Framework Core** for data persistence
- **Logging** with Serilog
- **CI/CD Pipeline** with GitHub Actions

## 🛠️ Technologies

- **WPF** (.NET 8.0)
- **C#** 12.0
- **Entity Framework Core** 8.0
- **xUnit** for unit testing
- **Moq** for mocking
- **Serilog** for logging
- **Material Design in XAML**
- **Microsoft.Extensions.DependencyInjection** for IoC

## 📋 Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or later
- SQL Server LocalDB (for database)

## 🏗️ Architecture

```
TaskManager/
├── TaskManager.Core/           # Domain models and interfaces
├── TaskManager.Data/           # Data access layer with EF Core
├── TaskManager.Services/       # Business logic services
├── TaskManager.WPF/           # WPF presentation layer
│   ├── ViewModels/            # MVVM ViewModels
│   ├── Views/                 # XAML Views
│   ├── Commands/              # ICommand implementations
│   └── Converters/            # Value converters
├── TaskManager.Tests/         # Unit tests
└── TaskManager.Benchmarks/    # Performance benchmarks
```

## 🚦 Getting Started

1. Clone the repository
```bash
git clone https://github.com/yourusername/WPF-MVVM-IoC-Demo.git
```

2. Restore NuGet packages
```bash
dotnet restore
```

3. Update database
```bash
dotnet ef database update -p TaskManager.Data -s TaskManager.WPF
```

4. Run the application
```bash
dotnet run --project TaskManager.WPF
```

## 🧪 Running Tests

```bash
dotnet test
```

## 📊 Performance Benchmarks

```bash
dotnet run --project TaskManager.Benchmarks -c Release
```

## 🔧 Configuration

Application settings can be configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagerDb;Trusted_Connection=True;"
  },
  "Logging": {
    "MinimumLevel": "Information"
  }
}
```

## 📦 NuGet Packages

- Microsoft.Extensions.DependencyInjection
- Microsoft.EntityFrameworkCore.SqlServer
- MaterialDesignThemes
- Serilog.Sinks.File
- xUnit
- Moq
- BenchmarkDotNet

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🎯 Skills Demonstrated

- ✅ WPF with MVVM pattern
- ✅ IoC/Dependency Injection
- ✅ C# and .NET 8
- ✅ Test-Driven Development
- ✅ Performance optimization
- ✅ Git/GitHub
- ✅ CI/CD with GitHub Actions
- ✅ Clean Architecture
- ✅ SOLID principles
- ✅ Async/await patterns
- ✅ Entity Framework Core
- ✅ Unit testing with mocking 