using InternProject.Core;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace InternProject.Business.Mapping
{
    public static class TelerikToDTOMapper
    {
        public static FilterDto MapToDto(CompositeFilterDescriptor compositeFilter)
        {
            var dto = new FilterDto();

            if (compositeFilter?.FilterDescriptors == null)
            {
                return dto;
            }

            var properties = typeof(FilterDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var descriptor in compositeFilter.FilterDescriptors)
            {
                if (string.IsNullOrEmpty(descriptor.Member) || descriptor.Value == null)
                    continue;

                var prop = properties.FirstOrDefault(p => p.Name.Equals(descriptor.Member, StringComparison.OrdinalIgnoreCase));

                if (prop == null || !prop.CanWrite) continue;

                var propType = prop.PropertyType;

                var propValue = prop.GetValue(dto) ?? Activator.CreateInstance(propType);


                if (propType == typeof(NumericFilter))
                {
                    var numFilter = (NumericFilter)propValue;
                    double val = Convert.ToDouble(descriptor.Value.ToString());

                    if (descriptor.Operator == FilterOperator.IsGreaterThan || descriptor.Operator == FilterOperator.IsGreaterThanOrEqualTo)
                        numFilter.Min = val;
                    else if (descriptor.Operator == FilterOperator.IsLessThan || descriptor.Operator == FilterOperator.IsLessThanOrEqualTo)
                        numFilter.Max = val;
                    else if (descriptor.Operator == FilterOperator.IsEqualTo)
                    {
                        numFilter.Min = val;
                        numFilter.Max = val;
                    }
                }

                else if (propType == typeof(StringFilter))
                {
                    var strFilter = (StringFilter)propValue;
                    strFilter.Value = descriptor.Value.ToString()!;
                    strFilter.MatchMode = descriptor.Operator.ToString();
                }

                else if (propType == typeof(DateFilter))
                {
                    var dateFilter = (DateFilter)propValue;
                    DateTime val = Convert.ToDateTime(descriptor.Value.ToString());

                    if (descriptor.Operator == FilterOperator.IsGreaterThan || descriptor.Operator == FilterOperator.IsGreaterThanOrEqualTo)
                        dateFilter.StartDate = val;
                    else if (descriptor.Operator == FilterOperator.IsLessThan || descriptor.Operator == FilterOperator.IsLessThanOrEqualTo)
                        dateFilter.EndDate = val;
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