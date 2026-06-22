using FluentValidation; 
using InternProject.Core;
using InternProject.DataAccess;
using InternProject.Business.Validation; 
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternProject.Business
{
    public class TaskService
    {
        private readonly TaskRepository _taskRepository;
        private readonly ILogger<TaskService> _logger;

        public TaskService(TaskRepository taskRepository, ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository;
            _logger = logger;
        }

        public async Task<List<task>> GetFilteredTasksAsync(FilterDto filterDto)
        {
            _logger.LogInformation("Filtering demands received");


            var validator = new FilterDtoValidator();
            var validationResult = await validator.ValidateAsync(filterDto);

            if (!validationResult.IsValid)
            {

                foreach (var failure in validationResult.Errors)
                {
                    _logger.LogWarning("Validation failed! Field: {Property} | Error: {Message}",
                        failure.PropertyName, failure.ErrorMessage);
                }


                throw new ValidationException(validationResult.Errors);
            }

            return await _taskRepository.GetFilteredTasksAsync(filterDto);
        }
    }
}