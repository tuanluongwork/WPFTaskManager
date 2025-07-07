using BenchmarkDotNet.Running;
using TaskManager.Benchmarks;

Console.WriteLine("Task Manager Performance Benchmarks");
Console.WriteLine("===================================");

var summary = BenchmarkRunner.Run<TaskServiceBenchmarks>(); 