using InternProject.Core;
using InternProject.Core.Filters;
using InternProject.Core.Interfaces;
using InternProject.Core.Properties;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace InternProject.Business.Mapping
{
    public class TelerikToDTOMapper
    {
        private static class DelegateCache<TDto> where TDto : class
        {
            public static readonly PropertyInfo[] Properties = typeof(TDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            public static readonly ConcurrentDictionary<string, Action<TDto, object>> Setters =
                new ConcurrentDictionary<string, Action<TDto, object>>(StringComparer.OrdinalIgnoreCase);
        }

        public static TDto MapToDto<TDto>(DataSourceRequest request) where TDto : class, new()
        {
            var targetDto = new TDto();
            var dtoProperties = DelegateCache<TDto>.Properties;

            ProcessSorts(request, targetDto);

            if (request.Filter?.FilterDescriptors == null) return targetDto;

            foreach (var descriptor in request.Filter.FilterDescriptors)
            {
                if (string.IsNullOrEmpty(descriptor.Member) || descriptor.Value == null) continue;

                var prop = dtoProperties.FirstOrDefault(p => p.Name.Equals(descriptor.Member, StringComparison.OrdinalIgnoreCase));
                if (prop == null || !prop.CanWrite) continue;

                var propValue = prop.GetValue(targetDto) ?? Activator.CreateInstance(prop.PropertyType);
                MapFilterValue(descriptor, prop.PropertyType, propValue);

                var setterDelegate = DelegateCache<TDto>.Setters.GetOrAdd(prop.Name, _ => CreateSetter<TDto>(prop));
                setterDelegate(targetDto, propValue);
            }

            return targetDto;
        }

        private static Action<TDto, object> CreateSetter<TDto>(PropertyInfo propertyInfo)
        {
            var targetExp = Expression.Parameter(typeof(TDto), "target");
            var valueExp = Expression.Parameter(typeof(object), "value");
            var castValueExp = Expression.Convert(valueExp, propertyInfo.PropertyType);
            var propertyExp = Expression.Property(targetExp, propertyInfo);
            var assignExp = Expression.Assign(propertyExp, castValueExp);
            return Expression.Lambda<Action<TDto, object>>(assignExp, targetExp, valueExp).Compile();
        }

        private static void ProcessSorts<TDto>(DataSourceRequest request, TDto targetDto) where TDto : class
        {
            if (targetDto is FilterDto baseFilterDto && request.Sorts != null)
            {
                foreach (var sort in request.Sorts)
                {
                    baseFilterDto.Sorts.Add(new SortRule { Member = sort.Member, SortDirection = sort.SortDirection });
                }
            }
        }

        private static void MapFilterValue(FilterDescriptor descriptor, Type propType, object propValue)
        {
            if (propValue is RangeInterface rangeFilter)
            {
                ProcessRangeFilter(descriptor, propType, rangeFilter);
            }
            else if (propType == typeof(StringFilter))
            {
                var strFilter = (StringFilter)propValue;
                strFilter.Value = descriptor.Value?.ToString() ?? string.Empty;
                strFilter.MatchMode = descriptor.Operator.ToString();
            }
            else if (propType == typeof(BooleanFilter))
            {
                var boolFilter = (BooleanFilter)propValue;
                boolFilter.Value = (bool)ParseValue(descriptor.Value, typeof(bool));
            }
            else if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(EnumFilter<>))
            {
                ProcessEnumFilter(descriptor, propType, propValue);
            }
        }

        private static void ProcessRangeFilter(FilterDescriptor descriptor, Type propType, RangeInterface rangeFilter)
        {
            Type numericType = propType.GetGenericArguments()[0];
            var convertedValue = ParseValue(descriptor.Value, numericType);

            if (descriptor.Operator == FilterOperator.IsGreaterThan || descriptor.Operator == FilterOperator.IsGreaterThanOrEqualTo)
                rangeFilter.SetMin(convertedValue);
            else if (descriptor.Operator == FilterOperator.IsLessThan || descriptor.Operator == FilterOperator.IsLessThanOrEqualTo)
                rangeFilter.SetMax(convertedValue);
            else if (descriptor.Operator == FilterOperator.IsEqualTo)
            {
                rangeFilter.SetMin(convertedValue);
                rangeFilter.SetMax(convertedValue);
            }
        }

        private static void ProcessEnumFilter(FilterDescriptor descriptor, Type propType, object propValue)
        {
            Type enumType = propType.GetGenericArguments()[0];
            var selectedValuesProp = propType.GetProperty("SelectedValues");
            var list = selectedValuesProp?.GetValue(propValue) as IList;

            if (list == null)
            {
                Type listType = typeof(List<>).MakeGenericType(enumType);
                list = (IList)Activator.CreateInstance(listType);
                selectedValuesProp?.SetValue(propValue, list);
            }

            string valueStr = descriptor.Value is JsonElement je ? je.ToString() : descriptor.Value?.ToString() ?? string.Empty;
            if (Enum.TryParse(enumType, valueStr, true, out object parsedEnum))
            {
                list.Add(parsedEnum);
            }
        }

        private static object ParseValue(object value, Type targetType)
        {
            if (value is JsonElement jsonElement)
                return jsonElement.Deserialize(targetType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Convert.ChangeType(value, targetType);
        }
    }
}