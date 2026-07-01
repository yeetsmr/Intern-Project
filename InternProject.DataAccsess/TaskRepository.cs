using BenchmarkDotNet.Filters;
using InternProject.Core;
using InternProject.Core.Filters;
using InternProject.Core.Interfaces;
using InternProject.Core.Properties;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace InternProject.DataAccess
{
    public class TaskRepository
    {
        private readonly IMongoCollection<Tasks> _task;

        public TaskRepository(IMongoDatabase database)
        {
            _task = database.GetCollection<Tasks>("dto");
        }
        public async Task CreateTaskAsync(Tasks task)
        {

            await _task.InsertOneAsync(task);

        }

        public async Task<List<Tasks>> GetTasksAsync(FilterDto request)
        {
            var filter = BuildDynamicFilter(request);

            var query = _task.Find(filter);

            if (request.Sorts != null && request.Sorts.Count > 0)
            {
                var sortBuilder = Builders<Tasks>.Sort;
                var sortDefinitions = new List<SortDefinition<Tasks>>();

                foreach (var sort in request.Sorts)
                {
                    if (sort.SortDirection == "Ascending")
                    {
                        sortDefinitions.Add(sortBuilder.Ascending(sort.Member));
                    }
                    else
                    {
                        sortDefinitions.Add(sortBuilder.Descending(sort.Member));
                    }
                }

                query = query.Sort(sortBuilder.Combine(sortDefinitions));
            }

            return await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Limit(request.PageSize)
                .ToListAsync();
        }


        private FilterDefinition<Tasks>? BuildDynamicFilter(FilterDto dto)
        {
            var builder = Builders<Tasks>.Filter;
            var filters = new List<FilterDefinition<Tasks>>();

            var properties = dto.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var propValue = prop.GetValue(dto);
                if (propValue == null) continue;

                string fieldName = prop.Name;
                Type propType = prop.PropertyType;
                FilterDefinition<Tasks>? fieldFilter = null;

                if (propType == typeof(StringFilter))
                {
                    fieldFilter = BuildStringFilter(builder, fieldName, (StringFilter)propValue);
                }
                else if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(NumericFilter<>))
                {
                    fieldFilter = BuildNumericFilter(builder, fieldName, propValue);
                }
                else if (propType == typeof(BooleanFilter))
                {
                    fieldFilter = BuildBooleanFilter(builder, fieldName, (BooleanFilter)propValue);
                }
                else if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(EnumFilter<>))
                {
                    var selectedValuesProp = propType.GetProperty("SelectedValues");
                    var list = selectedValuesProp?.GetValue(propValue) as System.Collections.IEnumerable;

                    if (list != null)
                    {
                        var intValues = new System.Collections.Generic.List<int>();
                        foreach (var item in list)
                        {
                            intValues.Add((int)item);
                        }

                        if (intValues.Count > 0)
                        {
                            fieldFilter = builder.In(fieldName, intValues);
                        }
                    }
                }

                if (fieldFilter != null)
                {
                    filters.Add(fieldFilter);
                }
            }

            return filters.Count > 0 ? builder.And(filters) : builder.Empty;
        }


        private FilterDefinition<Tasks>? BuildStringFilter(FilterDefinitionBuilder<Tasks> builder, string fieldName, StringFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.Value)) return null;

            string pattern = filter.MatchMode == "StartsWith" ? "^" + filter.Value :
                             filter.MatchMode == "EndsWith" ? filter.Value + "$" :
                             filter.Value;

            return builder.Regex(fieldName, new BsonRegularExpression(pattern, "i"));
        }

        private FilterDefinition<Tasks>? BuildNumericFilter(FilterDefinitionBuilder<Tasks> builder, string fieldName, dynamic filter)
        {
            var filters = new List<FilterDefinition<Tasks>>();

            if (filter.Min != null)
                filters.Add(builder.Gte(fieldName, filter.Min));

            if (filter.Max != null)
                filters.Add(builder.Lte(fieldName, filter.Max));

            return filters.Count > 0 ? builder.And(filters) : null;
        }

        private FilterDefinition<Tasks>? BuildBooleanFilter(FilterDefinitionBuilder<Tasks> builder, string fieldName, BooleanFilter filter)
        {
            return builder.Eq(fieldName, filter.Value);
        }

        private FilterDefinition<Tasks>? BuildEnumFilter(FilterDefinitionBuilder<Tasks> builder, string fieldName, object filterValue, Type propType)
        {
            var selectedValuesProp = propType.GetProperty("SelectedValues");
            if (selectedValuesProp == null) return null;

            var list = selectedValuesProp.GetValue(filterValue) as System.Collections.IList;

            if (list == null || list.Count == 0) return null;
            return builder.In<object>(fieldName, list.Cast<object>());
        }
    }
}