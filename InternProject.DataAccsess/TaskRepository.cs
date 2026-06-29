using InternProject.Core;
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


        private FilterDefinition<Tasks> BuildDynamicFilter(FilterDto dto)
        {
            var builder = Builders<Tasks>.Filter;
            var filters = new List<FilterDefinition<Tasks>>();

            if (dto == null)
                return builder.Empty;

            var properties = typeof(FilterDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var propValue = prop.GetValue(dto);

                if (propValue == null)
                    continue;

                string fieldName = prop.Name;
                Type propType = prop.PropertyType;

                FilterDefinition<Tasks>? fieldFilter = null;

                if (propType == typeof(StringFilter))
                {
                    fieldFilter = BuildStringFilter(builder, fieldName, (StringFilter)propValue);
                }
                else if (propType == typeof(NumericFilter))
                {
                    fieldFilter = BuildNumericFilter(builder, fieldName, (NumericFilter)propValue);
                }
                else if (propType == typeof(BooleanFilter))
                {
                    fieldFilter = BuildBooleanFilter(builder, fieldName, (BooleanFilter)propValue);
                }
                else if (propType == typeof(DateFilter))
                {
                    fieldFilter = BuildDateFilter(builder, fieldName, (DateFilter)propValue);
                }
                else if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(EnumFilter<>))
                {
                    fieldFilter = BuildEnumFilter(builder, fieldName, propValue, propType);
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

        private FilterDefinition<Tasks>? BuildNumericFilter(FilterDefinitionBuilder<Tasks> builder, string fieldName, NumericFilter filter)
        {
            var numFilters = new List<FilterDefinition<Tasks>>();

            if (filter.Min.HasValue)
                numFilters.Add(builder.Gte(fieldName, filter.Min.Value));

            if (filter.Max.HasValue)
                numFilters.Add(builder.Lte(fieldName, filter.Max.Value));

            if (numFilters.Count == 0) return null;
            if (numFilters.Count == 1) return numFilters[0];

            return builder.And(numFilters);
        }

        private FilterDefinition<Tasks>? BuildDateFilter(FilterDefinitionBuilder<Tasks> builder, string fieldName, DateFilter filter)
        {
            var dateFilters = new List<FilterDefinition<Tasks>>();

            if (filter.StartDate.HasValue)
                dateFilters.Add(builder.Gte(fieldName, filter.StartDate.Value));

            if (filter.EndDate.HasValue)
                dateFilters.Add(builder.Lte(fieldName, filter.EndDate.Value));

            if (dateFilters.Count == 0) return null;
            if (dateFilters.Count == 1) return dateFilters[0];

            return builder.And(dateFilters);
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