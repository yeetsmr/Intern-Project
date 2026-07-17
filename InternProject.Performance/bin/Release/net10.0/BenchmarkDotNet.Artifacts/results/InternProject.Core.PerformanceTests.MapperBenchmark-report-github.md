```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8655/25H2/2025Update/HudsonValley2)
13th Gen Intel Core i5-1334U 1.30GHz, 1 CPU, 12 logical and 10 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v3


```
| Method            | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0      | Allocated | Alloc Ratio |
|------------------ |---------:|---------:|---------:|------:|--------:|----------:|----------:|------------:|
| DirectMapping     | 10.69 ms | 0.090 ms | 0.080 ms |  1.00 |    0.01 | 2039.0625 |  12.21 MB |        1.00 |
| ReflectionMapping | 10.40 ms | 0.206 ms | 0.421 ms |  0.97 |    0.04 | 2039.0625 |  12.21 MB |        1.00 |
