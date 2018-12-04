# Performance Benchmark Repo

This is just a collection of various benchmarks illustrating the performance benefits or pitfalls of different approaches.

To run any class of benchmark, all that's needed is the class name of the benchmark:
```
dotner build
cd Benchmarks
dotnet run -c Release --framework netcoreapp2.1 -- Regex
```
Without the `-- Regex` argument, a list will be presented like this:
```
λ dotnet run -c Release --framework netcoreapp2.1
Please, select benchmark, list of available:
Allocation
Conditional
DictionaryCultureInfo
DictionaryVsArray
DLR
Exception
Find
Foreach
JSON
Lambda
LINQ
ManyReadersRareWrite
Regex
StaticConstructors
Trim
TimeSpan
```

Often these are the result of questions on Twitter or crazy ideas. 
So that others may benefit, I've created a repo to maintain here, rather than one-off test rigs on my machine only.

If you have any questions, I'm usually available in realtime at [@Nick_Craver](https://twitter.com/Nick_Craver)

Note: The repo uses the new Visual Studio 2017 minimal `.csproj` format.