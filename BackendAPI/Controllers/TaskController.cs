using InternProject.Business;
using InternProject.Business.MappingAlgoritms;
using InternProject.Core;
using InternProject.Core.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks")]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TaskController(TaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost("filter-dynamic")]
        public async Task<IActionResult> GetDynamicTasks([FromQuery] string? mappingMethod, [FromBody] DataSourceRequest request)
        {
            DetailedFilterDto DTO;
            switch (mappingMethod?.ToLower())
            {
                case "reflection":

                    DTO = TelerikToDtoReflectionMapping.MapToDto<DetailedFilterDto>(request);
                    break;

                case "direct":

                    DTO = TelerikToDtoDirectMapping.MapToDto(request);
                    break;

                case "cet":
                default:
                    DTO = TelerikToDtoCteMapping.MapToDto<DetailedFilterDto>(request);
                    break;
            }

            DTO.PageNumber = request.PageNumber;
            DTO.PageSize = request.PageSize;

            var tasks = await _taskService.GetDynamicFilteredTasksAsync(DTO);
            return Ok(tasks);
            //return Ok(DTO);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] Tasks task)
        {
            await _taskService.CreateTaskAsync(task);
            return Ok(task);
        }

        [AllowAnonymous]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(string id, [FromBody] Tasks task)
        {
            await _taskService.UpdateTaskAsync(id, task);
            return Ok(new { Message = "Task updated successfully." });
        }

        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(string id)
        {
            await _taskService.DeleteTaskAsync(id);
            return Ok(new { Message = "Task deleted successfully." });
        }
    }
}