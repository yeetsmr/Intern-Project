using InternProject.Core.Properties;
using InternProject.DataAccess;
using Microsoft.Extensions.Logging;

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

        public async Task CreateTaskAsync(Tasks task)
        {
            await _taskRepository.CreateTaskAsync(task);
        }


        public async Task UpdateTaskAsync(string id, Tasks task)
        {
            await _taskRepository.UpdateTaskAsync(id, task);
            _logger.LogInformation($"Task with ID {id} was updated.");
        }

        public async Task DeleteTaskAsync(string id)
        {
            await _taskRepository.DeleteTaskAsync(id);
            _logger.LogInformation($"Task with ID {id} was deleted.");
        }
    }
}