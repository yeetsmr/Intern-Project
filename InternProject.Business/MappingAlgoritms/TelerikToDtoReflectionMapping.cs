using InternProject.Core;
using InternProject.Core.Filters;
using InternProject.Core.Interfaces;
using InternProject.Core.Properties;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace InternProject.Business.MappingAlgoritms
{
    public static class TelerikToDtoReflectionMapping
    {
        public static T MapToDto<T>(DataSourceRequest request) where T : new()
        {
            var dto = new T();
            var compositeFilter = request.Filter as CompositeFilterDescriptor;

            if (compositeFilter?.FilterDescriptors == null)
            {
                return dto;
            }

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var descriptor in compositeFilter.FilterDescriptors)
            {
                if (string.IsNullOrEmpty(descriptor.Member) || descriptor.Value == null)
                    continue;

                var prop = properties.FirstOrDefault(p => p.Name.Equals(descriptor.Member, StringComparison.OrdinalIgnoreCase));

                if (prop == null || !prop.CanWrite) continue;

                var propType = prop.PropertyType;

                var propValue = prop.GetValue(dto) ?? Activator.CreateInstance(propType);

                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(NumericFilter<>))
                {
                    Type targetType = propType.GetGenericArguments()[0];
                    object val = Convert.ChangeType(descriptor.Value.ToString(), targetType);
                    var rangeFilter = (RangeInterface)propValue;

                    if (descriptor.Operator == FilterOperator.IsGreaterThan || descriptor.Operator == FilterOperator.IsGreaterThanOrEqualTo)
                    {
                        rangeFilter.SetMin(val);
                    }
                    else if (descriptor.Operator == FilterOperator.IsLessThan || descriptor.Operator == FilterOperator.IsLessThanOrEqualTo)
                    {
                        rangeFilter.SetMax(val);
                    }
                    else if (descriptor.Operator == FilterOperator.IsEqualTo)
                    {
                        rangeFilter.SetMin(val);
                        rangeFilter.SetMax(val);
                    }
                }
                else if (propType == typeof(StringFilter))
                {
                    var strFilter = (StringFilter)propValue;
                    strFilter.Value = descriptor.Value.ToString()!;
                    strFilter.MatchMode = descriptor.Operator.ToString();
                }
                else if (propType == typeof(BooleanFilter))
                {
                    var boolFilter = (BooleanFilter)propValue;
                    boolFilter.Value = Convert.ToBoolean(descriptor.Value.ToString());
                }
                else if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(EnumFilter<>))
                {
                    Type enumType = propType.GetGenericArguments()[0];
                    var selectedValuesProp = propType.GetProperty("SelectedValues");
                    var list = selectedValuesProp!.GetValue(propValue) as System.Collections.IList;

                    if (descriptor.Operator == FilterOperator.IsContainedIn && descriptor.Value is JsonElement jsonArray && jsonArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in jsonArray.EnumerateArray())
                        {
                            if (Enum.TryParse(enumType, item.ToString(), true, out var enumVal))
                                list!.Add(enumVal);
                        }
                    }
                    else
                    {
                        if (Enum.TryParse(enumType, descriptor.Value.ToString(), true, out var singleEnum))
                            list!.Add(singleEnum);
                    }
                }

                prop.SetValue(dto, propValue);
            }

            return dto;
        }
    }
}