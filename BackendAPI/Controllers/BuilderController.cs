using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Reflection;

namespace BackendAPI.BuilderController
{
    public static class FilterBuilder<TDocument>
    {
        public static FilterDefinition<TDocument> Build<TFilterDto>(TFilterDto filterDto)
        {
            var filterMaker = Builders<TDocument>.Filter;
            var combinedFilter = filterMaker.Empty;

            if (filterDto == null) return combinedFilter;

            var properties = typeof(TFilterDto).GetProperties();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(filterDto);
                if (value == null) continue;

                string originalName = prop.Name;
                string dbFieldName = originalName;
                string stringMode = "Exact";


                if (originalName.EndsWith("Contains"))
                {
                    dbFieldName = originalName.Replace("Contains", "");
                    stringMode = "Contains";
                }
                else if (originalName.EndsWith("StartsWith"))
                {
                    dbFieldName = originalName.Replace("StartsWith", "");
                    stringMode = "StartsWith";
                }
                else if (originalName.EndsWith("EndsWith"))
                {
                    dbFieldName = originalName.Replace("EndsWith", "");
                    stringMode = "EndsWith";
                }

                var specificFilter = CreateTypeSpecificFilter(filterMaker, dbFieldName, prop.PropertyType, value, stringMode);
                combinedFilter = filterMaker.And(combinedFilter, specificFilter);
            }

            return combinedFilter;
        }


        private static FilterDefinition<TDocument> CreateTypeSpecificFilter(
            FilterDefinitionBuilder<TDocument> filterMaker, string fieldName, Type propType, object value, string stringMode)
        {
            if (propType == typeof(string))
                return BuildStringFilter(filterMaker, fieldName, value, stringMode); 

            if (propType == typeof(double?) || propType == typeof(double))
                return BuildDoubleFilter(filterMaker, fieldName, value);

            if (propType == typeof(DateTime?) || propType == typeof(DateTime))
                return BuildDateFilter(filterMaker, fieldName, value);

            return filterMaker.Eq(fieldName, value);
        }


        private static FilterDefinition<TDocument> BuildStringFilter(
            FilterDefinitionBuilder<TDocument> filterMaker, string fieldName, object value, string mode)
        {
            string textValue = value.ToString();


            string pattern = mode switch
            {
                "StartsWith" => $"^{textValue}", 
                "EndsWith" => $"{textValue}$",  
                "Contains" => textValue,       
                _ => null                        
            };


            if (pattern == null)
                return filterMaker.Eq(fieldName, textValue);


            return filterMaker.Regex(fieldName, new BsonRegularExpression(pattern, "i"));
        }

        private static FilterDefinition<TDocument> BuildDoubleFilter(
            FilterDefinitionBuilder<TDocument> filterMaker, string fieldName, object value)
        {
            return filterMaker.Lte(fieldName, Convert.ToDouble(value));
        }

        private static FilterDefinition<TDocument> BuildDateFilter(
            FilterDefinitionBuilder<TDocument> filterMaker, string fieldName, object value)
        {
            return filterMaker.Gt(fieldName, (DateTime)value);
        }
    }
}