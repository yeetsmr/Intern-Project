using InternProject.Core;
using InternProject.Core.Filters;
using InternProject.Core.Interfaces;
using InternProject.Core.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace InternProject.Business.MappingAlgoritms
{
    public class TelerikToDtoCetMapping
    {
        public class PropertyAccessor<TDto>
        {
            public Type PropertyType { get; set; }
            public Func<TDto, object> Getter { get; set; }
            public Action<TDto, object> Setter { get; set; }
            public Func<object> Instantiator { get; set; }
        }

        public static class DelegateCache<TDto> where TDto : class
        {
            public static readonly Dictionary<string, PropertyAccessor<TDto>> Accessors;

            static DelegateCache()
            {
                Accessors = new Dictionary<string, PropertyAccessor<TDto>>(StringComparer.OrdinalIgnoreCase);
                var properties = typeof(TDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in properties)
                {
                    if (!prop.CanWrite) continue;

                    Accessors[prop.Name] = new PropertyAccessor<TDto>
                    {
                        PropertyType = prop.PropertyType,
                        Getter = CreateGetter(prop),
                        Setter = CreateSetter(prop),
                        Instantiator = CreateInstantiator(prop.PropertyType)
                    };
                }
            }

            private static Func<TDto, object> CreateGetter(PropertyInfo prop)
            {
                var targetExp = Expression.Parameter(typeof(TDto), "target");
                var propExp = Expression.Property(targetExp, prop);
                var castExp = Expression.Convert(propExp, typeof(object));
                return Expression.Lambda<Func<TDto, object>>(castExp, targetExp).Compile();
            }

            private static Action<TDto, object> CreateSetter(PropertyInfo prop)
            {
                var targetExp = Expression.Parameter(typeof(TDto), "target");
                var valueExp = Expression.Parameter(typeof(object), "value");
                var castValueExp = Expression.Convert(valueExp, prop.PropertyType);
                var propExp = Expression.Property(targetExp, prop);
                var assignExp = Expression.Assign(propExp, castValueExp);
                return Expression.Lambda<Action<TDto, object>>(assignExp, targetExp, valueExp).Compile();
            }

            private static Func<object> CreateInstantiator(Type type)
            {
                if (type == typeof(string)) return () => string.Empty;
                var ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor == null) return () => Activator.CreateInstance(type);

                return Expression.Lambda<Func<object>>(Expression.New(ctor)).Compile();
            }
        }

        public static TDto MapToDto<TDto>(DataSourceRequest request) where TDto : class, new()
        {
            var targetDto = new TDto();

            ProcessSorts(request, targetDto);

            if (request.Filter?.FilterDescriptors == null) return targetDto;

            foreach (var descriptor in request.Filter.FilterDescriptors)
            {
                if (string.IsNullOrEmpty(descriptor.Member) || descriptor.Value == null) continue;
                if (!DelegateCache<TDto>.Accessors.TryGetValue(descriptor.Member, out var accessor)) continue;

                var propValue = accessor.Getter(targetDto) ?? accessor.Instantiator();

                MapFilterValue(descriptor, accessor.PropertyType, propValue);
                accessor.Setter(targetDto, propValue);
            }

            return targetDto;
        }

        public static void ProcessSorts<TDto>(DataSourceRequest request, TDto targetDto) where TDto : class
        {
            if (targetDto is FilterDto baseFilterDto && request.Sorts != null)
            {
                foreach (var sort in request.Sorts)
                {
                    baseFilterDto.Sorts.Add(new SortRule { Member = sort.Member, SortDirection = sort.SortDirection });
                }
            }
        }

        public static void MapFilterValue(FilterDescriptor descriptor, Type propType, object propValue)
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

        public static void ProcessRangeFilter(FilterDescriptor descriptor, Type propType, RangeInterface rangeFilter)
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

        public static void ProcessEnumFilter(FilterDescriptor descriptor, Type propType, object propValue)
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

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public static object ParseValue(object value, Type targetType)
        {
            if (value is JsonElement jsonElement)
                return jsonElement.Deserialize(targetType, _jsonOptions);

            return Convert.ChangeType(value, targetType);
        }
    }
}