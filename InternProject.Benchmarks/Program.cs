using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using InternProject.Business.MappingAlgoritms;
using InternProject.Core;
using InternProject.Core.Filters;
using InternProject.Core.Properties;
using System.Collections.Generic;
using System;

namespace InternProject.Benchmarks
{
    [MemoryDiagnoser]
    public class MappingBenchmarks
    {
        private DataSourceRequest _request;

        [GlobalSetup]
        public void Setup()
        {
            var filters = new List<FilterDescriptor>
            {
                new FilterDescriptor { Member = "EstimatedCost", Operator = FilterOperator.IsEqualTo, Value = 1000 },
                new FilterDescriptor { Member = "TaskName", Operator = FilterOperator.Contains, Value = "MongoDB" },
                new FilterDescriptor { Member = "IsCompleted", Operator = FilterOperator.IsEqualTo, Value = false },
                new FilterDescriptor { Member = "IsActive", Operator = FilterOperator.IsEqualTo, Value = true },
                new FilterDescriptor { Member = "StoryPoints", Operator = FilterOperator.IsEqualTo, Value = 5 },
                new FilterDescriptor { Member = "SprintNumber", Operator = FilterOperator.IsEqualTo, Value = 2 },
                new FilterDescriptor { Member = "Category", Operator = FilterOperator.IsEqualTo, Value = "Development" },
                new FilterDescriptor { Member = "DueDate", Operator = FilterOperator.IsGreaterThan, Value = DateTime.UtcNow.AddDays(7) },
                new FilterDescriptor { Member = "CreatedAfter", Operator = FilterOperator.IsLessThan, Value = DateTime.UtcNow.AddDays(-7) }
            };

            _request = new DataSourceRequest
            {
                PageSize = 50,
                PageNumber = 1,
                Filter = new CompositeFilterDescriptor
                {
                    Operator = FilterCompositionLogicalOperator.Or,
                    FilterDescriptors = filters
                }
            };
        }

        [Benchmark]
        public DetailedFilterDto CteMappingTest()
        {
            return TelerikToDtoCetMapping.MapToDto<DetailedFilterDto>(_request);
        }
        [Benchmark(Baseline = true)]
        public DetailedFilterDto DirectMappingTest()
        {
            return TelerikToDtoDirectMapping.MapToDto(_request);
        }

        [Benchmark]
        public DetailedFilterDto ReflectionMappingTest()
        {
            return TelerikToDtoReflectionMapping.MapToDto<DetailedFilterDto>(_request);
        }


    }

    class Program
    {
        static void Main(string[] args)
        {


            try
            {
                var config = new DebugInProcessConfig();

                var summary = BenchmarkRunner.Run<MappingBenchmarks>(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Benchmark Error]: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Detay]: {ex.InnerException.Message}");
                }
            }

            Console.WriteLine("\nProcess completed...");
            Console.ReadLine();
        }
    }
}