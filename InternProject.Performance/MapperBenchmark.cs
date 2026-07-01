using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Reflection;
using InternProject;
using InternProject.Core.Filters;
using InternProject.Core.Properties;

namespace InternProject.Core.PerformanceTests
{
    [MemoryDiagnoser] 
    public class MapperBenchmark
    {
        private FilterDto _dto;
        private PropertyInfo[] _properties;

        [GlobalSetup]
        public void Setup()
        {
            _dto = new FilterDto();

            _properties = typeof(FilterDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        [Benchmark(Baseline = true)]
        public void DirectMapping()
        {

            for (int i = 0; i < 100000; i++)
            {
                _dto.MaxTime = new NumericFilter<double> { Min = 5, Max = 10 };
                _dto.TaskName = new StringFilter { Value = "Test", MatchMode = "Contains" };
                
            }
        }


        [Benchmark]
        public void ReflectionMapping()
        {
            for (int i = 0; i < 100000; i++)
            {
                foreach (var prop in _properties)
                {
                    if (prop.Name == "MaxTime")
                    {
                        var numericFilter = new NumericFilter<double> { Min = 5, Max = 10 };
                        prop.SetValue(_dto, numericFilter);
                    }
                    else if (prop.Name == "TaskName")
                    {
                        var stringFilter = new StringFilter { Value = "Test", MatchMode = "Contains" };
                        prop.SetValue(_dto, stringFilter);
                    }
                    
                }
            }
        }
    }
            
        
   

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MapperBenchmark>();
        }
    }
}