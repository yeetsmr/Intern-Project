using InternProject.Core;
using InternProject.Core.Properties;


namespace InternProject.Business.MappingAlgoritms
{
    public class TelerikToDtoDirectMapping
    {
        public static DetailedFilterDto MapToDto(DataSourceRequest request)
        {
            var dto = new DetailedFilterDto();

            if (request.Filter?.FilterDescriptors == null) return dto;

            foreach (var descriptor in request.Filter.FilterDescriptors)
            {
                string member = descriptor.Member;
                if (string.IsNullOrEmpty(member) || descriptor.Value == null) continue;

                switch (member)
                {
                    case "EstimatedCost":
                        var costType = typeof(DetailedFilterDto).GetProperty("EstimatedCost").PropertyType;
                        var costVal = dto.EstimatedCost ?? Activator.CreateInstance(costType);
                        TelerikToDtoCetMapping.MapFilterValue(descriptor, costType, costVal);
                        dto.EstimatedCost = (dynamic)costVal;
                        break;

                    case "TaskName":
                        var nameType = typeof(DetailedFilterDto).GetProperty("TaskName").PropertyType;
                        var nameVal = dto.TaskName ?? Activator.CreateInstance(nameType);
                        TelerikToDtoCetMapping.MapFilterValue(descriptor, nameType, nameVal);
                        dto.TaskName = (dynamic)nameVal;
                        break;

                    case "IsCompleted":
                        var compType = typeof(DetailedFilterDto).GetProperty("IsCompleted").PropertyType;
                        var compVal = dto.IsCompleted ?? Activator.CreateInstance(compType);
                        TelerikToDtoCetMapping.MapFilterValue(descriptor, compType, compVal);
                        dto.IsCompleted = (dynamic)compVal;
                        break;

                    case "IsActive":
                        var activeType = typeof(DetailedFilterDto).GetProperty("IsActive").PropertyType;
                        var activeVal = dto.IsActive ?? Activator.CreateInstance(activeType);
                        TelerikToDtoCetMapping.MapFilterValue(descriptor, activeType, activeVal);
                        dto.IsActive = (dynamic)activeVal;
                        break;

                    case "StoryPoints":
                        var spType = typeof(DetailedFilterDto).GetProperty("StoryPoints").PropertyType;
                        var spVal = dto.StoryPoints ?? Activator.CreateInstance(spType);
                        TelerikToDtoCetMapping.MapFilterValue(descriptor, spType, spVal);
                        dto.StoryPoints = (dynamic)spVal;
                        break;

                    case "SprintNumber":
                        var snType = typeof(DetailedFilterDto).GetProperty("SprintNumber").PropertyType;
                        var snVal = dto.SprintNumber ?? Activator.CreateInstance(snType);
                        TelerikToDtoCetMapping.MapFilterValue(descriptor, snType, snVal);
                        dto.SprintNumber = (dynamic)snVal;
                        break;

                    case "Category":
                        var catType = typeof(DetailedFilterDto).GetProperty("Category").PropertyType;
                        var catVal = dto.Category ?? Activator.CreateInstance(catType);
                        TelerikToDtoCetMapping.MapFilterValue(descriptor, catType, catVal);
                        dto.Category = (dynamic)catVal;
                        break;

                    case "DueDate":
                        var dueType = typeof(DetailedFilterDto).GetProperty("DueDate").PropertyType;
                        var dueVal = dto.DueDate ?? Activator.CreateInstance(dueType);
                        TelerikToDtoCetMapping.MapFilterValue(descriptor, dueType, dueVal);
                        dto.DueDate = (dynamic)dueVal;
                        break;

                    case "CreatedAfter":
                        var createdType = typeof(DetailedFilterDto).GetProperty("CreatedAfter").PropertyType;
                        var createdVal = dto.CreatedAfter ?? Activator.CreateInstance(createdType);
                        TelerikToDtoCetMapping.MapFilterValue(descriptor, createdType, createdVal);
                        dto.CreatedAfter = (dynamic)createdVal;
                        break;
                }
            }

            return dto;
        }
    }
}