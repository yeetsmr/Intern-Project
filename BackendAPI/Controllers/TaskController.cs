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
        public async Task<ActionResult<List<task>>> GetFilteredTasks([FromBody] FilterDto filterDto)
        {
            var filterMaker = Builders<task>.Filter;
            var combinedFilter = filterMaker.Empty;

            if (filterDto != null)
            {

                var properties = filterDto.GetType().GetProperties();

                foreach (var prop in properties)
                {

                    var value = prop.GetValue(filterDto);


                    if (value == null) continue;


                    string dbFieldName = prop.Name;
                    if (dbFieldName.EndsWith("Contains")) dbFieldName = dbFieldName.Replace("Contains", "");

                    if (prop.PropertyType == typeof(string))
                    {

                        var regexFilter = filterMaker.Regex(dbFieldName, new MongoDB.Bson.BsonRegularExpression(value.ToString(), "i"));
                        combinedFilter = filterMaker.And(combinedFilter, regexFilter);
                    }
                    else if (prop.PropertyType == typeof(double?) || prop.PropertyType == typeof(double))
                    {

                        var timeFilter = filterMaker.Lte(dbFieldName, Convert.ToDouble(value));
                        combinedFilter = filterMaker.And(combinedFilter, timeFilter);
                    }
                    else if (prop.PropertyType == typeof(DateTime?) || prop.PropertyType == typeof(DateTime))
                    {

                        var dateFilter = filterMaker.Gt(dbFieldName, (DateTime)value);
                        combinedFilter = filterMaker.And(combinedFilter, dateFilter);
                    }
                    else
                    {

                        var eqFilter = filterMaker.Eq(dbFieldName, value);
                        combinedFilter = filterMaker.And(combinedFilter, eqFilter);


                    }
                }
            }
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

