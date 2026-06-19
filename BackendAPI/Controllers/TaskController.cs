using BackendAPI.Helpers;
using InternProject.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class TasksController : ControllerBase
    {
        private readonly IMongoCollection<task> _taskCollection;
        public TasksController()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("secondweek");
            _taskCollection = database.GetCollection<task>("dto");
        }
        [HttpPost("filter")]
        public async Task<IActionResult> GetFilteredTasks([FromBody] FilterDto filterDto)
        {

            var combinedFilter = FilterBuilder<task>.Build(filterDto);

            var tasks = await _taskCollection.Find(combinedFilter).ToListAsync();

            return Ok(tasks);
        }
        [HttpPost]
        public async Task<ActionResult> CreateTask([FromBody] task newTask)
        {

            if (newTask.CreatedAfter == default)
            {
                newTask.CreatedAfter = System.DateTime.UtcNow;
            }
            await _taskCollection.InsertOneAsync(newTask);
            return Ok(new { message = "Task created successfully", id = newTask.Id });
        }
        [HttpPut("{TaskName}")]
        public async Task<ActionResult> UpdateTask(string TaskName, [FromBody] task updatedTask)
        {

            var filter = Builders<task>.Filter.Eq(x => x.TaskName, TaskName);
            var update = Builders<task>.Update
                .Set(x => x.TaskName, updatedTask.TaskName)
                .Set(x => x.MaxTime, updatedTask.MaxTime)
                .Set(x => x.IsCompleted, updatedTask.IsCompleted)
                .Set(x => x.pri, updatedTask.pri)
                .Set(x => x.CreatedAfter, updatedTask.CreatedAfter);
            var result = await _taskCollection.UpdateOneAsync(filter, update);
            await _taskCollection.UpdateOneAsync(filter, update);
            return Ok(new { message = "Task updated successfully", count = result.ModifiedCount });
        }

        [HttpDelete("{TaskName}")]
        public async Task<ActionResult> DeleteTask(string TaskName)
        {

            var filterDelete = Builders<task>.Filter.Eq(x => x.TaskName, TaskName);
            var result = await _taskCollection.DeleteOneAsync(filterDelete);
            await _taskCollection.DeleteOneAsync(filterDelete);
            return Ok(new { message = "Task deleted successfully", count = result.DeletedCount });
        }
    }
}

