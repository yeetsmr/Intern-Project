using FluentValidation;
using InternProject.Core;
using InternProject.DataAccess;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

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


        public async Task<List<Tasks>> GetDynamicFilteredTasksAsync(FilterDto request)
        {
            return await _taskRepository.GetTasksAsync(request);
        }
    }
}