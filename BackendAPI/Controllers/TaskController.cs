using InternProject.Business;
using InternProject.Business.Mapping;
using InternProject.Core;
using InternProject.Core.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

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
        public async Task<IActionResult> GetDynamicTasks([FromBody] DataSourceRequest request)
        {
            var DTO = TelerikToDTOMapper.MapToDto<DetailedFilterDto>(request);

            DTO.PageNumber = request.PageNumber;
            DTO.PageSize = request.PageSize;

            var tasks = await _taskService.GetDynamicFilteredTasksAsync(DTO);
            return Ok(tasks);
        }

        [AllowAnonymous] 
        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] Tasks task)
        {
      
            await _taskService.CreateTaskAsync(task);

            return Ok(new { Message = "Task created successfully." });
        }
    }
}
