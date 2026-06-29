using InternProject.Business;
using InternProject.Business.Mapping;
using InternProject.Core;
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
        public async Task<IActionResult> GetDynamicTasks([FromBody] DataSourceRequest request)
        {
            FilterDto myCustomDto = TelerikToDTOMapper.MapToDto(request.Filter);

            myCustomDto.PageNumber = request.PageNumber;
            myCustomDto.PageSize = request.PageSize;


            var tasks = await _taskService.GetDynamicFilteredTasksAsync(myCustomDto);

            return Ok(tasks);
        }
    }
}