using InternProject.Core;
using InternProject.Business;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
        [HttpPost("filter")]
        public async Task<IActionResult> GetFilteredTasks([FromBody] FilterDto filterDto)
        {
            try
            {
                var tasks = await _taskService.GetFilteredTasksAsync(filterDto);
                return Ok(tasks);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { Messages = ex.Errors });
            }
        }
    }
}