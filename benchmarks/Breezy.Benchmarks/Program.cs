// See https://aka.ms/new-console-template for more information

//Console.WriteLine("Hello, World!");

using BenchmarkDotNet.Running;
using Breezy.Benchmarks;

BenchmarkRunner.Run<CustomBenchmarks>();

/*var test = new CustomBenchmarks();
test.GlobalSetup();
await test.QueryAsyncOneToManyRows_WithBreezy();*/

Console.ReadLine();